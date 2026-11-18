#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    [CustomEditor(typeof(SelectiveLogsScriptableObject))]
    public class SelectiveLogsScriptableObjectEditor : UnityEditor.Editor
    {

        private string newSectionName = "";
        private Color newSectionColor = Color.white;
        private bool showNewSectionInput;

        private ReorderableList systemDefinedList;
        private ReorderableList userDefinedList;

        public override void OnInspectorGUI()
        {
            SelectiveLogsScriptableObject config = target as SelectiveLogsScriptableObject;

            if (config == null)
                return;

            serializedObject.Update();
            config.InitializeDefaultSections();

            SerializedProperty systemDefinedProp = serializedObject.FindProperty("systemDefinedSections");
            SerializedProperty userDefinedProp = serializedObject.FindProperty("userDefinedSections");

            systemDefinedList ??= new ReorderableList(serializedObject, systemDefinedProp, true, true, false, false)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "System Defined Sections", EditorStyles.boldLabel),
                drawElementCallback = (rect, index, _, __) =>
                    DrawSectionElement(rect, systemDefinedProp.GetArrayElementAtIndex(index)),
                elementHeightCallback = index => GetElementHeight(systemDefinedProp.GetArrayElementAtIndex(index))
            };

            userDefinedList ??= new ReorderableList(serializedObject, userDefinedProp, true, true, false, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "User Defined Sections", EditorStyles.boldLabel),
                drawElementCallback = (rect, index, _, __) =>
                    DrawSectionElement(rect, userDefinedProp.GetArrayElementAtIndex(index)),
                onRemoveCallback = list =>
                {
                    if (list.index >= 0 && list.index < userDefinedProp.arraySize)
                        userDefinedProp.DeleteArrayElementAtIndex(list.index);
                },
                elementHeightCallback = index => GetElementHeight(userDefinedProp.GetArrayElementAtIndex(index))
            };

            systemDefinedList.DoLayoutList();
            GUILayout.Space(5);
            userDefinedList.DoLayoutList();

            GUILayout.Space(10);
            DrawAddNewSectionButton(config);

            serializedObject.ApplyModifiedProperties();

            if (!GUI.changed)
                return;

            EditorUtility.SetDirty(config);
            EnhancedConsoleWindow.RepaintIfOpen();
        }

        private void DrawSectionElement(Rect rect, SerializedProperty property)
        {
            SerializedProperty nameProperty = property.FindPropertyRelative("name");
            SerializedProperty colorProperty = property.FindPropertyRelative("color");
            SerializedProperty enabledProperty = property.FindPropertyRelative("enabled");
            SerializedProperty useRegexProperty = property.FindPropertyRelative("useRegex");
            SerializedProperty patternProperty = property.FindPropertyRelative("pattern");

            float checkboxWidth = 20;
            float colorWidth = 50;
            float padding = 5;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineSpacing = EditorGUIUtility.standardVerticalSpacing;

            Rect enabledRect = new(rect.x, rect.y, checkboxWidth, lineHeight);
            Rect colorRect = new(rect.x + checkboxWidth + padding, rect.y, colorWidth, lineHeight);
            Rect nameRect = new(rect.x + checkboxWidth + colorWidth + padding * 2, rect.y, 150, lineHeight);
            Rect regexRect = new(rect.x + checkboxWidth + colorWidth + 150 + padding * 3, rect.y, 60, lineHeight);

            enabledProperty.boolValue = EditorGUI.Toggle(enabledRect, enabledProperty.boolValue);
            colorProperty.colorValue = EditorGUI.ColorField(colorRect, colorProperty.colorValue);
            EditorGUI.PropertyField(nameRect, nameProperty, GUIContent.none);
            useRegexProperty.boolValue = EditorGUI.Toggle(regexRect, new GUIContent("Regex"), useRegexProperty.boolValue);

            if (!useRegexProperty.boolValue)
                return;

            float patternY = rect.y + lineHeight + lineSpacing;
            Rect patternLabelRect = new(rect.x, patternY, 60, lineHeight);
            Rect patternFieldRect = new(rect.x + 65, patternY, rect.width - 65, lineHeight);

            EditorGUI.LabelField(patternLabelRect, "Pattern", EditorStyles.label);
            EditorGUI.PropertyField(patternFieldRect, patternProperty, GUIContent.none);
        }

        private float GetElementHeight(SerializedProperty property)
        {
            SerializedProperty useRegexProperty = property.FindPropertyRelative("useRegex");
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineSpacing = EditorGUIUtility.standardVerticalSpacing;

            if (useRegexProperty.boolValue)
                return (lineHeight + lineSpacing) * 2 + 4;

            return lineHeight + lineSpacing;
        }


        private void DrawAddNewSectionButton(SelectiveLogsScriptableObject config)
        {
            GUILayout.Label("Add New Section", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;

            showNewSectionInput = EditorGUILayout.Foldout(showNewSectionInput, "New Section Input");

            if (showNewSectionInput)
            {
                newSectionName = EditorGUILayout.TextField("Section Name", newSectionName);
                newSectionColor = EditorGUILayout.ColorField("Section Color", newSectionColor);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Add Section"))
                {
                    if (!string.IsNullOrEmpty(newSectionName))
                    {
                        config.AddUserDefinedSection(newSectionName, newSectionColor);
                        newSectionName = "";
                        newSectionColor = Color.white;
                        showNewSectionInput = false;
                    }
                }

                if (GUILayout.Button("Cancel"))
                {
                    newSectionName = "";
                    newSectionColor = Color.white;
                    showNewSectionInput = false;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif

