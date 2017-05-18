using System;

namespace RequestHandlers.TsGen
{
    [AttributeUsage(AttributeTargets.Property)]
    public class JsonDiscriminatorAttribute : Attribute
    {
    }
}