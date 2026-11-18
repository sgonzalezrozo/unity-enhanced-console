#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// A popup under the section filter button that turns sections on and off. Edits each section's enabled
    /// state directly and reports every change; closes on Escape, a click outside or losing focus.
    /// </summary>
    public class SectionLogsMenuWindow : EditorWindow
    {
        private const float ITEM_HEIGHT = 20f;
        private const float COLOR_BOX_SIZE = 12f;
        private const float COLOR_BOX_GAP = 6f;
        private const float SPACING = 4f;
        private const float WINDOW_WIDTH = 200f;
        private const float MAX_WINDOW_HEIGHT = 400f;
        private const float FRAME_HEIGHT = 40f;
        private const int BUTTON_ROWS = 2;
        private static readonly Color BACKGROUND_COLOR = new(0.2f, 0.2f, 0.2f, 1f);

        private static SectionLogsMenuWindow currentInstance;

        private SelectiveLogSection[] sections;
        private Action onChanged;
        private Vector2 scrollPosition;

        public static void Show(Rect buttonRect, SelectiveLogSection[] sectionConfigs, Action onChangedCallback)
        {
            if (currentInstance != null)
                currentInstance.Close();

            currentInstance = CreateInstance<SectionLogsMenuWindow>();
            currentInstance.sections = sectionConfigs;
            currentInstance.onChanged = onChangedCallback;

            float height = (sectionConfigs.Length + BUTTON_ROWS) * ITEM_HEIGHT + FRAME_HEIGHT;
            Vector2 size = new(WINDOW_WIDTH, Mathf.Min(height, MAX_WINDOW_HEIGHT));
            currentInstance.position = new Rect(new Vector2(buttonRect.x, buttonRect.yMax), size);
            currentInstance.ShowPopup();
            currentInstance.Focus();
        }

        private void OnGUI()
        {
            if (sections == null)
            {
                Close();
                return;
            }

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), BACKGROUND_COLOR);
            DrawContentSafely();
            CloseOnEscapeOrClickOutside();
        }

        private void OnLostFocus()
        {
            Close();
        }

        private void OnDestroy()
        {
            if (currentInstance == this)
                currentInstance = null;
        }

        private void DrawContentSafely()
        {
            try
            {
                GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                GUILayout.Space(SPACING);
                DrawBulkButtons();
                GUILayout.Space(SPACING);
                DrawSectionToggles();
                GUILayout.Space(SPACING);
                GUILayout.EndVertical();
            }
            catch (ExitGUIException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[EnhancedConsole] Section menu failed: {exception.Message}\n{exception.StackTrace}");
                Close();
            }
        }

        private void DrawBulkButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(SPACING);
            if (GUILayout.Button("Enable All", EditorStyles.miniButtonLeft, GUILayout.Height(ITEM_HEIGHT)))
                SetAll(true);

            if (GUILayout.Button("Disable All", EditorStyles.miniButtonRight, GUILayout.Height(ITEM_HEIGHT)))
                SetAll(false);

            GUILayout.Space(SPACING);
            GUILayout.EndHorizontal();
        }

        private void DrawSectionToggles()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            foreach (SelectiveLogSection section in sections)
            {
                if (section != null && section.name != null)
                    DrawSectionToggle(section);
            }

            GUILayout.EndScrollView();
        }

        private void DrawSectionToggle(SelectiveLogSection section)
        {
            Rect lineRect = EditorGUILayout.GetControlRect(false, ITEM_HEIGHT);
            lineRect.x += SPACING;
            lineRect.width -= 2 * SPACING;

            float colorTop = lineRect.y + (ITEM_HEIGHT - COLOR_BOX_SIZE) * 0.5f;
            EditorGUI.DrawRect(new Rect(lineRect.x, colorTop, COLOR_BOX_SIZE, COLOR_BOX_SIZE), section.color);

            float toggleLeft = lineRect.x + COLOR_BOX_SIZE + COLOR_BOX_GAP;
            Rect toggleRect = new(toggleLeft, lineRect.y, lineRect.xMax - toggleLeft, ITEM_HEIGHT);
            bool enabled = EditorGUI.Toggle(toggleRect, section.name, section.enabled);
            if (enabled == section.enabled)
                return;

            section.enabled = enabled;
            onChanged?.Invoke();
        }

        private void SetAll(bool enabled)
        {
            foreach (SelectiveLogSection section in sections)
            {
                if (section != null && section.name != null)
                    section.enabled = enabled;
            }

            onChanged?.Invoke();
        }

        private void CloseOnEscapeOrClickOutside()
        {
            Event currentEvent = Event.current;
            bool clickedOutside = currentEvent.type == EventType.MouseDown &&
                                  !IsInsideWindow(currentEvent.mousePosition);
            bool pressedEscape = currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape;
            if (clickedOutside || pressedEscape)
                Close();
        }

        private bool IsInsideWindow(Vector2 mousePosition)
        {
            return new Rect(0, 0, position.width, position.height).Contains(mousePosition);
        }
    }
}
#endif
