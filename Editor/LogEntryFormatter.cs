#if UNITY_EDITOR
using System.Text;
using UnityEngine;

namespace EnhancedConsole.Editor
{
	/// <summary>
	/// Formats log entries for display and clipboard operations.
	/// Single Responsibility: Log formatting only.
	/// </summary>
	internal class LogEntryFormatter
	{
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

			string prefix = showCollapsedCount && log.Count > 1 ? $"({log.Count}x) " : "";
			builder.Append($"{prefix}[{log.Type}] {log.Message}");

			if (showStackTrace && !string.IsNullOrEmpty(log.StackTrace))
			{
				builder.AppendLine();
				builder.Append(log.StackTrace);
			}

			return builder.ToString();
		}

		public string FormatSingleEntry(LogEntry log)
		{
			StringBuilder builder = new();

			builder.AppendLine($"[{log.Type}] {log.Message}");

			if (showStackTrace && !string.IsNullOrEmpty(log.StackTrace))
			{
				builder.AppendLine();
				builder.AppendLine("Stack Trace:");
				builder.AppendLine(log.StackTrace);
			}

			return builder.ToString();
		}

		public string FormatTimeDisplay(float realtimeSinceStartup)
		{
			int minutes = (int)(realtimeSinceStartup / 60f);
			int seconds = (int)(realtimeSinceStartup % 60f);
			int milliseconds = (int)((realtimeSinceStartup * 1000f) % 1000f);
			return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
		}

		public string TruncateMessage(string message, int maxLength = 200)
		{
			if (string.IsNullOrEmpty(message))
				return "";

			string displayText = message;
			int newlineIndex = displayText.IndexOfAny(new[] { '\r', '\n' });
			if (newlineIndex >= 0)
				displayText = displayText.Substring(0, newlineIndex);

			if (displayText.Length > maxLength)
				displayText = displayText.Substring(0, maxLength) + "...";

			return displayText;
		}

		public Texture2D GetIconForLogType(LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Exception:
				case LogType.Assert:
					return UnityEditor.EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
				case LogType.Warning:
					return UnityEditor.EditorGUIUtility.IconContent("console.warnicon").image as Texture2D;
				default:
					return UnityEditor.EditorGUIUtility.IconContent("console.infoicon").image as Texture2D;
			}
		}
	}
}
#endif

