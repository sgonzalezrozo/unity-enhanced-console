#if UNITY_EDITOR
using System.Collections.Generic;
using EnhancedConsole.Editor.LogManagement;
using EnhancedConsole.Editor.Preferences;
using UnityEditor;

namespace EnhancedConsole.Editor.Window
{
    /// <summary>
    /// Creates the console's shared state and services once per window, and holds the commands that several
    /// parts of the window use.
    /// </summary>
    internal sealed class ConsoleContext
    {
        private readonly EditorWindow window;

        public ConsoleContext(EditorWindow ownerWindow)
        {
            window = ownerWindow;
            Preferences = new ConsolePreferences();
            Sections = new SectionedLogOrganizer();
            Filters = new LogFilterManager(Sections);
            Logs = new LogEntryManager(Filters);
            Styles = new ConsoleStyles(Preferences, Colors);
            ApplyFilterPreferences();
            LoadSectionConfig();
        }

        public ConsolePreferences Preferences { get; }
        public SectionedLogOrganizer Sections { get; }
        public LogFilterManager Filters { get; }
        public LogEntryManager Logs { get; }
        public ConsoleStyles Styles { get; }
        public LogLevelColorScheme Colors { get; } = new();
        public LogSelection Selection { get; } = new();
        public RepaintScheduler Repaints { get; } = new();
        public SourceNavigator Navigator { get; } = new();
        public SelectiveLogsScriptableObject SectionConfig { get; private set; }
        public float WindowWidth => window.position.width;
        public float WindowHeight => window.position.height;

        // The cached filtered view; cheap to call several times per GUI event. Don't modify the list.
        public List<LogEntry> GetFilteredLogs()
        {
            List<LogEntry> logs = Logs.GetFilteredEntries();
            Selection.UpdateForView(logs, Logs.FilteredResetCount);
            return logs;
        }

        public void ClearLogs()
        {
            UnityConsoleBridge.Clear();
            // Like Unity's Clear, compiler errors stay until the code compiles.
            Logs.ClearKeepingCompilerErrors();
            Selection.Clear();
            Repaint();
        }

        public void LoadSectionConfig()
        {
            SectionConfig = SelectiveLogsScriptableObject.FindConfig();
            if (SectionConfig == null)
            {
                Sections.SetFilterActive(false);
                return;
            }

            Sections.Initialize(SectionConfig);
        }

        public void Repaint()
        {
            if (window != null)
                window.Repaint();
        }

        private void ApplyFilterPreferences()
        {
            Filters.SetCollapse(Preferences.Collapse.Value);
            Filters.SetCollapseExactStacktrace(Preferences.CollapseExactStackTrace.Value);
            Sections.SetFilterMode(Preferences.SectionFilterMode);
            Sections.SetFilterActive(Preferences.SectionFilterActive.Value);
        }
    }
}
#endif
