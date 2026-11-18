#if UNITY_EDITOR
using EnhancedConsole.Editor.Preferences;
using EnhancedConsole.Editor.Window.LogList;
using EnhancedConsole.Editor.Window.Toolbar;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window
{
    /// <summary>
    /// Items in the window's ⋮ menu, like Unity's Console: lines per log, time mode, monospace and rich text.
    /// </summary>
    internal sealed class WindowMenu
    {
        private readonly ConsoleContext context;
        private readonly LogListView listView;
        private readonly TimeModeMenu timeModeMenu;

        public WindowMenu(ConsoleContext consoleContext, LogListView logListView)
        {
            context = consoleContext;
            listView = logListView;
            timeModeMenu = new TimeModeMenu(context);
        }

        public void AddItems(GenericMenu menu)
        {
            ConsolePreferences preferences = context.Preferences;
            for (int lines = 1; lines <= ConsolePreferences.MAX_LOG_ENTRY_LINES; lines++)
            {
                GUIContent label = new($"Log Entry/{lines} {(lines == 1 ? "Line" : "Lines")}");
                menu.AddItem(label, preferences.LogEntryLines == lines, OnLogEntryLinesSelected, lines);
            }

            timeModeMenu.AddItems(menu, "Time/");
            menu.AddItem(new GUIContent("Use Monospace Font"), preferences.Monospace.Value, ToggleMonospace);
            menu.AddItem(new GUIContent("Rich Text"), preferences.RichText.Value, ToggleRichText);
        }

        private void OnLogEntryLinesSelected(object lines)
        {
            context.Preferences.SetLogEntryLines((int)lines);
            listView.RequestScrollToSelection();
            RebuildStyles();
        }

        private void ToggleMonospace()
        {
            context.Preferences.Monospace.Toggle();
            RebuildStyles();
        }

        private void ToggleRichText()
        {
            context.Preferences.RichText.Toggle();
            RebuildStyles();
        }

        private void RebuildStyles()
        {
            context.Styles.Invalidate();
            context.Repaint();
        }
    }
}
#endif
