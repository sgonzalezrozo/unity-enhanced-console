#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.LogList
{
    /// <summary>
    /// Highlights the first search match in a row's visible text, behind the label.
    /// </summary>
    internal sealed class SearchHighlighter
    {
        private readonly ConsoleContext context;
        private readonly GUIContent measureContent = new();

        public SearchHighlighter(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void Draw(Rect textRect, string displayText)
        {
            string search = context.Filters.GetSearchFilter();
            if (Event.current.type != EventType.Repaint || !CanHighlight(search, displayText))
                return;

            int matchIndex = displayText.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
                return;

            float left = GetMatchLeft(textRect, displayText, matchIndex);
            if (left >= textRect.xMax)
                return;

            float width = Mathf.Min(MeasureTextWidth(displayText.Substring(matchIndex, search.Length)),
                textRect.xMax - left);
            float top = GetLineTop(textRect, displayText, matchIndex);
            Rect highlightRect = new(left, top, width, context.Styles.Message.lineHeight);
            EditorGUI.DrawRect(highlightRect, context.Colors.SearchHighlight);
        }

        // Rich text tags take no space when drawn, so positions inside tagged text can't be measured from the
        // raw string.
        private bool CanHighlight(string search, string displayText)
        {
            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(displayText))
                return false;

            return !context.Preferences.RichText.Value || displayText.IndexOf('<') < 0;
        }

        private float GetMatchLeft(Rect textRect, string displayText, int matchIndex)
        {
            int lineStart = matchIndex > 0 ? displayText.LastIndexOf('\n', matchIndex - 1) + 1 : 0;
            string textBeforeMatch = displayText.Substring(lineStart, matchIndex - lineStart);
            return textRect.x + context.Styles.Message.padding.left + MeasureTextWidth(textBeforeMatch);
        }

        private float GetLineTop(Rect textRect, string displayText, int matchIndex)
        {
            GUIStyle style = context.Styles.Message;
            int lineCount = CountNewlines(displayText, displayText.Length) + 1;
            float blockHeight = lineCount * style.lineHeight;
            float freeHeight = textRect.height - style.padding.vertical - blockHeight;
            float blockTop = textRect.y + style.padding.top + freeHeight * 0.5f;
            return blockTop + CountNewlines(displayText, matchIndex) * style.lineHeight;
        }

        private static int CountNewlines(string text, int end)
        {
            int count = 0;
            for (int i = 0; i < end; i++)
            {
                if (text[i] == '\n')
                    count++;
            }

            return count;
        }

        private float MeasureTextWidth(string text)
        {
            if (text.Length == 0)
                return 0f;

            GUIStyle style = context.Styles.Message;
            measureContent.text = text;
            return style.CalcSize(measureContent).x - style.padding.horizontal;
        }
    }
}
#endif
