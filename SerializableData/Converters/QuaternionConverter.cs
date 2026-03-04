using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

namespace Sackrany.SerializableData.Converters
{
    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        private const string TypeMetaProperty = "$type";
        
        public override void WriteJson(JsonWriter writer, Quaternion v, JsonSerializer serializer)
        {
            string asmName = typeof(Quaternion).Assembly.GetName().Name;        // => "UnityEngine.CoreModule"
            string typeName = typeof(Quaternion).FullName;    
            
            writer.WriteStartObject();
            writer.WritePropertyName(TypeMetaProperty); writer.WriteValue($"{typeName}, {asmName}");
            writer.WritePropertyName("x"); writer.WriteValue(v.x);
            writer.WritePropertyName("y"); writer.WriteValue(v.y);
            writer.WritePropertyName("z"); writer.WriteValue(v.z);
            writer.WritePropertyName("w"); writer.WriteValue(v.w);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            obj.Remove(TypeMetaProperty);
            float x = obj["x"]?.Value<float>() ?? 0;
            float y = obj["y"]?.Value<float>() ?? 0;
            float z = obj["z"]?.Value<float>() ?? 0;
            float w = obj["w"]?.Value<float>() ?? 0;
            return new Quaternion(x, y, z, w);
        }
    }
}