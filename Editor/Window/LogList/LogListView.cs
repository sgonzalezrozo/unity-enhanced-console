#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.LogList
{
    /// <summary>
    /// The scrolling log list. Draws only the visible rows, follows new logs while scrolled to the bottom, and
    /// keeps the selection in view after keyboard navigation.
    /// </summary>
    internal sealed class LogListView
    {
        private readonly ConsoleContext context;
        private readonly LogRowRenderer rowRenderer;
        private Vector2 scrollPosition;
        private bool followNewest = true;
        private bool scrollToSelection;
        private float visibleHeight;
        private Rect listRect;

        public LogListView(ConsoleContext consoleContext)
        {
            context = consoleContext;
            rowRenderer = new LogRowRenderer(context);
        }

        public int PageSize => Mathf.Max(1, Mathf.FloorToInt(visibleHeight / context.Styles.RowHeight) - 1);

        public void RequestScrollToSelection()
        {
            scrollToSelection = true;
        }

        public int GetIndexAt(Vector2 mousePosition)
        {
            if (!listRect.Contains(mousePosition))
                return -1;

            return (int)((mousePosition.y - listRect.y + scrollPosition.y) / context.Styles.RowHeight);
        }

        public void Draw(List<LogEntry> logs)
        {
            float contentHeight = logs.Count * context.Styles.RowHeight;
            Rect rect = GUILayoutUtility.GetRect(0, GetListHeight(contentHeight), GUILayout.ExpandWidth(true));

            // During Layout, GetRect returns a placeholder rect, so the height from the last real event is used.
            bool hasRealRect = Event.current.type != EventType.Layout;
            float viewHeight = hasRealRect ? rect.height : visibleHeight;
            float maxScroll = Mathf.Max(0f, contentHeight - viewHeight);
            if (hasRealRect)
                UpdateScrollTarget(rect, maxScroll);

            DrawRows(logs, rect, contentHeight);

            // Scrolling up stops following new logs; scrolling back to the bottom resumes it. This must run on
            // Layout too: the scroll view applies mouse wheel and trackpad scrolling in the event after the
            // scroll, which is usually Layout. Skipping it there let the next Repaint snap back to the bottom.
            if (viewHeight > 0f)
                followNewest = scrollPosition.y >= maxScroll - 1f;
        }

        private float GetListHeight(float contentHeight)
        {
            float reservedHeight = WindowLayout.TOOLBAR_HEIGHT + WindowLayout.BOTTOM_MARGIN;
            if (context.Preferences.ShowStackTrace.Value)
                reservedHeight += context.Preferences.DetailsHeight.Value;

            float availableHeight = context.WindowHeight - reservedHeight;
            return Mathf.Min(availableHeight, Mathf.Max(contentHeight, WindowLayout.MIN_LOG_LIST_HEIGHT));
        }

        private void UpdateScrollTarget(Rect rect, float maxScroll)
        {
            listRect = rect;
            visibleHeight = rect.height;

            if (scrollToSelection)
            {
                scrollToSelection = false;
                ScrollSelectionIntoView();
            }
            // Autoscroll: while the list is at the bottom, new logs keep it there.
            else if (followNewest)
                scrollPosition.y = maxScroll;
        }

        private void ScrollSelectionIntoView()
        {
            int selectedIndex = context.Selection.SelectedIndex;
            if (selectedIndex < 0)
                return;

            float rowTop = selectedIndex * context.Styles.RowHeight;
            float rowBottom = rowTop + context.Styles.RowHeight;
            if (rowTop < scrollPosition.y)
                scrollPosition.y = rowTop;
            else if (rowBottom > scrollPosition.y + visibleHeight)
                scrollPosition.y = rowBottom - visibleHeight;
        }

        private void DrawRows(List<LogEntry> logs, Rect rect, float contentHeight)
        {
            float rowWidth = rect.width - WindowLayout.SCROLLBAR_WIDTH;
            float rowHeight = context.Styles.RowHeight;
            try
            {
                scrollPosition = GUI.BeginScrollView(rect, scrollPosition, new Rect(0, 0, rowWidth, contentHeight));
                int first = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / rowHeight));
                int last = Mathf.Min(logs.Count - 1, first + Mathf.CeilToInt(rect.height / rowHeight) + 1);
                for (int i = first; i <= last; i++)
                    rowRenderer.Draw(logs[i], i, rowWidth);
            }
            finally
            {
                GUI.EndScrollView();
            }
        }
    }
}
#endif
