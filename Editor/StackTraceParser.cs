#if UNITY_EDITOR
using UnityEditor;

namespace EnhancedConsole.Editor
{
	/// <summary>
	/// Parses stack trace strings and extracts file/line information.
	/// Single Responsibility: Stack trace parsing only.
	/// </summary>
	internal class StackTraceParser
	{
		public StackTraceLineInfo ParseLine(string line)
		{
			StackTraceLineInfo info = new();

			if (string.IsNullOrEmpty(line))
				return info;

			int atIndex = line.IndexOf(" (at ", System.StringComparison.Ordinal);
			if (atIndex < 0)
				return info;

			int startPath = atIndex + 5;
			int endPath = line.IndexOf(')', startPath);
			if (endPath < 0)
				return info;

			string pathAndLine = line.Substring(startPath, endPath - startPath);
			if (!TryExtractFileInfo(pathAndLine, out string assetPath, out int lineNumber))
				return info;

			if (!IsValidAssetPath(assetPath))
				return info;

			UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
			if (asset == null)
				return info;

			info.HasFileInfo = true;
			info.FilePath = assetPath;
			info.LineNumber = lineNumber;

			return info;
		}

	public bool TryParseFirstLine(string stackTrace, out string filePath, out int lineNumber)
	{
		filePath = null;
		lineNumber = 0;

		if (string.IsNullOrEmpty(stackTrace))
			return false;

		string[] lines = stackTrace.Split('\n');
		foreach (string line in lines)
		{
			if (string.IsNullOrEmpty(line))
				continue;

			// Skip LogContextHandler frames (our custom log interceptor)
			if (line.Contains("LogContextHandler") || line.Contains("LogContextHelper"))
				continue;

			// Skip Unity's internal Debug.Log frames
			if (line.Contains("UnityEngine.Debug:") || line.Contains("UnityEngine.Logger:"))
				continue;

			StackTraceLineInfo info = ParseLine(line);
			if (info.HasFileInfo)
			{
				filePath = info.FilePath;
				lineNumber = info.LineNumber;
				return true;
			}
		}

		return false;
	}

		private bool TryExtractFileInfo(string pathAndLine, out string assetPath, out int lineNumber)
		{
			assetPath = null;
			lineNumber = 0;

			string[] parts = pathAndLine.Split(':');
			if (parts.Length < 2)
				return false;

			assetPath = parts[0];
			return int.TryParse(parts[1], out lineNumber);
		}

		private bool IsValidAssetPath(string path)
		{
			return !string.IsNullOrEmpty(path) &&
				   (path.StartsWith("Assets/") || path.StartsWith("Packages/"));
		}
	}
}
#endif

