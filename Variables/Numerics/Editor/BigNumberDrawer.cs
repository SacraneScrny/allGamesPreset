using System;

using UnityEditor;

using UnityEngine;
// Для Math.Clamp

namespace Sackrany.Variables.Numerics.Editor
{
    [CustomPropertyDrawer(typeof(BigNumber))]
    public class BigNumberDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Отрисовка лейбла (имя переменной)
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Убираем отступ для внутренних полей
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var mantissaProp = property.FindPropertyRelative("mantissa");
            var exponentProp = property.FindPropertyRelative("exponent");

            // Защита от ошибок (если забыли [SerializeField])
            if (mantissaProp == null || exponentProp == null)
            {
                EditorGUI.HelpBox(position, "Add [SerializeField] fields", MessageType.Error);
                return;
            }

            // --- НАСТРОЙКА РАЗМЕРОВ ---
            
            // 1. Ограничиваем общую ширину полей ввода. 
            // Берем либо текущую ширину, либо 220 пикселей (что меньше), чтобы не было "колбасы"
            position.width = Mathf.Min(position.width, 220f);

            // 2. Задаем ширины компонентов
            float mantissaWidth = 50f; // Фиксированная короткая ширина для числа 1-10
            float labelWidth = 15f;    // Место под букву "e"
            // Экспонента занимает всё остальное место от наших 220px (или меньше)
            float exponentWidth = position.width - mantissaWidth - labelWidth; 

            Rect mantissaRect = new Rect(position.x, position.y, mantissaWidth, position.height);
            Rect eLabelRect = new Rect(mantissaRect.xMax, position.y, labelWidth, position.height);
            Rect exponentRect = new Rect(eLabelRect.xMax, position.y, exponentWidth, position.height);

            // --- ОТРИСОВКА ---

            // 1. Поле Мантиссы с ограничением 1-10
            EditorGUI.BeginChangeCheck();
            double newMantissa = EditorGUI.DoubleField(mantissaRect, mantissaProp.doubleValue);
            if (EditorGUI.EndChangeCheck())
            {
                // Ограничиваем ввод диапазоном [1, 10]
                // Примечание: В строгой математике это [1, 10), но для UI удобнее 10 включительно.
                mantissaProp.doubleValue = Math.Clamp(newMantissa, 1.0, 10.0);
            }

            // 2. Декоративная буква "e"
            var centeredStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            EditorGUI.LabelField(eLabelRect, "e", centeredStyle);

            // 3. Поле Экспоненты
            exponentProp.intValue = EditorGUI.IntField(exponentRect, exponentProp.intValue);

            // Восстанавливаем отступ
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}