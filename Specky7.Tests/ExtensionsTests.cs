using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Specky7.Tests;

[TestClass()]
public class ExtensionsTests
{
    [TestMethod()]
    public void AddSpecksTest()
    {
        //Arrange
        IServiceCollection serviceProvider = new MockServiceCollecton();

        //Act and Assert
        Assert.ThrowsException<TypeAccessException>(() => serviceProvider.AddSpecks());
    }
}

internal class MockServiceCollecton : IServiceCollection
{
    readonly List<ServiceDescriptor> services = new();
    public ServiceDescriptor this[int index] { get => services[index]; set => services[index] = value; }
    public int Count => services.Count;
    public bool IsReadOnly => true;
    public void Add(ServiceDescriptor item) => services.Add(item);
    public void Clear() => services.Clear();
    public bool Contains(ServiceDescriptor item) => services.Contains(item);
    public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => services.CopyTo(array, arrayIndex);
    public IEnumerator<ServiceDescriptor> GetEnumerator() => services.GetEnumerator();
    public int IndexOf(ServiceDescriptor item) => services.IndexOf(item);
    public void Insert(int index, ServiceDescriptor item) => services.Insert(index, item);
    public bool Remove(ServiceDescriptor item) => services.Remove(item);
    public void RemoveAt(int index) => services.RemoveAt(index);
    IEnumerator IEnumerable.GetEnumerator() => services.GetEnumerator();
}

[SpeckyConfiguration]
interface ISpeckConfiguration
{
    [Singleton]
    A_Foo A_Foo { get; set; }

    [Scoped]
    IFooId A_FooId { get; set; }

    [Speck]
    A_FooTime A_FooTime { get; set;}


    B_Foo B_Foo { get; set; }


    B_FooId B_FooId { get; set; }


    B_FooTime B_FooTime { get; set; }
}