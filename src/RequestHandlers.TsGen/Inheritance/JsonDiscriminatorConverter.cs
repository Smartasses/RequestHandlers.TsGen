using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace RequestHandlers.TsGen.Inheritance
{
    public class JsonDiscriminatorConverter : JsonConverter // die moet ge ook nog ff toevoegen aan u converters van mvc
    {
        private readonly JsonDiscriminatorHelper _mapping;

        public JsonDiscriminatorConverter(params Assembly[] assemblies)
        {
            _mapping = new JsonDiscriminatorHelper(assemblies);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var camelCase = (serializer.ContractResolver.GetType() == typeof(CamelCasePropertyNamesContractResolver));
            var jo = new JObject();
            foreach (PropertyInfo prop in value.GetType().GetProperties())
            {
                if (prop.CanRead)
                {
                    var propValue = prop.GetValue(value, new object[0]);
                    jo.Add(camelCase ? ToCamelCase(prop.Name) : prop.Name, propValue != null ? JToken.FromObject(propValue, serializer) : JValue.CreateNull());
                }
            }
            jo.WriteTo(writer);
        }
        private static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
                return s;
            char[] chArray = s.ToCharArray();
            for (int index = 0; index < chArray.Length; ++index)
            {
                bool flag = index + 1 < chArray.Length;
                if (index <= 0 || !flag || char.IsUpper(chArray[index + 1]))
                    chArray[index] = char.ToLower(chArray[index]);
                else
                    break;
            }
            return new string(chArray);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var mapResult = _mapping.Mapping[objectType];
            var camelCase = (serializer.ContractResolver.GetType() == typeof(CamelCasePropertyNamesContractResolver));
            var result = existingValue;
            if (mapResult.Mapping.Any())
            {
                var token = JToken.ReadFrom(reader);
                var obj = token as JObject;
                if (!token.HasValues || obj == null)
                {
                    return null;
                }
                var discriminatorValue = obj.GetValue(camelCase ? ToCamelCase(mapResult.DiscriminatorPropertyName) : mapResult.DiscriminatorPropertyName).ToObject(mapResult.DiscriminatorType);
                if (mapResult.DiscriminatorType.GetTypeInfo().IsEnum)
                {
                    discriminatorValue = System.Enum.ToObject(mapResult.DiscriminatorType, discriminatorValue);
                }

                var targetType = mapResult.Mapping[discriminatorValue];
                result = Activator.CreateInstance(targetType);

                foreach (PropertyInfo prop in targetType.GetProperties())
                {
                    if (prop.CanWrite)
                    {
                        var value = obj.GetValue(prop.Name, StringComparison.CurrentCultureIgnoreCase);
                        if (value != null)
                        {
                            prop.SetValue(result, value.ToObject(prop.PropertyType, serializer), null);
                        }
                    }
                }
                return result;
            }
            return result;
        }

        public override bool CanConvert(Type objectType)
        {
            return _mapping.Mapping.ContainsKey(objectType);
        }
    }
}