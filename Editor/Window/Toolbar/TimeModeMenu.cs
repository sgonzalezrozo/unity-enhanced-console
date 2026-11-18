#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// The time mode items, shown in the Time dropdown and in the window's ⋮ menu.
    /// </summary>
    internal sealed class TimeModeMenu
    {
        private readonly ConsoleContext context;

        public TimeModeMenu(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void AddItems(GenericMenu menu, string prefix)
        {
            AddItem(menu, prefix + "Since Startup", TimeDisplayMode.SinceStartup);
            AddItem(menu, prefix + "Wall Clock", TimeDisplayMode.WallClock);
            AddItem(menu, prefix + "Frame", TimeDisplayMode.Frame);
        }

        private void AddItem(GenericMenu menu, string label, TimeDisplayMode mode)
        {
            bool isCurrent = context.Preferences.TimeMode == mode;
            menu.AddItem(new GUIContent(label), isCurrent, OnItemSelected, mode);
        }

        private void OnItemSelected(object mode)
        {
            context.Preferences.SetTimeMode((TimeDisplayMode)mode);
            // Picking a time mode implies wanting to see it.
            context.Preferences.ShowTime.Set(true);
            context.Repaint();
        }
    }
}
#endif
