#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Details
{
    /// <summary>
    /// The draggable bar above the details panel. The new height is saved once, when the drag ends.
    /// </summary>
    internal sealed class DetailsResizeHandle
    {
        private const float HANDLE_HEIGHT = 5f;
        private const float GRIP_SPACING = 4f;
        private const float GRIP_SIZE = 3f;
        private const float MIN_DETAILS_HEIGHT = 100f;

        private readonly ConsoleContext context;
        private bool isResizing;

        public DetailsResizeHandle(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void Draw()
        {
            float top = context.WindowHeight - context.Preferences.DetailsHeight.Value - HANDLE_HEIGHT;
            Rect handleRect = new(0, top, context.WindowWidth, HANDLE_HEIGHT);
            EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);
            DrawGrip(handleRect);
            HandleDrag(handleRect);
        }

        private void DrawGrip(Rect handleRect)
        {
            float centerY = handleRect.center.y;
            float centerX = handleRect.center.x;
            EditorGUI.DrawRect(new Rect(0, centerY - 0.5f, handleRect.width, 1f), context.Colors.SeparatorLine);

            for (int i = -1; i <= 1; i++)
            {
                Rect gripRect = new(centerX + i * GRIP_SPACING - GRIP_SIZE * 0.5f, centerY - GRIP_SIZE * 0.5f,
                    GRIP_SIZE, GRIP_SIZE);
                EditorGUI.DrawRect(gripRect, context.Colors.GripHandle);
            }
        }

        private void HandleDrag(Rect handleRect)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && handleRect.Contains(currentEvent.mousePosition))
            {
                isResizing = true;
                currentEvent.Use();
            }

            if (!isResizing)
                return;

            if (currentEvent.type == EventType.MouseDrag)
                Resize(currentEvent);
            else if (currentEvent.type == EventType.MouseUp)
                EndResize(currentEvent);
        }

        private void Resize(Event currentEvent)
        {
            float maxHeight = context.WindowHeight - WindowLayout.TOOLBAR_HEIGHT - WindowLayout.MIN_LOG_LIST_HEIGHT -
                              WindowLayout.BOTTOM_MARGIN;
            float requestedHeight = context.WindowHeight - currentEvent.mousePosition.y;
            float height = Mathf.Clamp(requestedHeight, MIN_DETAILS_HEIGHT, maxHeight);
            context.Preferences.DetailsHeight.SetUnsaved(height);
            context.Repaint();
            currentEvent.Use();
        }

        // Writing EditorPrefs on every drag event is slow, so the height is saved once at the end.
        private void EndResize(Event currentEvent)
        {
            isResizing = false;
            context.Preferences.DetailsHeight.Save();
            currentEvent.Use();
        }
    }
}
#endif
