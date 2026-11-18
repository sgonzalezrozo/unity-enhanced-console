#if UNITY_EDITOR
using UnityEngine;

namespace EnhancedConsole.Editor
{
	/// <summary>
	/// Manages color styling for log levels and UI elements.
	/// Encapsulates all color definitions to simplify theming and styling logic.
	/// Single Responsibility: Color management only.
	/// </summary>
	internal class LogLevelColorScheme
	{
		private bool isDarkTheme;

		public LogLevelColorScheme()
		{
			isDarkTheme = UnityEditor.EditorGUIUtility.isProSkin;
		}

		public Color EvenRowBackground
		{
			get
			{
				return isDarkTheme
					? new Color(0.22f, 0.22f, 0.22f, 1f)
					: new Color(0.78f, 0.78f, 0.78f, 1f);
			}
		}

		public Color OddRowBackground
		{
			get
			{
				return isDarkTheme
					? new Color(0.25f, 0.25f, 0.25f, 1f)
					: new Color(0.72f, 0.72f, 0.72f, 1f);
			}
		}

		public Color SelectedRowBackground => new(0.24f, 0.48f, 0.90f, 0.5f);

		public Color ResizeHandleBackground => new(0.3f, 0.3f, 0.3f, 0.5f);

		public Color MessagePanelBackground
		{
			get
			{
				return isDarkTheme
					? new Color(0.28f, 0.28f, 0.28f, 1f)
					: new Color(0.85f, 0.85f, 0.85f, 1f);
			}
		}

		public Color StackTracePanelBackground
		{
			get
			{
				return isDarkTheme
					? new Color(0.27f, 0.27f, 0.27f, 1f)
					: new Color(0.84f, 0.84f, 0.84f, 1f);
			}
		}

		public Color SeparatorLine
		{
			get
			{
				return isDarkTheme
					? new Color(0.3f, 0.3f, 0.3f, 1f)
					: new Color(0.5f, 0.5f, 0.5f, 1f);
			}
		}

		public Color SeparatorLineDim
		{
			get
			{
				return isDarkTheme
					? new Color(0.15f, 0.15f, 0.15f, 1f)
					: new Color(0.5f, 0.5f, 0.5f, 1f);
			}
		}

		public Color GripHandle
		{
			get
			{
				return isDarkTheme
					? new Color(0.7f, 0.7f, 0.7f, 0.8f)
					: new Color(0.6f, 0.6f, 0.6f, 0.8f);
			}
		}

		public Color TextDefault
		{
			get
			{
				return isDarkTheme ? Color.white : Color.black;
			}
		}

		public Color TextMuted => new(0.7f, 0.7f, 0.7f, 1f);

		public Color TextLink => new(0.4f, 0.6f, 1f);

		public Color TextLinkHover => new(0.6f, 0.8f, 1f);

		public Color CountCircleBackground
		{
			get
			{
				return isDarkTheme
					? new Color(0.25f, 0.25f, 0.25f, 1f)
					: new Color(0.2f, 0.2f, 0.2f, 1f);
			}
		}

		public Color CountCircleBackgroundDark => new(0.2f, 0.2f, 0.2f, 1f);

		public Color CountCircleText => new(0.95f, 0.95f, 0.95f, 1f);

		public Color SelectedCountCircle => new(0.1f, 0.3f, 0.6f, 1f);

		public Color ToggleButtonActive => new(0.6f, 0.8f, 1f);

		public Color DisabledButton => new(0.7f, 0.7f, 0.7f);

		public Color CompilationStatusCompiling => new(1f, 0.8f, 0.3f, 1f);

		public Color CompilationStatusError => new(1f, 0.3f, 0.3f, 1f);

		public Color ClearButtonBlocked => new(1f, 0.5f, 0.5f, 0.7f);

		public Color DisabledUIElement => new(0.5f, 0.5f, 0.5f, 0.5f);

		public Color SearchHighlight
		{
			get
			{
				return isDarkTheme
					? new Color(1f, 0.82f, 0.2f, 0.35f)
					: new Color(1f, 0.75f, 0f, 0.45f);
			}
		}
	}
}
#endif

