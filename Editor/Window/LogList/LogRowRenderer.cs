#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.LogList
{
    /// <summary>
    /// Draws one log row: background, section bubble, icon, message, time and collapse count.
    /// </summary>
    internal sealed class LogRowRenderer
    {
        private const float BUBBLE_LEFT = 2f;
        private const float ICON_LEFT = 15f;
        private const float ICON_WIDTH = 16f;
        private const float ICON_TEXT_GAP = 5f;
        private const float RIGHT_PADDING = 5f;
        private const float COUNT_CIRCLE_SIZE = 20f;
        private const float TEXT_LEFT = ICON_LEFT + ICON_WIDTH + ICON_TEXT_GAP;
        private static readonly Vector2 BUBBLE_SIZE = new(8f, 12f);
        private static readonly Vector2 ICON_SIZE = new(ICON_WIDTH, ICON_WIDTH);
        private static readonly Vector2 COUNT_CIRCLE_SIZE_2D = new(COUNT_CIRCLE_SIZE, COUNT_CIRCLE_SIZE);

        private readonly ConsoleContext context;
        private readonly SearchHighlighter highlighter;

        public LogRowRenderer(ConsoleContext consoleContext)
        {
            context = consoleContext;
            highlighter = new SearchHighlighter(context);
        }

        public void Draw(LogEntry log, int index, float width)
        {
            Rect rowRect = new(0, index * context.Styles.RowHeight, width, context.Styles.RowHeight);
            EditorGUI.DrawRect(rowRect, GetBackgroundColor(index));
            DrawSectionBubble(rowRect, log);
            DrawIcon(rowRect, log);
            DrawMessage(rowRect, log);
            DrawTime(rowRect, log);
            DrawCollapseCount(rowRect, log, index);
        }

        private Color GetBackgroundColor(int index)
        {
            LogLevelColorScheme colors = context.Colors;
            if (context.Selection.IsSelected(index))
                return colors.SelectedRowBackground;

            return index % 2 == 0 ? colors.EvenRowBackground : colors.OddRowBackground;
        }

        private void DrawSectionBubble(Rect rowRect, LogEntry log)
        {
            SelectiveLogSection section = context.Sections.GetSectionForLog(log);
            if (section != null)
                EditorGUI.DrawRect(CenterVertically(rowRect, BUBBLE_LEFT, BUBBLE_SIZE), section.color);
        }

        private void DrawIcon(Rect rowRect, LogEntry log)
        {
            Rect iconRect = CenterVertically(rowRect, ICON_LEFT, ICON_SIZE);
            GUI.DrawTexture(iconRect, LogEntryFormatter.GetIconForLogType(log.type));
        }

        private void DrawMessage(Rect rowRect, LogEntry log)
        {
            float timeWidth = context.Preferences.ShowTime.Value ? GetTimeColumnWidth() : 0f;
            float textWidth = rowRect.width - TEXT_LEFT - RIGHT_PADDING - COUNT_CIRCLE_SIZE - timeWidth;
            Rect textRect = new(rowRect.x + TEXT_LEFT, rowRect.y, textWidth, rowRect.height);

            string displayText = log.GetDisplayText(context.Preferences.LogEntryLines);
            highlighter.Draw(textRect, displayText);
            GUI.Label(textRect, displayText, context.Styles.Message);
        }

        private void DrawTime(Rect rowRect, LogEntry log)
        {
            if (!context.Preferences.ShowTime.Value)
                return;

            float timeWidth = GetTimeColumnWidth();
            float left = rowRect.xMax - RIGHT_PADDING - COUNT_CIRCLE_SIZE - timeWidth;
            Rect timeRect = new(left, rowRect.y, timeWidth - RIGHT_PADDING, rowRect.height);
            GUI.Label(timeRect, log.GetTimeText(context.Preferences.TimeMode), context.Styles.Time);
        }

        private void DrawCollapseCount(Rect rowRect, LogEntry log, int index)
        {
            if (!context.Filters.IsCollapseEnabled || log.count <= 1)
                return;

            float left = rowRect.width - RIGHT_PADDING - COUNT_CIRCLE_SIZE;
            Rect circleRect = CenterVertically(rowRect, left, COUNT_CIRCLE_SIZE_2D);
            EditorGUI.DrawRect(circleRect, GetCountCircleColor(index));
            GUI.Label(circleRect, log.GetCountText(), context.Styles.Count);
        }

        private Color GetCountCircleColor(int index)
        {
            LogLevelColorScheme colors = context.Colors;
            if (context.Selection.SelectedIndex == index)
                return colors.SelectedCountCircle;

            return index % 2 == 0 ? colors.CountCircleBackground : colors.CountCircleBackgroundDark;
        }

        private float GetTimeColumnWidth()
        {
            return context.Styles.GetTimeColumnWidth(context.Preferences.TimeMode);
        }

        private static Rect CenterVertically(Rect rowRect, float left, Vector2 size)
        {
            return new Rect(rowRect.x + left, rowRect.y + (rowRect.height - size.y) * 0.5f, size.x, size.y);
        }
    }
}
#endif
