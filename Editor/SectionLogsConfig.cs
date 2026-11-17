#if UNITY_EDITOR
using UnityEngine;

namespace EnhancedConsole.Editor
{
    public static class SectionLogsConfig
    {
        public static readonly SelectiveLogSection[] Sections = new[]
        {
            new SelectiveLogSection("Initialization", new Color(0.3f, 0.9f, 0.6f)),
            new SelectiveLogSection("Localization", new Color(0.8f, 0.4f, 0.8f)),
            new SelectiveLogSection("UIScroller", new Color(0.3f, 0.7f, 0.9f)),
            new SelectiveLogSection("ScreenSystem", new Color(1.0f, 0.6f, 0.2f)),
            new SelectiveLogSection("UIScripting", new Color(0.2f, 0.6f, 1.0f)),
        };
    }
}
#endif

