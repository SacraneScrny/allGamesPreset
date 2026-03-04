using System.Collections.Generic;

using Sackrany.Actor.Modules.ModuleComposition;
using Sackrany.Actor.Static;
using Sackrany.Actor.Unit;
using Sackrany.SerializableData.Components;
using Sackrany.UnitSystem;
using Sackrany.UnitSystem.Static;

using UnityEngine;

namespace Sackrany.SerializableData.WorldSerializables
{
    [RequireComponent(typeof(Unit))]
    public class UnitSerializer : SerializableBehaviour
    {
        public bool SerializePosition = true;
        public bool SerializeRotation = true;
        public bool SerializeScale;
        Unit unit;
        private protected override void OnRegister()
        {
            unit = GetComponent<Unit>();
            
            unit.Command(SerializeUnit);
        }
        void SerializeUnit(Unit u)
        {      
            RegisterSerializable(
                "unit::modules",
                () =>
                {
                    Dictionary<string, object[]> data = new ();
                        foreach (var module in u.GetModules())
                            if (module is ISerializableModule serializableModule)
                                data.Add(module.GetType().Name, serializableModule.Serialize());
                    return data;
                },
                (data) =>
                {
                    Dictionary<string, ISerializableModule> ser = new ();
                        foreach (var module in u.GetModules())
                            if (module is ISerializableModule serializableModule)
                                ser.Add(module.GetType().Name, serializableModule);

                    foreach (var d in data)
                        if (ser.TryGetValue(d.Key, out ISerializableModule serializableModule))
                            serializableModule.Deserialize(d.Value);
                });
            
            if (SerializePosition)
                RegisterSerializable("unit::position", () => transform.position, (p) => transform.position = p + Vector3.up * 0.01f);
            if (SerializeRotation)
                RegisterSerializable("unit::rotation", () => transform.rotation, (p) => transform.rotation = p);
            if (SerializeScale)
                RegisterSerializable("unit::scale", () => transform.localScale, (p) => transform.localScale = p);
        }
        protected override void onDeserialized()
        {
            unit.MarkAsDeserialized();
        }
    }
}