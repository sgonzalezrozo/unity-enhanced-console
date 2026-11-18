#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Inspector for the section config: reorderable lists of system and user sections, and a form to add one.
    /// </summary>
    [CustomEditor(typeof(SelectiveLogsScriptableObject))]
    public class SelectiveLogsScriptableObjectEditor : UnityEditor.Editor
    {
        private const float CHECKBOX_WIDTH = 20f;
        private const float COLOR_WIDTH = 50f;
        private const float NAME_WIDTH = 150f;
        private const float REGEX_WIDTH = 60f;
        private const float FIELD_PADDING = 5f;
        private const float PATTERN_LABEL_WIDTH = 60f;
        private const float PATTERN_FIELD_LEFT = 65f;
        private const float REGEX_ROW_EXTRA_HEIGHT = 4f;
        private const float LIST_SPACING = 5f;
        private const float FORM_SPACING = 10f;
        private static readonly GUIContent REGEX_LABEL_CONTENT = new("Regex");

        private string newSectionName = "";
        private Color newSectionColor = Color.white;
        private bool showNewSectionInput;

        private ReorderableList systemDefinedList;
        private ReorderableList userDefinedList;
        private SerializedProperty systemDefinedProperty;
        private SerializedProperty userDefinedProperty;

        public override void OnInspectorGUI()
        {
            SelectiveLogsScriptableObject config = target as SelectiveLogsScriptableObject;

            if (config == null)
                return;

            serializedObject.Update();
            config.InitializeDefaultSections();
            BindLists();
            systemDefinedList.DoLayoutList();
            GUILayout.Space(LIST_SPACING);
            userDefinedList.DoLayoutList();
            GUILayout.Space(FORM_SPACING);
            DrawNewSectionForm(config);
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
                OnConfigChanged(config);
        }

        private static void OnConfigChanged(SelectiveLogsScriptableObject config)
        {
            EditorUtility.SetDirty(config);
            EnhancedConsoleWindow.RepaintIfOpen();
        }

        private void BindLists()
        {
            systemDefinedProperty = serializedObject.FindProperty("systemDefinedSections");
            userDefinedProperty = serializedObject.FindProperty("userDefinedSections");
            systemDefinedList ??= CreateSystemDefinedList();
            userDefinedList ??= CreateUserDefinedList();
        }

        private ReorderableList CreateSystemDefinedList()
        {
            return new ReorderableList(serializedObject, systemDefinedProperty, true, true, false, false)
            {
                drawHeaderCallback = DrawSystemDefinedHeader,
                drawElementCallback = DrawSystemDefinedElement,
                elementHeightCallback = GetSystemDefinedElementHeight
            };
        }

        private ReorderableList CreateUserDefinedList()
        {
            return new ReorderableList(serializedObject, userDefinedProperty, true, true, false, true)
            {
                drawHeaderCallback = DrawUserDefinedHeader,
                drawElementCallback = DrawUserDefinedElement,
                onRemoveCallback = RemoveUserDefinedElement,
                elementHeightCallback = GetUserDefinedElementHeight
            };
        }

        private static void DrawSystemDefinedHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "System Defined Sections", EditorStyles.boldLabel);
        }

        private static void DrawUserDefinedHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "User Defined Sections", EditorStyles.boldLabel);
        }

        private void DrawSystemDefinedElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawSectionElement(rect, systemDefinedProperty.GetArrayElementAtIndex(index));
        }

        private void DrawUserDefinedElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DrawSectionElement(rect, userDefinedProperty.GetArrayElementAtIndex(index));
        }

        private float GetSystemDefinedElementHeight(int index)
        {
            return GetElementHeight(systemDefinedProperty.GetArrayElementAtIndex(index));
        }

        private float GetUserDefinedElementHeight(int index)
        {
            return GetElementHeight(userDefinedProperty.GetArrayElementAtIndex(index));
        }

        private void RemoveUserDefinedElement(ReorderableList list)
        {
            if (list.index >= 0 && list.index < userDefinedProperty.arraySize)
                userDefinedProperty.DeleteArrayElementAtIndex(list.index);
        }

        private void DrawSectionElement(Rect rect, SerializedProperty property)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            DrawMainRow(new Rect(rect.x, rect.y, rect.width, lineHeight), property);

            SerializedProperty useRegexProperty = property.FindPropertyRelative("useRegex");
            if (!useRegexProperty.boolValue)
                return;

            float patternTop = rect.y + lineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect patternRow = new(rect.x, patternTop, rect.width, lineHeight);
            DrawPatternRow(patternRow, property.FindPropertyRelative("pattern"));
        }

        private static void DrawMainRow(Rect row, SerializedProperty property)
        {
            SerializedProperty enabledProperty = property.FindPropertyRelative("enabled");
            SerializedProperty colorProperty = property.FindPropertyRelative("color");
            SerializedProperty useRegexProperty = property.FindPropertyRelative("useRegex");

            float colorLeft = row.x + CHECKBOX_WIDTH + FIELD_PADDING;
            float nameLeft = colorLeft + COLOR_WIDTH + FIELD_PADDING;
            float regexLeft = nameLeft + NAME_WIDTH + FIELD_PADDING;
            enabledProperty.boolValue = EditorGUI.Toggle(new Rect(row.x, row.y, CHECKBOX_WIDTH, row.height),
                enabledProperty.boolValue);
            colorProperty.colorValue = EditorGUI.ColorField(new Rect(colorLeft, row.y, COLOR_WIDTH, row.height),
                colorProperty.colorValue);
            EditorGUI.PropertyField(new Rect(nameLeft, row.y, NAME_WIDTH, row.height),
                property.FindPropertyRelative("name"), GUIContent.none);
            useRegexProperty.boolValue = EditorGUI.Toggle(new Rect(regexLeft, row.y, REGEX_WIDTH, row.height),
                REGEX_LABEL_CONTENT, useRegexProperty.boolValue);
        }

        private static void DrawPatternRow(Rect row, SerializedProperty patternProperty)
        {
            Rect labelRect = new(row.x, row.y, PATTERN_LABEL_WIDTH, row.height);
            EditorGUI.LabelField(labelRect, "Pattern", EditorStyles.label);
            Rect fieldRect = new(row.x + PATTERN_FIELD_LEFT, row.y, row.width - PATTERN_FIELD_LEFT, row.height);
            EditorGUI.PropertyField(fieldRect, patternProperty, GUIContent.none);
        }

        private float GetElementHeight(SerializedProperty property)
        {
            float rowHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return property.FindPropertyRelative("useRegex").boolValue
                ? rowHeight * 2 + REGEX_ROW_EXTRA_HEIGHT
                : rowHeight;
        }

        private void DrawNewSectionForm(SelectiveLogsScriptableObject config)
        {
            GUILayout.Label("Add New Section", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            showNewSectionInput = EditorGUILayout.Foldout(showNewSectionInput, "New Section Input");
            if (showNewSectionInput)
                DrawNewSectionFields(config);

            EditorGUI.indentLevel--;
        }

        private void DrawNewSectionFields(SelectiveLogsScriptableObject config)
        {
            newSectionName = EditorGUILayout.TextField("Section Name", newSectionName);
            newSectionColor = EditorGUILayout.ColorField("Section Color", newSectionColor);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Section") && !string.IsNullOrEmpty(newSectionName))
            {
                config.AddUserDefinedSection(newSectionName, newSectionColor);
                ResetNewSectionForm();
            }

            if (GUILayout.Button("Cancel"))
                ResetNewSectionForm();

            EditorGUILayout.EndHorizontal();
        }

        private void ResetNewSectionForm()
        {
            newSectionName = "";
            newSectionColor = Color.white;
            showNewSectionInput = false;
        }
    }
}
#endif

