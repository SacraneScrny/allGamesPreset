using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Sackrany.Utils.Tracer
{
    public class TraceManager : AManager<TraceManager>
    {
        public bool SaveToFile = true;
        public bool IsTraceEnabled = true;
        readonly Dictionary<ITraceable, (refInt, StringBuilder)> traces = new ();

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Trace(ITraceable from, string message)
        {
            if (!Instance.IsTraceEnabled || !from.IsTracing()) return;
            if (!Instance.traces.TryGetValue(from, out var sb))
            {
                sb = (new refInt(0), new StringBuilder());
                sb.Item2.AppendLine($"[{from.name} TRACE] ({Time()})");
                Instance.traces.Add(from, sb);
            }
            sb.Item2.AppendLine($"{sb.Item1.value} [{Time()}] {message}");
            sb.Item1 += 1;
        }

        static string Time() => UnityEngine.Time.timeSinceLevelLoad.ToString("F3");//DateTime.Now.ToString("T", System.Globalization.CultureInfo.InvariantCulture);
        
        void OnApplicationQuit()
        {
            foreach (var trace in traces.Values)
            {
                Debug.Log(trace.Item2.ToString());
            }    
            if (SaveToFile && IsTraceEnabled)
                SaveTracesToFile();
        }
        
        void SaveTracesToFile()
        {
            try
            {
                string projectPath = Directory.GetParent(Application.dataPath).FullName;
                string folderPath = Path.Combine(projectPath, "Debug", "Traces");
                
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }

                Directory.CreateDirectory(folderPath);

                foreach (var kvp in traces)
                {
                    ITraceable traceable = kvp.Key;
                    StringBuilder content = kvp.Value.Item2;

                    if (traceable is UnityEngine.Object obj)
                    {
                        string safeName = string.Join("_", obj.name.Split(Path.GetInvalidFileNameChars()));
                
                        string fileName = $"{safeName}_{obj.GetInstanceID()}.txt";
                        string fullPath = Path.Combine(folderPath, fileName);

                        File.WriteAllText(fullPath, content.ToString());
                    }
                    else
                    {
                        string fileName = $"Trace_{traceable.GetHashCode()}.txt";
                        File.WriteAllText(Path.Combine(folderPath, fileName), content.ToString());
                    }
                }
        
                Debug.Log($"Traces saved to: {folderPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save traces: {e.Message}");
            }
        }

        class refInt
        {
            public uint value;

            public refInt(uint value)
            {
                this.value = value;
            }

            public static refInt operator +(refInt a, uint b)
            {
                a.value += b;
                return a;
            }
        }
    }
}