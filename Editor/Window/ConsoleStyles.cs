#if UNITY_EDITOR
using EnhancedConsole.Editor.Preferences;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window
{
    /// <summary>
    /// GUI styles and the row metrics that depend on them. Built on the first GUI event, since EditorStyles
    /// aren't ready before, and again after a display option changes.
    /// </summary>
    internal sealed class ConsoleStyles
    {
        private const float MIN_ROW_HEIGHT = 32f;
        private const float ROW_VERTICAL_PADDING = 10f;
        private const float TIME_COLUMN_PADDING = 10f;
        private const int STACK_TRACE_FONT_SIZE = 11;
        private const int MONOSPACE_FALLBACK_FONT_SIZE = 12;
        private const string MONOSPACE_FONT_PATH = "Fonts/RobotoMono/RobotoMono-Regular.ttf";
        private static readonly string[] MONOSPACE_FALLBACK_FONTS =
            { "Menlo", "Consolas", "DejaVu Sans Mono", "Courier New" };

        private static Font monospaceFont;

        private readonly ConsolePreferences preferences;
        private readonly LogLevelColorScheme colors;
        private bool isBuilt;
        private float timeColumnWidth = -1f;
        private TimeDisplayMode measuredTimeMode;

        public ConsoleStyles(ConsolePreferences consolePreferences, LogLevelColorScheme colorScheme)
        {
            preferences = consolePreferences;
            colors = colorScheme;
        }

        public GUIStyle Message { get; private set; }
        public GUIStyle Time { get; private set; }
        public GUIStyle Count { get; private set; }
        public GUIStyle StackTraceLine { get; private set; }
        public GUIStyle StackTraceLink { get; private set; }
        public float RowHeight { get; private set; } = MIN_ROW_HEIGHT;

        public void Invalidate()
        {
            isBuilt = false;
        }

        public void EnsureBuilt()
        {
            if (isBuilt)
                return;

            BuildStyles();
            timeColumnWidth = -1f;
            UpdateRowHeight();
            isBuilt = true;
        }

        public float GetTimeColumnWidth(TimeDisplayMode mode)
        {
            if (timeColumnWidth >= 0f && measuredTimeMode == mode)
                return timeColumnWidth;

            GUIContent sample = new(LogEntryFormatter.GetTimeSample(mode));
            timeColumnWidth = Time.CalcSize(sample).x + TIME_COLUMN_PADDING;
            measuredTimeMode = mode;
            return timeColumnWidth;
        }

        private void BuildStyles()
        {
            Message = CreateMessageStyle();
            Time = CreateLabelStyle(EditorStyles.miniLabel, TextAnchor.MiddleRight, colors.TextMuted);
            Count = CreateLabelStyle(EditorStyles.miniLabel, TextAnchor.MiddleCenter, colors.CountCircleText);
            Count.fontStyle = FontStyle.Bold;
            StackTraceLine = CreateStackTraceLineStyle();
            if (preferences.Monospace.Value)
                ApplyMonospaceFont();

            StackTraceLink = new GUIStyle(StackTraceLine)
            {
                normal = { textColor = colors.TextLink },
                hover = { textColor = colors.TextLinkHover }
            };
        }

        private GUIStyle CreateMessageStyle()
        {
            GUIStyle style = CreateLabelStyle(EditorStyles.label, TextAnchor.MiddleLeft, colors.TextDefault);
            style.richText = preferences.RichText.Value;
            style.wordWrap = false;
            style.clipping = TextClipping.Clip;
            return style;
        }

        private static GUIStyle CreateLabelStyle(GUIStyle baseStyle, TextAnchor alignment, Color textColor)
        {
            GUIStyle style = new(baseStyle) { alignment = alignment };
            style.normal.textColor = textColor;
            return style;
        }

        private GUIStyle CreateStackTraceLineStyle()
        {
            return new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(4, 4, 2, 2),
                fontSize = STACK_TRACE_FONT_SIZE,
                wordWrap = false,
                richText = preferences.RichText.Value
            };
        }

        private void ApplyMonospaceFont()
        {
            Font font = GetMonospaceFont();
            Message.font = font;
            StackTraceLine.font = font;
        }

        private void UpdateRowHeight()
        {
            float lineHeight = Message.lineHeight > 0f ? Message.lineHeight : EditorGUIUtility.singleLineHeight;
            float textHeight = Mathf.Ceil(preferences.LogEntryLines * lineHeight) + ROW_VERTICAL_PADDING;
            RowHeight = Mathf.Max(MIN_ROW_HEIGHT, textHeight);
        }

        // The font Unity's own Console uses for its monospace option, with OS fonts as a fallback.
        private static Font GetMonospaceFont()
        {
            if (monospaceFont != null)
                return monospaceFont;

            monospaceFont = EditorGUIUtility.Load(MONOSPACE_FONT_PATH) as Font;
            if (monospaceFont == null)
                monospaceFont = Font.CreateDynamicFontFromOSFont(MONOSPACE_FALLBACK_FONTS,
                    MONOSPACE_FALLBACK_FONT_SIZE);

            return monospaceFont;
        }
    }
}
#endif
