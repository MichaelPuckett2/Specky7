using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Specky7.Tests;

[TestClass()]
public class ExtensionsTests
{
    [TestMethod()]
    public void AddInvalidConfigurationTest()
    {
        //Arrange
        IServiceCollection serviceProvider = new MockServiceCollecton();

        //Act and Assert
        Assert.ThrowsException<TypeAccessException>(() =>
        {
            serviceProvider.AddSpecks(opts =>
            {
                opts.AddConfiguration<IInvalidConfiguration>();
            });
        });
    }

    [TestMethod()]
    public void AddInvalidOptionTest()
    {
        //Arrange
        IServiceCollection serviceProvider = new MockServiceCollecton();

        //Act and Assert
        Assert.ThrowsException<TypeAccessException>(() =>
        {
            serviceProvider.AddSpecks(opts =>
            {
                opts.AddConfiguration<IInvalidConfiguration>();
                opts.AddConfiguration<IOkConfiguration>();
                opts.AddOptions("Invalid");
            });
        });
    }

    [TestMethod()]
    public void AddOkOptionTest()
    {
        //Arrange
        IServiceCollection serviceProvider = new MockServiceCollecton();

        //Act
        serviceProvider.AddSpecks(opts =>
        {
            opts.AddConfiguration<IInvalidConfiguration>();
            opts.AddConfiguration<IOkConfiguration>();
            opts.AddConfiguration<IOk2Configuration>();
            opts.AddOptions("Ok");
            opts.AddOptions("Ok2");
        });

        //Assert
        Assert.AreEqual(4, serviceProvider.Count);
    }

    [TestMethod()]
    public void AddSpecksScanningTest()
    {
        //Arrange
        IServiceCollection serviceProvider = new MockServiceCollecton();

        //Act
        serviceProvider.AddSpecks(opts =>
        {
            opts.AddAssemblies(typeof(ExtensionsTests).Assembly);
        });

        //Assert
        Assert.AreEqual(4, serviceProvider.Count);
        var a = serviceProvider.Any(x => x.ServiceType == typeof(IFooTime) 
        && x.ImplementationType == typeof(B_Foo) 
        && x.Lifetime == ServiceLifetime.Singleton);

        var b = serviceProvider.Any(x => x.ServiceType == typeof(IFooId) 
        && x.ImplementationType == typeof(B_Foo) 
        && x.Lifetime == ServiceLifetime.Scoped);

        var c = serviceProvider.Any(x => x.ServiceType == typeof(A_FooTime) 
        && x.ImplementationType == typeof(A_FooTime) 
        && x.Lifetime == ServiceLifetime.Singleton);

        var d = serviceProvider.Any(x => x.ServiceType == typeof(B_FooTime) 
        && x.ImplementationType == typeof(B_FooTime) 
        && x.Lifetime == ServiceLifetime.Transient);

        Assert.IsTrue(a);
        Assert.IsTrue(b);
        Assert.IsTrue(c);
        Assert.IsTrue(d);
    }
}