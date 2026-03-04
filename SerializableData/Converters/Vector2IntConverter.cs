using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sackrany.SerializableData.Converters
{
    public class Vector2IntConverter : JsonConverter<UnityEngine.Vector2Int>
    {
        private const string TypeMetaProperty = "$type";
        
        public override void WriteJson(JsonWriter writer, UnityEngine.Vector2Int v, JsonSerializer serializer)
        {
            string asmName = typeof(UnityEngine.Vector2Int).Assembly.GetName().Name;        // => "UnityEngine.CoreModule"
            string typeName = typeof(UnityEngine.Vector2Int).FullName;    
            
            writer.WriteStartObject();
            writer.WritePropertyName(TypeMetaProperty); writer.WriteValue($"{typeName}, {asmName}");
            writer.WritePropertyName("x"); writer.WriteValue(v.x);
            writer.WritePropertyName("y"); writer.WriteValue(v.y);
            writer.WriteEndObject();
        }

        public override UnityEngine.Vector2Int ReadJson(JsonReader reader, Type objectType, UnityEngine.Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            obj.Remove(TypeMetaProperty);
            int x = obj["x"]?.Value<int>() ?? 0;
            int y = obj["y"]?.Value<int>() ?? 0;
            return new UnityEngine.Vector2Int(x, y);
        }
    }
}