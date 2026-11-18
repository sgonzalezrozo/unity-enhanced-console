#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// The search field, focused with Ctrl+F or Cmd+F.
    /// </summary>
    internal sealed class SearchField
    {
        private const string CONTROL_NAME = "EnhancedConsoleSearch";
        private const float LABEL_WIDTH = 60f;
        private const float FIELD_WIDTH = 200f;
        private const float CLEAR_BUTTON_WIDTH = 20f;
        private static readonly GUIContent LABEL_CONTENT = new("Search:", "Ctrl+F or Cmd+F");

        private readonly LogFilterManager filters;

        public SearchField(LogFilterManager filterManager)
        {
            filters = filterManager;
        }

        public static void Focus()
        {
            EditorGUI.FocusTextInControl(CONTROL_NAME);
        }

        // Edit > Find (Ctrl+F or Cmd+F) arrives as a command when the window has focus.
        public static void HandleFindCommand(ConsoleContext context)
        {
            Event currentEvent = Event.current;
            bool isCommand = currentEvent.type is EventType.ValidateCommand or EventType.ExecuteCommand;
            if (!isCommand || currentEvent.commandName != "Find")
                return;

            if (currentEvent.type == EventType.ExecuteCommand)
            {
                Focus();
                context.Repaint();
            }

            currentEvent.Use();
        }

        public void Draw()
        {
            GUILayout.Label(LABEL_CONTENT, EditorStyles.miniLabel, GUILayout.Width(LABEL_WIDTH));
            GUI.SetNextControlName(CONTROL_NAME);
            string search = EditorGUILayout.TextField(filters.GetSearchFilter(), EditorStyles.toolbarSearchField,
                GUILayout.Width(FIELD_WIDTH));
            filters.SetSearchFilter(search);

            if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(CLEAR_BUTTON_WIDTH)))
                filters.SetSearchFilter("");
        }
    }
}
#endif
