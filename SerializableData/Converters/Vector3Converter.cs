using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sackrany.SerializableData.Converters
{
    public class Vector3Converter : JsonConverter<UnityEngine.Vector3>
    {
        private const string TypeMetaProperty = "$type";
        
        public override void WriteJson(JsonWriter writer, UnityEngine.Vector3 v, JsonSerializer serializer)
        {
            string asmName = typeof(UnityEngine.Vector3).Assembly.GetName().Name;        // => "UnityEngine.CoreModule"
            string typeName = typeof(UnityEngine.Vector3).FullName;    
            
            writer.WriteStartObject();
            writer.WritePropertyName(TypeMetaProperty); writer.WriteValue($"{typeName}, {asmName}");
            writer.WritePropertyName("x"); writer.WriteValue(v.x);
            writer.WritePropertyName("y"); writer.WriteValue(v.y);
            writer.WritePropertyName("z"); writer.WriteValue(v.z);
            writer.WriteEndObject();
        }

        public override UnityEngine.Vector3 ReadJson(JsonReader reader, Type objectType, UnityEngine.Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            obj.Remove(TypeMetaProperty);
            float x = obj["x"]?.Value<float>() ?? 0;
            float y = obj["y"]?.Value<float>() ?? 0;
            float z = obj["z"]?.Value<float>() ?? 0;
            return new UnityEngine.Vector3(x, y, z);
        }
    }
}