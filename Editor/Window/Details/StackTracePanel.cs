#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Details
{
    /// <summary>
    /// The selected log's stack trace, with clickable file links. Draws only the visible lines.
    /// </summary>
    internal sealed class StackTracePanel
    {
        private const float LINE_HEIGHT = 16f;
        private const float LABEL_HEIGHT = 24f;
        private const float LABEL_INDENT = 10f;
        private const float LABEL_GAP = 10f;
        private const float PANEL_PADDING = 4f;
        private const float SEPARATOR_OFFSET = 3f;

        private readonly ConsoleContext context;
        private Vector2 scrollPosition;
        private string cachedStackTrace;
        private string[] lines = Array.Empty<string>();
        private StackTraceLineInfo[] lineInfos = Array.Empty<StackTraceLineInfo>();

        public StackTracePanel(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void Draw(List<LogEntry> logs)
        {
            int selectedIndex = context.Selection.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= logs.Count)
                return;

            string stackTrace = logs[selectedIndex].stackTrace;
            if (string.IsNullOrEmpty(stackTrace))
                return;

            float height = context.Preferences.DetailsHeight.Value;
            Rect panelRect = new(0, context.WindowHeight - height, context.WindowWidth, height);
            DrawSeparator(panelRect.y);
            EditorGUI.LabelField(new Rect(LABEL_INDENT, panelRect.y, panelRect.width - LABEL_INDENT, LABEL_HEIGHT),
                "Stacktrace:");
            DrawStackTrace(panelRect, stackTrace);
        }

        private void DrawSeparator(float panelTop)
        {
            Color color = context.Colors.SeparatorLineDim;
            EditorGUI.DrawRect(new Rect(0, panelTop - SEPARATOR_OFFSET, context.WindowWidth, 1), color);
            EditorGUI.DrawRect(new Rect(0, panelTop - 1, context.WindowWidth, 1), color);
        }

        private void DrawStackTrace(Rect panelRect, string stackTrace)
        {
            float top = panelRect.y + LABEL_HEIGHT + LABEL_GAP;
            Rect backgroundRect = new(panelRect.x, top, panelRect.width, panelRect.yMax - top);
            EditorGUI.DrawRect(backgroundRect, context.Colors.StackTracePanelBackground);

            UpdateLines(stackTrace);
            Rect viewRect = new(backgroundRect.x + PANEL_PADDING, backgroundRect.y + PANEL_PADDING,
                backgroundRect.width - 2 * PANEL_PADDING, backgroundRect.height - 2 * PANEL_PADDING);
            float contentHeight = lines.Length * LINE_HEIGHT;
            Rect contentRect = new(0, 0, viewRect.width - WindowLayout.SCROLLBAR_WIDTH, contentHeight);
            try
            {
                scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, contentRect, false,
                    contentHeight > viewRect.height);
                DrawVisibleLines(viewRect);
            }
            finally
            {
                GUI.EndScrollView();
            }
        }

        // Splitting and parsing (which includes an AssetDatabase lookup per line) happen once per selected stack
        // trace, not once per line on every GUI event.
        private void UpdateLines(string stackTrace)
        {
            if (stackTrace == cachedStackTrace)
                return;

            cachedStackTrace = stackTrace;
            lines = stackTrace.Split('\n');
            lineInfos = new StackTraceLineInfo[lines.Length];
            for (int i = 0; i < lines.Length; i++)
                lineInfos[i] = context.Navigator.ParseLine(lines[i]);
        }

        private void DrawVisibleLines(Rect viewRect)
        {
            int first = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / LINE_HEIGHT));
            int last = Mathf.Min(lines.Length - 1, first + Mathf.CeilToInt(viewRect.height / LINE_HEIGHT) + 1);
            float lineWidth = viewRect.width - WindowLayout.SCROLLBAR_WIDTH;
            for (int i = first; i <= last; i++)
            {
                if (!string.IsNullOrEmpty(lines[i]))
                    DrawLine(new Rect(0, i * LINE_HEIGHT, lineWidth, LINE_HEIGHT), i);
            }
        }

        private void DrawLine(Rect lineRect, int index)
        {
            StackTraceLineInfo info = lineInfos[index];
            if (!info.hasFileInfo)
            {
                GUI.Label(lineRect, lines[index], context.Styles.StackTraceLine);
                return;
            }

            if (GUI.Button(lineRect, "", GUIStyle.none))
                context.Navigator.OpenFile(info.filePath, info.lineNumber);

            EditorGUIUtility.AddCursorRect(lineRect, MouseCursor.Link);
            GUI.Label(lineRect, lines[index], context.Styles.StackTraceLink);
        }
    }
}
#endif
