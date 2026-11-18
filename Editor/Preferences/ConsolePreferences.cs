#if UNITY_EDITOR
using System;
using UnityEngine;

namespace EnhancedConsole.Editor.Preferences
{
    /// <summary>
    /// Every Enhanced Console setting that's kept between sessions. Create it from OnEnable or later:
    /// EditorPrefs can't be read while Unity constructs the window.
    /// </summary>
    internal sealed class ConsolePreferences
    {
        public const string CLEAR_ON_BUILD_KEY = "EnhancedConsole_ClearOnBuild";
        public const int MAX_LOG_ENTRY_LINES = 10;
        private const float DEFAULT_DETAILS_HEIGHT = 200f;

        private readonly IntPreference timeMode =
            new("EnhancedConsole_TimeMode", (int)TimeDisplayMode.SinceStartup);
        private readonly IntPreference logEntryLines = new("EnhancedConsole_LogEntryLines", 1);
        private readonly IntPreference sectionFilterMode =
            new("EnhancedConsole_SL_FilterMode", (int)Editor.SectionFilterMode.StackAndContent);

        public BoolPreference ClearOnPlay { get; } = new("EnhancedConsole_ClearOnPlay", false);
        public BoolPreference ClearOnBuild { get; } = new(CLEAR_ON_BUILD_KEY, false);
        public BoolPreference ClearOnRecompile { get; } = new("EnhancedConsole_ClearOnRecompile", false);
        public BoolPreference Collapse { get; } = new("EnhancedConsole_Collapse", false);
        public BoolPreference CollapseExactStackTrace { get; } =
            new("EnhancedConsole_CollapseExactStacktrace", false);
        public BoolPreference ShowTime { get; } = new("EnhancedConsole_ShowTime", false);
        public BoolPreference ShowStackTrace { get; } = new("EnhancedConsole_ShowStackTrace", false);
        public BoolPreference RecompileWithAssetRefresh { get; } = new("EnhancedConsole_RecompileMode", false);
        public BoolPreference ErrorPause { get; } = new("EnhancedConsole_ErrorPause", false);
        public BoolPreference Monospace { get; } = new("EnhancedConsole_Monospace", false);
        public BoolPreference RichText { get; } = new("EnhancedConsole_RichText", true);
        public BoolPreference SectionFilterActive { get; } = new("EnhancedConsole_SL_Active", false);
        public FloatPreference DetailsHeight { get; } = new("EnhancedConsole_DetailsHeight", DEFAULT_DETAILS_HEIGHT);

        public TimeDisplayMode TimeMode => Enum.IsDefined(typeof(TimeDisplayMode), timeMode.Value)
            ? (TimeDisplayMode)timeMode.Value
            : TimeDisplayMode.SinceStartup;

        public int LogEntryLines => Mathf.Clamp(logEntryLines.Value, 1, MAX_LOG_ENTRY_LINES);

        public SectionFilterMode SectionFilterMode => (SectionFilterMode)sectionFilterMode.Value;

        public void SetTimeMode(TimeDisplayMode mode)
        {
            timeMode.Set((int)mode);
        }

        public void SetLogEntryLines(int lines)
        {
            logEntryLines.Set(Mathf.Clamp(lines, 1, MAX_LOG_ENTRY_LINES));
        }

        public void SetSectionFilterMode(SectionFilterMode mode)
        {
            sectionFilterMode.Set((int)mode);
        }
    }
}
#endif
