using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Sackrany.SerializableData.Components;
using Sackrany.SerializableData.Converters;
using Sackrany.SerializableData.Entities;
using Sackrany.Utils;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Sackrany.SerializableData
{
    public class DataManager : AManager<DataManager>
    {
        static readonly bool UsePlayerPrefs = false;
        static readonly string Datapath = Application.persistentDataPath + "/Saves/";
        static readonly string DatafileName = "saveData";
        static readonly string FileExtension = ".json";

        public event System.Action<List<object>> OnSaveDataCall;
        SaveDataStructure saveData;
        SerializationContainer serializationContainer = null;
        bool _isInitialized = false;
        IEnumerator Start()
        {
            int serializables = FindObjectsByType<SerializableBehaviour>(FindObjectsSortMode.None).Length;
            Instance.serializationContainer ??= LoadData<SerializationContainer>();
            yield return new WaitWhile(() => serializationContainer.TemporaryContainer.Count < serializables);
            
            Instance.Initialize();
        }
        
        private protected override void OnManagerAwake()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = { new Vector3Converter(), new Vector2Converter(), new QuaternionConverter(), new Vector2IntConverter() },
            };
        }
        void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            serializationContainer.DeserializeAll();
            OnSaveDataCall += (l) =>
            {
                serializationContainer.SerializeAll();
                l.Add(serializationContainer);
            };
        }
        public static void RegisterSerializable(SerializableBehaviour serializable)
        {
            Instance.serializationContainer ??= LoadData<SerializationContainer>();
            Instance.serializationContainer.TemporaryContainer.TryAdd(serializable.Guid, serializable);
        }
        
        public static T LoadData<T>(string customFolder = "") where T : new()
        {
            if (Instance.saveData != null)
            {
                if (Instance.saveData[typeof(T).Name] is not T)
                    Instance.saveData[typeof(T).Name] = new T();
                Instance.saveData[typeof(T).Name] ??= new T();
                return (T)Instance.saveData[typeof(T).Name];
            }

            if (UsePlayerPrefs)
            {
                var saveData = PlayerPrefs.GetString("save");
                Instance.saveData = PlayerPrefs.HasKey("save")
                    ? JsonConvert.DeserializeObject<SaveDataStructure>(saveData)
                    : null;
                
                Instance.saveData ??= new SaveDataStructure();
                if (Instance.saveData.TryGetValue("AppVersion", out var ver) && GetMajorVersion((string)ver) != GetMajorVersion(Application.version)) 
                    Instance.saveData = new SaveDataStructure();
                Instance.saveData[typeof(T).Name] ??= new T();
                
                return (T)Instance.saveData[typeof(T).Name];
            }
            
            if (customFolder != "" && !Directory.Exists(Datapath + customFolder + "/")) 
                Directory.CreateDirectory(Datapath + customFolder + "/");
            
            string dataStream;
            var savefile = Datapath + customFolder + "/" + DatafileName + FileExtension;

            if (!Directory.Exists(Datapath)) Directory.CreateDirectory(Datapath);

            if (!File.Exists(savefile))
            {
                SaveData(new T(), customFolder);

                return (T)Instance.saveData[typeof(T).Name];
            }

            dataStream = File.ReadAllText(savefile);
            Instance.saveData = JsonConvert.DeserializeObject<SaveDataStructure>(dataStream);
            Instance.saveData ??= new SaveDataStructure();
            if (Instance.saveData.TryGetValue("AppVersion", out var v) && GetMajorVersion((string)v) != GetMajorVersion(Application.version)) 
                Instance.saveData = new SaveDataStructure();
            
            if (Instance.saveData[typeof(T).Name] is not T)
                Instance.saveData[typeof(T).Name] = new T();
            Instance.saveData[typeof(T).Name] ??= new T();
            
            return (T)Instance.saveData[typeof(T).Name];
        }
        static void SaveData<T>(T data, string customFolder = "") where T : new()
        {
            if (customFolder != "" && !Directory.Exists(Datapath + customFolder + "/")) 
                Directory.CreateDirectory(Datapath + customFolder + "/");
         
            if (Instance.saveData == null)
                Instance.saveData = new SaveDataStructure();
            
            var savefile = Datapath + customFolder + "/" + DatafileName + FileExtension;
            using (var dataStream = File.CreateText(savefile))
            {
                Instance.saveData[typeof(T).Name] = data ?? new T();
                dataStream.Write(JsonConvert.SerializeObject(Instance.saveData, Formatting.Indented));
            }
        }
        public static void SaveAllData(string customFolder = "")
        {
            if (!Instance._isInitialized) return;
            
            if (!UsePlayerPrefs)
                if (customFolder != "" && !Directory.Exists(Datapath + customFolder + "/")) 
                    Directory.CreateDirectory(Datapath + customFolder + "/");
         
            if (Instance.saveData == null)
                Instance.saveData = new SaveDataStructure();
            
            List<object> data = new List<object>();
            Instance.saveData["AppVersion"] = Application.version;
            Instance.OnSaveDataCall?.Invoke(data);
            foreach (var d in data)
                Instance.saveData[d.GetType().Name] = d;
            
            if (UsePlayerPrefs)
            {
                var save = JsonConvert.SerializeObject(Instance.saveData, Formatting.Indented);
                PlayerPrefs.SetString("save", save);
                PlayerPrefs.Save(); 
                return;
            }
            
            var savefile = Datapath + customFolder + "/" + DatafileName + FileExtension;
            using (var dataStream = File.CreateText(savefile))
            {
                dataStream.Write(JsonConvert.SerializeObject(Instance.saveData, Formatting.Indented));
            }
        }

        #if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                SaveAllData();
            }
        }
        #endif
        
        static string GetMajorVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return "0";
            return version.Split('.')[0];
        }
    }
    
    public class SaveDataStructure
    {
        public Dictionary<string, object> _saveData = new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                _saveData ??= new Dictionary<string, object>();
                if (_saveData.TryGetValue(key, out var value))
                    return value;
                _saveData.Add(key, null);
                return null;
            }
            set
            {
                _saveData ??= new Dictionary<string, object>();
                if (!_saveData.TryGetValue(key, out var val))
                    _saveData.Add(key, value);
                else
                 _saveData[key] = value;
            }
        }
        public bool TryGetValue(string key, out object value) => _saveData.TryGetValue(key, out value);
    }
}