#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Finds the file and line of stack trace frames, keeping only files that can be opened.
    /// </summary>
    internal class StackTraceParser
    {
        private const string LOCATION_START = " (at ";

        public StackTraceLineInfo ParseLine(string line)
        {
            if (!TryParseFrameLocation(line, out string path, out int lineNumber))
                return new StackTraceLineInfo();

            string openablePath = GetOpenablePath(path);
            if (openablePath == null)
                return new StackTraceLineInfo();

            return new StackTraceLineInfo { hasFileInfo = true, filePath = openablePath, lineNumber = lineNumber };
        }

        // The first frame that can be opened, skipping Unity's logging frames and the context handler.
        public bool TryParseFirstLine(string stackTrace, out string filePath, out int lineNumber)
        {
            filePath = null;
            lineNumber = 0;
            if (string.IsNullOrEmpty(stackTrace))
                return false;

            foreach (string line in stackTrace.Split('\n'))
            {
                StackTraceLineInfo info = IsLoggingFrame(line) ? new StackTraceLineInfo() : ParseLine(line);
                if (!info.hasFileInfo)
                    continue;

                filePath = info.filePath;
                lineNumber = info.lineNumber;
                return true;
            }

            return false;
        }

        // Frames look like "Type:Method () (at path:line)". The path can be absolute, for example in a local
        // package, and on Windows it contains a drive colon, so the line number is after the last colon.
        public static bool TryParseFrameLocation(string line, out string path, out int lineNumber)
        {
            path = null;
            lineNumber = 0;
            int start = string.IsNullOrEmpty(line) ? -1 : line.IndexOf(LOCATION_START, StringComparison.Ordinal);
            int end = start < 0 ? -1 : line.LastIndexOf(')');
            if (end <= start)
                return false;

            string location = line.Substring(start + LOCATION_START.Length, end - start - LOCATION_START.Length);
            int colon = location.LastIndexOf(':');
            if (colon <= 0 || !int.TryParse(location.Substring(colon + 1), out lineNumber))
                return false;

            path = location.Substring(0, colon);
            return true;
        }

        // A project path (Assets/ or Packages/) when the file is in the project, the absolute path when it's a
        // file outside it, or null when it can't be opened, such as Unity's own source paths.
        public static string GetOpenablePath(string path)
        {
            if (IsProjectAsset(path))
                return path;

            string logicalPath = FileUtil.GetLogicalPath(path);
            if (IsProjectAsset(logicalPath))
                return logicalPath;

            return Path.IsPathRooted(path) && File.Exists(path) ? path : null;
        }

        private static bool IsProjectAsset(string path)
        {
            bool isProjectPath = !string.IsNullOrEmpty(path) &&
                                 (path.StartsWith("Assets/") || path.StartsWith("Packages/"));
            return isProjectPath && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null;
        }

        private static bool IsLoggingFrame(string line)
        {
            return line.Contains("LogContextHandler") || line.Contains("LogContextHelper") ||
                   line.Contains("UnityEngine.Debug:") || line.Contains("UnityEngine.Logger:");
        }
    }
}
#endif
