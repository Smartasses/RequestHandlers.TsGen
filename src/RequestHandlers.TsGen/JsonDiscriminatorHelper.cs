using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RequestHandlers.TsGen
{
    public class JsonDiscriminatorHelper
    {
        public class Result
        {
            public Type DiscriminatorType { get; set; }
            public string DiscriminatorPropertyName { get; set; }
            public Dictionary<object, Type> Mapping { get; set; }
            public List<Type> BaseTypes { get; set; }
        }
        public JsonDiscriminatorHelper(params Assembly[] assemblies)
        {
            var types = assemblies
                .SelectMany(x => x.GetTypes()).ToArray()
                .SelectMany(x => x
                    .GetProperties(BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public)
                    .Select(p => new { Property = p, Descriminator = p.GetCustomAttribute<JsonDiscriminatorAttribute>() })
                    .Where(p => p.Descriminator != null), (type, property) => new { Type = type, property.Property })
                .Select(x =>
                {
                
                    bool canBeConstructed = false;
                    object discriminatorValue = null;
                    if (!x.Type.GetTypeInfo().IsAbstract && !x.Type.GetTypeInfo().IsInterface)
                    {
                        try
                        {
                            var instance = Activator.CreateInstance(x.Type);
                            canBeConstructed = true;
                            discriminatorValue = x.Property.GetMethod.Invoke(instance, new object[0]);
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Make sure you have a public parameterless constructor. [Type: {x.Type}]", e);
                        }
                    }
                    return new
                    {
                        x.Type,
                        x.Property.Name,
                        x.Property,
                        canBeConstructed,
                        discriminatorValue
                    };
                })
                .ToArray();

            var constructables = types.Select(x => new
            {
                Type = x,
                CanBeConstructedBy = types.Where(t => t.canBeConstructed && x.Type.IsAssignableFrom(t.Type)).ToArray()
            }).ToArray();

            Mapping = constructables.ToDictionary(x => x.Type.Type,
                x =>
                    new Result
                    {
                        DiscriminatorType = x.Type.Property.PropertyType,
                        DiscriminatorPropertyName = x.Type.Property.Name,
                        Mapping = x.CanBeConstructedBy.ToDictionary(m => m.discriminatorValue, m => m.Type),
                        BaseTypes = types.Select(t => t.Type).Where(t => t != x.Type.Type && t.IsAssignableFrom(x.Type.Type)).ToList()
                    });
        }

        public Dictionary<Type, Result> Mapping { get; set; }
    }
}