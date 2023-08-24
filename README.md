# Specky6
Lightweight wrapper to assist injection, using attributes, using the built in DI model integrated in .NET 6 and up.

## Required at builder.Services to add Specks (may include other assemblies)

`builder.Services.AddSpecks();`

## Examples (If you're familiar with .NET's built in injection then the naming here should be straight forward.)

### Transient, Scoped, or Singleton attributes inject the type as the implementation. 

`[Transient]` will inject the type as transient.

`[Scoped]` will inject the type as scoped.

`[Singleton]` will inject the type as singleton.

```
[Scoped]
public class MyClass { }
```
> Is the same as:
```
public class MyClass { }

builder.Services.AddScoped<MyClass>();
```

### TransientAs, ScopedAs, or SingletonAs attributes inject the implementation as the interface provided in the attribute.

`[TransientAs(Type)]` will inject the `TransientAsAttribute.Type` to be implemented by the `Type` the `TransientAsAttribute` is given to. `Type` must inherit from the `TransientAsAttribute.Type`

`[ScopedAs(Type)]` will inject the `ScopedAsAttribute.Type` to be implemented by the `Type` the `ScopedAsAttribute` is given to. `Type` must inherit from the `ScopedAsAttribute.Type`.

`[SingletonAs(Type)]` will inject the `SingletonAsAttribute.Type` to be implemented by the `Type` the `SingletonAsAttribute` is given to. `Type` must inherit from the `SingletonAsAttribute.Type`.

> Example:
```
public interface IWorker

[ScopedAs(typeof(IWorker)]
public class Worker : IWorker
```
> Is the same as:
```
public interface IWorker

public class Worker : IWorker

builder.Services.AddScoped<IWorker, Worker>();
```

## Transient
Transient will inject a new instance for every request.

`[Transient] public class MyClass { }` or `[TransientAs(typeof(IMyClass))] public class MyClass : IMyClass { }`

## Scoped
Scoped will inject a new instance for every session, typically, in a web app for example, this is each time the Http connection is reset.

`[Scoped] public class MyClass { }` or `[ScopedAs(typeof(IMyClass))] public class MyClass : IMyClass { }` 

## Singleton
Singleton will inject a the same instance for the lifetime of the application.

`[Singleton] public class MyClass { }` or `[SingletonAs(typeof(IMyClass))] public class MyClass : IMyClass { }`

## Using Speck attributes across multiple projects / assemblies
In order to scan for specks across multiple assemblies you need to pass those assemblies to the AddSpecks method.
Note: Don't forget to include the assembly you are working in, assuming you will have Specks there also.

```
builder.Services.AddSpecks(new []
{
    typeof(Program).Assembly,
    typeof(MyProject2.SomeNamespace.SomeType).Assembly,
    typeof(MyProject3.AnotherNamespace.IInterfaceForSomething).Assembly
});
```

# Using Configurations
With Specky you can create an interface for injecting types in a single file.

To use configurations you will need to first make any `interface` and then add the `SpeckyConfigurationAttribute` to that `interface`.

Next add the properties or methods with the proper injection attribute.

Note: The `SpeckyConfiguration` does not get injected.  It is used as a lookup for other types only and happens at startup. This is not to be confused or considered as a factory or any other magic utility. This simply saves you from having to type `builder.Services.AddScoped<MyClass>();` and form having to place a `SpeckAttribute` on any classes.  It lets you keep the configuration one or more files to make it easier to navigate and modify.

> The following will inject the `StartUp` type as a `Singleton`, the `TraceLogger` as `Transient` abstracted by `ILogger`, and the `Worker` as `Scoped` abstracted by the `IWorker` interface.
```
[SpeckyConfiguration]
interface ISpeckyConfiguration
{
    [Singleton] StartUp GetStartUp();        
    [TransientAs(typeof(ILogger))] TraceLogger GetLog();
    [ScopedAs(typeof(IWorker))] Worker GetWorker();
}
```

# Full example of using Specky configurations:
### Note you do not have to put attributes on the types themselves or modify the StartUp.cs, program.cs, or any files where you're declaring DI.

Program.cs

    using Microsoft.Extensions.Hosting;
    using Specky6;

    using IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((_, services) => services.AddSpecks())
        .Build();

    ((StartUp)host.Services.GetService(typeof(StartUp))!).Start();

### StartUp.cs

    public class StartUp
    {
        private readonly IWorker worker;
        private readonly ILogger loger;

        public StartUp(IWorker worker, ILogger logger)
        {
            this.worker = worker;
            this.logger = logger;
        }

        public void Start()
        {
            logger.Log("Start");
            worker.DoWork(() => Console.WriteLine("App has started"));
            logger.Log("End");
        }
    }

### Logging interface and implementations:

    public interface ILogger
    {
        void Log(string message);
    }

    public class TraceLogger : ILogger
    {
        public void Log(string message) => Trace.WriteLine(message);
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message) => Console.WriteLine(message);
    }

### Worker interface and implementation:
    public interface IWorker
    {
        void DoWork(Action action);
    }

    public class Worker : IWorker
    {
        public void DoWork(Action action) => action.Invoke();
    }

### The specky configuration file:
    using Specky6;
    
    /* 
     * Note: Specky configurations do not get injected. 
     * The interface is used as a reference for Specky to locate and inject types.
     * You can use properties or methods. 
     */
    [SpeckyConfiguration]
    interface ISpeckyConfiguration
    {
        /* 
         * Method examples:
         * Note: the method return type is what gets implemented.
         * Replace ConsoleLog with TraceLog to inject TraceLog.
         * Method names to not matter. 
         */
        [SingletonAs(typeof(ILogger))] ConsoleLogger GetLog();
        [Singleton] StartUp GetStartUp();
    
        /* 
         * Property example:
         * Note: the property type is what gets implemented.
         * Property names do not matter. 
         */
        [SingletonAs(typeof(IWorker))] Worker Worker { get; }
    }
