#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// Sizes and the dropdown arrow shared by the toolbar's controls.
    /// </summary>
    internal static class ToolbarControls
    {
        public const float BUTTON_WIDTH = 70f;
        public const float WIDE_BUTTON_WIDTH = 80f;
        public const float NARROW_BUTTON_WIDTH = 50f;
        private const float DROPDOWN_ARROW_WIDTH = 15f;

        private static readonly GUIContent DROPDOWN_ARROW_CONTENT = new("▼");

        public static Rect ReserveDropdownRect()
        {
            return GUILayoutUtility.GetRect(DROPDOWN_ARROW_CONTENT, EditorStyles.toolbarDropDown,
                GUILayout.Width(DROPDOWN_ARROW_WIDTH));
        }

        public static bool DrawDropdownButton(Rect rect)
        {
            return GUI.Button(rect, DROPDOWN_ARROW_CONTENT, EditorStyles.toolbarDropDown);
        }
    }
}
#endif
