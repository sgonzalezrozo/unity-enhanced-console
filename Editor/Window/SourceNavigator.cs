#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnhancedConsole.Editor.Window
{
    /// <summary>
    /// Opens a log's source file and pings its context object.
    /// </summary>
    internal sealed class SourceNavigator
    {
        private readonly StackTraceParser stackTraceParser = new();

        public StackTraceLineInfo ParseLine(string line)
        {
            return stackTraceParser.ParseLine(line);
        }

        public void Open(LogEntry log)
        {
            if (!string.IsNullOrEmpty(log.stackTrace) &&
                stackTraceParser.TryParseFirstLine(log.stackTrace, out string filePath, out int lineNumber))
            {
                OpenFile(filePath, lineNumber);
                return;
            }

            if (!TryParseCompilerLocation(log.message, out string path, out int compilerLine))
                return;

            string openablePath = StackTraceParser.GetOpenablePath(path);
            if (openablePath != null)
                OpenFile(openablePath, compilerLine);
        }

        public void OpenFile(string filePath, int lineNumber)
        {
            InternalEditorUtility.OpenFileAtLineExternal(filePath, lineNumber);
        }

        // Finds the context object directly from its ID instead of searching every loaded scene.
        public void Ping(LogEntry log)
        {
            Object target = LogContextHelper.ResolveContextObject(log.contextGameObjectInstanceId);
            if (target == null)
                return;

            EditorGUIUtility.PingObject(target);
            UnityEditor.Selection.activeObject = target;
        }

        // Compiler messages look like "Assets/Folder/File.cs(12,5): error CS0001: ...".
        private static bool TryParseCompilerLocation(string message, out string path, out int lineNumber)
        {
            path = null;
            lineNumber = 0;
            if (string.IsNullOrEmpty(message))
                return false;

            int openParenIndex = message.IndexOf('(');
            int closeParenIndex = message.IndexOf(')');
            if (openParenIndex < 0 || closeParenIndex <= openParenIndex)
                return false;

            path = message.Substring(0, openParenIndex).Trim();
            string coordinates = message.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1);
            return int.TryParse(coordinates.Split(',')[0].Trim(), out lineNumber);
        }
    }
}
#endif
