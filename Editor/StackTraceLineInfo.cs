#if UNITY_EDITOR
namespace EnhancedConsole.Editor
{
    internal struct StackTraceLineInfo
    {
        public bool hasFileInfo;
        public string filePath;
        public int lineNumber;
    }
}
#endif

