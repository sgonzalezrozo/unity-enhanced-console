#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;

namespace EnhancedConsole.Editor.Window.LogList
{
    /// <summary>
    /// What can be done with logs in the list: open, ping, copy and delete.
    /// </summary>
    internal sealed class LogActions
    {
        private readonly ConsoleContext context;

        public LogActions(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void OpenSource(LogEntry log)
        {
            context.Navigator.Open(log);
        }

        public void PingContext(LogEntry log)
        {
            context.Navigator.Ping(log);
        }

        // Stack traces are included only while the Stack Trace panel is shown.
        public void CopySelected(List<LogEntry> logs)
        {
            List<LogEntry> selected = context.Selection.GetSelectedLogs(logs);
            bool includeStackTrace = context.Preferences.ShowStackTrace.Value;
            StringBuilder clipboard = new();
            for (int i = 0; i < selected.Count; i++)
            {
                if (i > 0)
                    clipboard.AppendLine();

                clipboard.Append(includeStackTrace
                    ? LogEntryFormatter.FormatWithStackTrace(selected[i])
                    : LogEntryFormatter.FormatMessage(selected[i]));
            }

            EditorGUIUtility.systemCopyBuffer = clipboard.ToString();
        }

        public void DeleteSelected(List<LogEntry> logs)
        {
            context.Logs.RemoveEntries(context.Selection.GetSelectedLogs(logs));
            context.Selection.Clear();
            context.Repaint();
        }
    }
}
#endif
