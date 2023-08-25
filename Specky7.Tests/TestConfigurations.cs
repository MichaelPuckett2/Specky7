namespace Specky7.Tests;

[SpeckyConfiguration(Option = "Invalid")]
interface IInvalidConfiguration
{
    [Singleton]
    A_Foo A_Foo { get; set; }

    [Scoped]
    IFooId A_FooId { get; set; }
}

[SpeckyConfiguration(Option = "Ok")]
interface IOkConfiguration
{
    [Singleton]
    A_Foo A_Foo { get; set; }

    [Scoped]
    A_FooId A_FooId { get; set; }
}

[SpeckyConfiguration(Option = "Ok2")]
interface IOk2Configuration
{
    [Transient]
    A_FooTime A_FooTime { get; set; }

    [Scoped]
    B_Foo B_Foo { get; set; }
}
