﻿namespace Specky7.Tests;
internal class A_Foo : IFooId, IFooTime
{
    public int Id { get; set; }
    public DateTime Time { get; set; }
}
internal class B_Foo : IFooId, IFooTime
{
    public int Id { get; set; }
    public DateTime Time { get; set; }
}
internal class A_FooId : IFooId
{
    public int Id { get; set; }
}
internal class B_FooId : IFooId
{
    public int Id { get; set; }
}

internal class A_FooTime : IFooId
{
    public int Id { get; set; }
}
internal class B_FooTime : IFooId
{
    public int Id { get; set; }
}