#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
	/// <summary>
	/// Formats log entries for display and clipboard operations.
	/// Single Responsibility: Log formatting only.
	/// </summary>
	internal class LogEntryFormatter
	{
		private const int MAX_DISPLAY_LENGTH = 200;
		private static readonly char[] NewlineChars = { '\r', '\n' };

		private static Texture2D errorIcon;
		private static Texture2D warningIcon;
		private static Texture2D infoIcon;

		private bool showStackTrace;
		private bool showCollapsedCount;

		public LogEntryFormatter(bool showStackTrace = true, bool showCollapsedCount = true)
		{
			this.showStackTrace = showStackTrace;
			this.showCollapsedCount = showCollapsedCount;
		}

		public void SetShowStackTrace(bool value)
		{
			showStackTrace = value;
		}

		public void SetShowCollapsedCount(bool value)
		{
			showCollapsedCount = value;
		}

		public string FormatForClipboard(LogEntry log)
		{
			StringBuilder builder = new();

			if (showCollapsedCount && log.Count > 1)
				builder.Append('(').Append(log.Count).Append("x) ");

			builder.Append('[').Append(log.Type).Append("] ").Append(log.Message);

			if (showStackTrace && !string.IsNullOrEmpty(log.StackTrace))
			{
				builder.AppendLine();
				builder.Append(log.StackTrace);
			}

			return builder.ToString();
		}

		public static string FormatTimeDisplay(float realtimeSinceStartup)
		{
			int minutes = (int)(realtimeSinceStartup / 60f);
			int seconds = (int)(realtimeSinceStartup % 60f);
			int milliseconds = (int)((realtimeSinceStartup * 1000f) % 1000f);
			return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
		}

		public static string TruncateMessage(string message)
		{
			if (string.IsNullOrEmpty(message))
				return "";

			string displayText = message;
			int newlineIndex = displayText.IndexOfAny(NewlineChars);
			if (newlineIndex >= 0)
				displayText = displayText.Substring(0, newlineIndex);

			if (displayText.Length > MAX_DISPLAY_LENGTH)
				displayText = displayText.Substring(0, MAX_DISPLAY_LENGTH) + "...";

			return displayText;
		}

		public static Texture2D GetIconForLogType(LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Exception:
				case LogType.Assert:
					return GetIcon(ref errorIcon, "console.erroricon");
				case LogType.Warning:
					return GetIcon(ref warningIcon, "console.warnicon");
				default:
					return GetIcon(ref infoIcon, "console.infoicon");
			}
		}

		// Uses Unity's null check, so an icon unloaded by a skin change is loaded again.
		private static Texture2D GetIcon(ref Texture2D cached, string name)
		{
			if (cached == null)
				cached = EditorGUIUtility.IconContent(name).image as Texture2D;

			return cached;
		}
	}
}
#endif
