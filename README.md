# Specky6
Lightweight wrapper to assist injection, using attributes, using the built in DI model integrated in .NET 6 and up.

## Required at builder.Services to add Specks

    builder.Services.AddSpecks();

## Examples (If you're familiar with .NET's built in injection then the naming here should be straight forward.)
## Transient
Transient will inject a new instance for every request.

    [Transient]
    public class MyClass { ... }

    // Equivalent to builder.Services.AddTransient<MyClass>();

    [TransientAs(typeof(IMyClass))]
    public class MyClass : IMyClass { ... }

    // Equivalent to builder.Services.AddTransient<IMyClass, MyClass>();
## Scoped
Scoped will inject a new instance for every session, typically, in a web app for example, this is each time the Http connection is reset.

    [Scoped]
    public class MyClass { ... }

    // Equivalent to builder.Services.AddScoped<MyClass>();

    [ScopedAs(typeof(IMyClass))]
    public class MyClass : IMyClass { ... }  

    // Equivalent to builder.Services.AddScoped<IMyClass, MyClass>();
## Singleton
Singleton will inject a the same instance for the lifetime of the application.


    [Scoped]
    public class MyClass { ... }    

    // Equivalent to builder.Services.AddSingleton<MyClass>();

    [ScopedAs(typeof(IMyClass))]
    public class MyClass : IMyClass { ... }

    // Equivalent to builder.Services.AddSingleton<IMyClass, MyClass>();


## Using Speck attributes across multiple projects / assemblies
In order to scan for specks across multiple assemblies you need to pass those assemblies to the AddSpecks method.
Note: Don't forget to include the assembly you are working in, assuming you will have Specks there also.

    builder.Services.AddSpecks(new []
    {
        typeof(Program).Assembly,
        typeof(MyProject2.SomeNamespace.SomeType).Assembly,
        typeof(MyProject3.AnotherNamespace.IInterfaceForSomething).Assembly
    });
