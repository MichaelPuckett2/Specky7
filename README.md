# Specky6
Lightweight wrapper to assist injection, using attributes, using the built in DI model integrated in .NET 6 and up.

## Required at builder.Services to add Specks

    builder.Services.AddSpecks();

## Examples (If you're familiar with .NET's built in injection then the naming here should be straight forward.)

### Transient, Scoped, or Singleton attributes inject the type as the implementation. 

-   Example: builder.Services.AddScoped<MyClass>();

### TransientAs, ScopedAs, or SingletonAs attributes inject the implementation as the interface provided in the attribute.

-   Example: Services.AddScoped<IMyClass, MyClass>();

## Transient
Transient will inject a new instance for every request.

    [Transient]
    public class MyClass { ... }

    // Or

    [TransientAs(typeof(IMyClass))]
    public class MyClass : IMyClass { ... }

## Scoped
Scoped will inject a new instance for every session, typically, in a web app for example, this is each time the Http connection is reset.

    [Scoped]
    public class MyClass { ... }
    
    // Or

    [ScopedAs(typeof(IMyClass))]
    public class MyClass : IMyClass { ... }  

## Singleton
Singleton will inject a the same instance for the lifetime of the application.

    [Scoped]
    public class MyClass { ... }    

    // Or

    [ScopedAs(typeof(IMyClass))]
    public class MyClass : IMyClass { ... }

## Using Speck attributes across multiple projects / assemblies
In order to scan for specks across multiple assemblies you need to pass those assemblies to the AddSpecks method.
Note: Don't forget to include the assembly you are working in, assuming you will have Specks there also.

    builder.Services.AddSpecks(new []
    {
        typeof(Program).Assembly,
        typeof(MyProject2.SomeNamespace.SomeType).Assembly,
        typeof(MyProject3.AnotherNamespace.IInterfaceForSomething).Assembly
    });

# Using Configurations
With Specky you can create an interface for injecting types in a single file.

To use configurations you will need to first make any interface and simply then add the SpeckyConfigurationAttribute to that interface.

Next add the properties or methods with the proper injection attribute.

Note: Specky configurations do not get injected. The interface is used as a reference for Specky to locate and inject types.


    [SpeckyConfiguration]
    interface ISpeckyConfiguration
    {
        [Singleton] StartUp GetStartUp();
        [SingletonAs(typeof(ILog))] TraceLog GetLog();
        [SingletonAs(typeof(IWorker))] Worker GetWorker();
    }

# Full example of using Specky configurations:

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
        private readonly ILog log;

        public StartUp(IWorker worker, ILog log)
        {
            this.worker = worker;
            this.log = log;
        }

        public void Start()
        {
            log.Log("Start");
            worker.DoWork(() => Console.WriteLine("App has started"));
            log.Log("End");
        }
    }

### Logging interface and implementations:

    public interface ILog
    {
        void Log(string message);
    }

    public class TraceLog : ILog
    {
        public void Log(string message) => Trace.WriteLine(message);
    }

    public class ConsoleLog : ILog
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
        [SingletonAs(typeof(ILog))] ConsoleLog GetLog();
        [Singleton] StartUp GetStartUp();
    
        /* 
         * Property example:
         * Note: the property type is what gets implemented.
         * Property names to not matter. 
         */
        [SingletonAs(typeof(IWorker))] Worker Worker { get; }
    }