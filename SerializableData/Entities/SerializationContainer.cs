using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Sackrany.SerializableData.Components;

namespace Sackrany.SerializableData.Entities
{
    [Serializable]
    public class SerializationContainer
    {
        [JsonProperty]
        private Dictionary<string, SerializationData> SerializedData = new ();

        [JsonIgnore]
        public Dictionary<string, SerializableBehaviour> TemporaryContainer = new();

        public void SerializeAll()
        {
            foreach (var kvp in TemporaryContainer)
                kvp.Value.Serialize();
            SerializedData = TemporaryContainer
                .ToDictionary(
                    x => x.Key,
                    x => new SerializationData(x.Value._serializedFields)
                );
        }
        public void DeserializeAll()
        {
            Dictionary<string, SerializableBehaviour> found = new (TemporaryContainer);
            if (SerializedData.Count > 0)
                foreach (var kvp in SerializedData)
                {
                    if (found.ContainsKey(kvp.Key))
                        found[kvp.Key].Deserialize(kvp.Value._serializedFields);
                    else
                        found[kvp.Key].Deserialize(null);
                }
            else
                foreach (var kvp in TemporaryContainer)
                    kvp.Value.Deserialize(null);
        }
    }

    [Serializable]
    public struct SerializationData
    {
        public Dictionary<string, object> _serializedFields;

        public SerializationData(Dictionary<string, object> serializedFields)
        {
            _serializedFields = serializedFields;
        }
    }
}