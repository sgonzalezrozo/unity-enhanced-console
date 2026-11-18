#if UNITY_EDITOR
using EnhancedConsole.Editor.Preferences;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// Lays out the toolbar: Clear, Collapse, Error Pause, Stack Trace, Time and Recompile on the left, then
    /// search, log type toggles and the section filter on the right.
    /// </summary>
    internal sealed class ConsoleToolbar
    {
        private static readonly GUIContent CLEAR_CONTENT = new("Clear", "Clear all logs");
        private static readonly GUIContent ERROR_PAUSE_CONTENT =
            new("Error Pause", "Pause Play Mode when an error, exception or failed assert is logged");

        private readonly ConsoleContext context;
        private readonly ConsolePreferences preferences;
        private readonly ToolbarToggle collapseToggle = new(new GUIContent("Collapse"), ToolbarControls.BUTTON_WIDTH);
        private readonly ToolbarToggle errorPauseToggle = new(ERROR_PAUSE_CONTENT, ToolbarControls.WIDE_BUTTON_WIDTH);
        private readonly ToolbarToggle stackTraceToggle =
            new(new GUIContent("Stack Trace"), ToolbarControls.WIDE_BUTTON_WIDTH);
        private readonly ToolbarToggle timeToggle = new(new GUIContent("Time"), ToolbarControls.NARROW_BUTTON_WIDTH);
        private readonly TimeModeMenu timeModeMenu;
        private readonly RecompileButton recompileButton;
        private readonly SearchField searchField;
        private readonly LogTypeToggles logTypeToggles;
        private readonly SectionFilterButton sectionFilterButton;

        public ConsoleToolbar(ConsoleContext consoleContext)
        {
            context = consoleContext;
            preferences = context.Preferences;
            timeModeMenu = new TimeModeMenu(context);
            recompileButton = new RecompileButton(preferences);
            searchField = new SearchField(context.Filters);
            logTypeToggles = new LogTypeToggles(context);
            sectionFilterButton = new SectionFilterButton(context);
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawLeftControls();
            GUILayout.FlexibleSpace();
            searchField.Draw();
            logTypeToggles.Draw();
            sectionFilterButton.Draw();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftControls()
        {
            DrawClearControls();
            DrawCollapseControls();
            DrawPreferenceToggle(errorPauseToggle, preferences.ErrorPause);
            DrawPreferenceToggle(stackTraceToggle, preferences.ShowStackTrace);
            DrawPreferenceToggle(timeToggle, preferences.ShowTime);
            DrawTimeModeMenu();

            if (!Application.isPlaying)
                recompileButton.Draw();
        }

        private void DrawClearControls()
        {
            if (GUILayout.Button(CLEAR_CONTENT, EditorStyles.toolbarButton,
                    GUILayout.Width(ToolbarControls.BUTTON_WIDTH)))
                context.ClearLogs();

            Rect dropdownRect = ToolbarControls.ReserveDropdownRect();
            if (!ToolbarControls.DrawDropdownButton(dropdownRect))
                return;

            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Clear on Play"), preferences.ClearOnPlay.Value,
                preferences.ClearOnPlay.Toggle);
            menu.AddItem(new GUIContent("Clear on Build"), preferences.ClearOnBuild.Value,
                preferences.ClearOnBuild.Toggle);
            menu.AddItem(new GUIContent("Clear on Recompile"), preferences.ClearOnRecompile.Value,
                preferences.ClearOnRecompile.Toggle);
            menu.DropDown(dropdownRect);
        }

        private void DrawCollapseControls()
        {
            bool isCollapsed = context.Filters.IsCollapseEnabled;
            bool collapse = collapseToggle.Draw(isCollapsed, GetToggleColor(isCollapsed));
            if (collapse != isCollapsed)
            {
                context.Filters.SetCollapse(collapse);
                preferences.Collapse.Set(collapse);
                context.Logs.RebuildWithCollapse();
            }

            Rect dropdownRect = ToolbarControls.ReserveDropdownRect();
            if (!ToolbarControls.DrawDropdownButton(dropdownRect))
                return;

            GenericMenu menu = new();
            menu.AddItem(new GUIContent("Exact Stacktrace"), context.Filters.IsCollapseExactStacktraceEnabled,
                ToggleCollapseExactStackTrace);
            menu.DropDown(dropdownRect);
        }

        private void ToggleCollapseExactStackTrace()
        {
            bool exact = !context.Filters.IsCollapseExactStacktraceEnabled;
            context.Filters.SetCollapseExactStacktrace(exact);
            preferences.CollapseExactStackTrace.Set(exact);
            context.Logs.RebuildWithCollapse();
        }

        private void DrawPreferenceToggle(ToolbarToggle toggle, BoolPreference preference)
        {
            preference.Set(toggle.Draw(preference.Value, GetToggleColor(preference.Value)));
        }

        private void DrawTimeModeMenu()
        {
            Rect dropdownRect = ToolbarControls.ReserveDropdownRect();
            if (!ToolbarControls.DrawDropdownButton(dropdownRect))
                return;

            GenericMenu menu = new();
            timeModeMenu.AddItems(menu, "");
            menu.DropDown(dropdownRect);
        }

        private Color GetToggleColor(bool isOn)
        {
            return isOn ? context.Colors.ToggleButtonActive : Color.white;
        }
    }
}
#endif
