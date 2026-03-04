using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sackrany.SerializableData.Converters
{
    public class Vector2Converter : JsonConverter<UnityEngine.Vector2>
    {
        private const string TypeMetaProperty = "$type";
        
        public override void WriteJson(JsonWriter writer, UnityEngine.Vector2 v, JsonSerializer serializer)
        {
            string asmName = typeof(UnityEngine.Vector2).Assembly.GetName().Name;        // => "UnityEngine.CoreModule"
            string typeName = typeof(UnityEngine.Vector2).FullName;    
            
            writer.WriteStartObject();
            writer.WritePropertyName(TypeMetaProperty); writer.WriteValue($"{typeName}, {asmName}");
            writer.WritePropertyName("x"); writer.WriteValue(v.x);
            writer.WritePropertyName("y"); writer.WriteValue(v.y);
            writer.WriteEndObject();
        }

        public override UnityEngine.Vector2 ReadJson(JsonReader reader, Type objectType, UnityEngine.Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            obj.Remove(TypeMetaProperty);
            float x = obj["x"]?.Value<float>() ?? 0;
            float y = obj["y"]?.Value<float>() ?? 0;
            return new UnityEngine.Vector2(x, y);
        }
    }
}