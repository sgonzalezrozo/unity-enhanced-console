#if UNITY_EDITOR
using System;
using System.Globalization;
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

		public static string FormatTime(LogEntry log, TimeDisplayMode mode)
		{
			switch (mode)
			{
				case TimeDisplayMode.WallClock:
					// Logs restored from before this field existed have no wall-clock time.
					return log.WallClockTicks > 0
						? new DateTime(log.WallClockTicks).ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)
						: "--:--:--.---";
				case TimeDisplayMode.Frame:
					return "#" + log.FrameCount.ToString(CultureInfo.InvariantCulture);
				default:
					return FormatTimeDisplay(log.RealtimeSinceStartup);
			}
		}

		/// <summary>The widest text a mode produces, for sizing the time column.</summary>
		public static string GetTimeSample(TimeDisplayMode mode)
		{
			return mode switch
			{
				TimeDisplayMode.WallClock => "00:00:00.000",
				TimeDisplayMode.Frame => "#0000000",
				_ => "00:00.000"
			};
		}

		public static string FormatTimeDisplay(float realtimeSinceStartup)
		{
			int minutes = (int)(realtimeSinceStartup / 60f);
			int seconds = (int)(realtimeSinceStartup % 60f);
			int milliseconds = (int)((realtimeSinceStartup * 1000f) % 1000f);
			return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
		}

		/// <summary>
		/// The first <paramref name="maxLines"/> lines of a message, each cut at 200 characters.
		/// </summary>
		public static string TruncateMessage(string message, int maxLines = 1)
		{
			if (string.IsNullOrEmpty(message))
				return "";

			maxLines = Math.Max(1, maxLines);
			StringBuilder builder = null;
			int start = 0;

			for (int line = 0; line < maxLines && start <= message.Length; line++)
			{
				int end = message.IndexOfAny(NewlineChars, start);
				bool isLast = end < 0;
				if (isLast)
					end = message.Length;

				string text = message.Substring(start, end - start);
				if (text.Length > MAX_DISPLAY_LENGTH)
					text = text.Substring(0, MAX_DISPLAY_LENGTH) + "...";

				if (line == 0 && (isLast || maxLines == 1))
					return text;

				builder ??= new StringBuilder();
				if (line > 0)
					builder.Append('\n');
				builder.Append(text);

				if (isLast)
					break;

				// Treat \r\n as one line break.
				start = end + 1;
				if (message[end] == '\r' && start < message.Length && message[start] == '\n')
					start++;
			}

			return builder?.ToString() ?? "";
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
