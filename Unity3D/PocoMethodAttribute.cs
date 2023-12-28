using System;

public sealed class PocoMethodAttribute : Attribute
{
    public string Name { get; private set; }

    public PocoMethodAttribute(string name)
    {
        Name = name;
    }
}