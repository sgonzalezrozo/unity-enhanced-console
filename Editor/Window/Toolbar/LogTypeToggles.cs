#if UNITY_EDITOR
using UnityEngine;

namespace EnhancedConsole.Editor.Window.Toolbar
{
    /// <summary>
    /// The info, warning and error toggles with their counts.
    /// </summary>
    internal sealed class LogTypeToggles
    {
        private readonly ConsoleContext context;
        private readonly CountToggle infoToggle = new(LogType.Log);
        private readonly CountToggle warningToggle = new(LogType.Warning);
        private readonly CountToggle errorToggle = new(LogType.Error);

        public LogTypeToggles(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void Draw()
        {
            LogFilterManager filters = context.Filters;
            filters.SetShowInfo(infoToggle.Draw(filters.IsShowingInfo, context.Logs.InfoCount,
                GetColor(filters.IsShowingInfo)));
            filters.SetShowWarnings(warningToggle.Draw(filters.IsShowingWarnings, context.Logs.WarningCount,
                GetColor(filters.IsShowingWarnings)));
            filters.SetShowErrors(errorToggle.Draw(filters.IsShowingErrors, context.Logs.ErrorCount,
                GetColor(filters.IsShowingErrors)));
        }

        private Color GetColor(bool isShowing)
        {
            return isShowing ? Color.white : context.Colors.DisabledButton;
        }
    }
}
#endif
