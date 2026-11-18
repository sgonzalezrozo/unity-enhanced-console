#if UNITY_EDITOR
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// A log type toggle showing its icon and count. The text is rebuilt only when the count changes.
    /// </summary>
    internal sealed class CountToggle
    {
        private readonly LogType type;
        private readonly ToolbarToggle toggle = new(new GUIContent(), ToolbarControls.NARROW_BUTTON_WIDTH);
        private int shownCount = -1;

        public CountToggle(LogType logType)
        {
            type = logType;
        }

        public bool Draw(bool value, int count, Color backgroundColor)
        {
            UpdateContent(count);
            return toggle.Draw(value, backgroundColor);
        }

        private void UpdateContent(int count)
        {
            // Assigned every time: the icon can be unloaded by a skin change.
            toggle.Content.image = LogEntryFormatter.GetIconForLogType(type);
            if (count == shownCount)
                return;

            shownCount = count;
            toggle.Content.text = " " + count;
        }
    }
}
#endif
