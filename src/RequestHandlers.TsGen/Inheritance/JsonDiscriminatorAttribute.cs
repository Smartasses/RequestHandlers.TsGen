using System;

namespace RequestHandlers.TsGen.Inheritance
{
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonDiscriminatorAttribute : Attribute
    {
    }
}