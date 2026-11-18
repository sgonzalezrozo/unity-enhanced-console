#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// A toolbar toggle button tinted with a background color.
    /// </summary>
    internal sealed class ToolbarToggle
    {
        private readonly GUIContent content;
        private readonly GUILayoutOption width;

        public ToolbarToggle(GUIContent toggleContent, float toggleWidth)
        {
            content = toggleContent;
            width = GUILayout.Width(toggleWidth);
        }

        public GUIContent Content => content;

        public bool Draw(bool value, Color backgroundColor)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            bool newValue = GUILayout.Toggle(value, content, EditorStyles.toolbarButton, width);
            GUI.backgroundColor = originalColor;
            return newValue;
        }
    }
}
#endif
