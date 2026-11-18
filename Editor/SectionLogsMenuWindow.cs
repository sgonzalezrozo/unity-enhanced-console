#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    public class SectionLogsMenuWindow : EditorWindow
    {
        private static SectionLogsMenuWindow currentInstance;

        private SelectiveLogSection[] sections;
        private Dictionary<string, bool> sectionStates;
        private Action onChanged;
        private Vector2 scrollPosition;
        private const float ITEM_HEIGHT = 20f;
        private const float COLOR_BOX_SIZE = 12f;

        public static void Show(Rect buttonRect, SelectiveLogSection[] sectionConfigs, Dictionary<string, bool> states,
            Action onChangedCallback)
        {
            if (currentInstance != null)
            {
                currentInstance.Close();
                currentInstance = null;
            }

            SectionLogsMenuWindow window = CreateInstance<SectionLogsMenuWindow>();
            currentInstance = window;

            window.sections = sectionConfigs;
            window.sectionStates = states;
            window.onChanged = onChangedCallback;

            float height = (sectionConfigs.Length + 2) * ITEM_HEIGHT + 40;
            Vector2 size = new(200, Mathf.Min(height, 400));

            Vector2 position = new(buttonRect.x, buttonRect.yMax);
            window.position = new Rect(position, size);

            window.ShowPopup();
            window.Focus();
        }

        private void OnGUI()
        {
            if (sections == null || sectionStates == null)
            {
                Close();
                return;
            }

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0.2f, 0.2f, 0.2f, 1f));

            try
            {
                GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                GUILayout.Space(4);

                if (GUILayout.Button("Enable All", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
                {
                    SetAll(true);

                    onChanged?.Invoke();
                }

                if (GUILayout.Button("Disable All", EditorStyles.miniButtonRight, GUILayout.Height(20)))
                {
                    SetAll(false);

                    onChanged?.Invoke();
                }
                GUILayout.Space(4);
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

                foreach (SelectiveLogSection section in sections)
                {
                    if (section == null || section.name == null ||
                        !sectionStates.TryGetValue(section.name, out bool isEnabled))
                        continue;

                    Rect lineRect = EditorGUILayout.GetControlRect(false, ITEM_HEIGHT);
                    lineRect.x += 4;
                    lineRect.width -= 8;

                    Rect colorBoxRect = new(lineRect.x, lineRect.y + (ITEM_HEIGHT - COLOR_BOX_SIZE) * 0.5f,
                        COLOR_BOX_SIZE, COLOR_BOX_SIZE);
                    EditorGUI.DrawRect(colorBoxRect, section.color);

                    Rect toggleRect = new(lineRect.x + COLOR_BOX_SIZE + 6, lineRect.y,
                        lineRect.width - COLOR_BOX_SIZE - 6, ITEM_HEIGHT);
                    bool newState = EditorGUI.Toggle(toggleRect, section.name, isEnabled);
                    if (newState == isEnabled)
                        continue;

                    sectionStates[section.name] = newState;
                    onChanged?.Invoke();
                }

                GUILayout.EndScrollView();

                GUILayout.Space(4);
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
                return;
            }

            Event currentEvent = Event.current;
            switch (currentEvent.type)
            {
                case EventType.MouseDown when !IsInsideWindow(currentEvent.mousePosition):
                case EventType.KeyDown when currentEvent.keyCode == KeyCode.Escape:
                    Close();
                    break;
            }
        }

        private void SetAll(bool enabled)
        {
            foreach (SelectiveLogSection section in sections)
            {
                if (section != null && section.name != null)
                    sectionStates[section.name] = enabled;
            }
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

        private bool IsInsideWindow(Vector2 mousePosition)
        {
            return new Rect(0, 0, position.width, position.height).Contains(mousePosition);
        }
    }
}
#endif
