using System;
using System.Reflection;

using Sackrany.Variables.ExpandedVariable.Abstracts;
using Sackrany.Variables.ExpandedVariable.Entities;

using UnityEditor;

using UnityEngine;

namespace Sackrany.Variables.ExpandedVariable.Editor
{
    [CustomPropertyDrawer(typeof(BaseExpandedVariable<>), true)]
    public class BaseExpandedVariableDrawer : PropertyDrawer
    {
        private const float Padding = 6f;
        private const float Spacing = 4f;

        private static readonly Color BgColor = new Color(0.2f, 0.35f, 0.6f, 0.15f);
        private static readonly Color BorderColor = new Color(0.2f, 0.45f, 0.85f, 1f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var variableProp = property.FindPropertyRelative("Variable");
            float h = variableProp != null
                ? EditorGUI.GetPropertyHeight(variableProp, true)
                : EditorGUIUtility.singleLineHeight;

            return EditorGUIUtility.singleLineHeight 
                   + h * 2
                   + Padding * 2
                   + Spacing * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Фон
            DrawBox(position);

            // Внутренний rect
            Rect content = new Rect(
                position.x + Padding,
                position.y + Padding,
                position.width - Padding * 2,
                position.height - Padding * 2
            );        
        
            Rect headerRect = new(
                content.x,
                content.y,
                content.width,
                EditorGUIUtility.singleLineHeight
            );
        
            Rect resetRect = new Rect(
                content.xMax - 50f,
                content.y,
                50f,
                EditorGUIUtility.singleLineHeight
            );

            if (GUI.Button(resetRect, "Reset"))
            {
                ResetValue(property);
            }
        
            EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);

            var variableProp = property.FindPropertyRelative("Variable");
            if (variableProp == null)
            {
                EditorGUI.LabelField(content, "Variable not found");
                EditorGUI.EndProperty();
                return;
            }
        
            float valueHeight = EditorGUI.GetPropertyHeight(variableProp, true);
            Rect valueRect = new(
                content.x,
                headerRect.yMax + Spacing,
                content.width,
                valueHeight
            );        
        
            Rect previewRect = new(
                content.x,
                valueRect.yMax + Spacing,
                content.width,
                EditorGUIUtility.singleLineHeight
            );

            DrawValueField(valueRect, variableProp);

            using (new EditorGUI.DisabledScope(true))
            {
                object preview = GetPreviewValue(property);
                EditorGUI.TextField(
                    previewRect,
                    preview != null ? preview.ToString() : "null"
                );
            }

            EditorGUI.EndProperty();
        }
    
        private void ResetValue(SerializedProperty property)
        {
            object target = property.serializedObject.targetObject;
            object container = GetFieldValue(target, property.propertyPath);

            if (container == null)
                return;

            Type type = container.GetType();

            SetField(type, container, "Variable");
            SetField(type, container, "_defaultValue");
            SetBoolField(type, container, "_hasInited", false);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
        private void SetField(Type type, object instance, string fieldName)
        {
            FieldInfo field = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (field == null)
                return;

            object defaultValue = field.FieldType.IsValueType
                ? Activator.CreateInstance(field.FieldType)
                : null;

            field.SetValue(instance, defaultValue);
        }
        private void SetBoolField(Type type, object instance, string fieldName, bool value)
        {
            FieldInfo field = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (field == null || field.FieldType != typeof(bool))
                return;

            field.SetValue(instance, value);
        }

        private void DrawValueField(Rect rect, SerializedProperty prop)
        {
            Type realType = fieldInfo.FieldType;
        
            if (realType == typeof(ExpandedQuaternion))
            {
                var raw = prop.quaternionValue;

                Vector3 euler = raw.eulerAngles;

                EditorGUI.BeginChangeCheck();
                euler = EditorGUI.Vector3Field(rect, "Value", euler);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.quaternionValue = Quaternion.Euler(euler);
                }
            }
            else
            {
                EditorGUI.PropertyField(rect, prop, new GUIContent("Value"), true);
            }
        }
        private void DrawBox(Rect rect)
        {
            EditorGUI.DrawRect(rect, BgColor);

            // рамка
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), BorderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), BorderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), BorderColor);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), BorderColor);
        }

        private object GetPreviewValue(SerializedProperty property)
        {
            try
            {
                object target = property.serializedObject.targetObject;
                object container = GetFieldValue(target, property.propertyPath);

                if (container == null)
                    return null;

                MethodInfo getValueMethod = container.GetType().GetMethod(
                    "GetValueEditor",
                    BindingFlags.Instance | BindingFlags.Public
                );

                return getValueMethod?.Invoke(container, null);
            }
            catch
            {
                return "err";
            }
        }
        private object GetFieldValue(object source, string path)
        {
            string[] parts = path.Replace(".Array.data[", "[").Split('.');
            object current = source;

            foreach (string part in parts)
            {
                if (current == null) return null;

                if (part.Contains("["))
                {
                    string fieldName = part.Substring(0, part.IndexOf("["));
                    int index = int.Parse(part.Substring(part.IndexOf("[") + 1).TrimEnd(']'));

                    FieldInfo field = current.GetType().GetField(
                        fieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );

                    var list = field?.GetValue(current) as System.Collections.IList;
                    current = list?[index];
                }
                else
                {
                    FieldInfo field = current.GetType().GetField(
                        part,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );

                    current = field?.GetValue(current);
                }
            }

            return current;
        }
    }
}
