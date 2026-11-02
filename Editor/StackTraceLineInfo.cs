#if UNITY_EDITOR
namespace EnhancedConsole.Editor
{
    internal struct StackTraceLineInfo
    {
        public bool HasFileInfo;
        public string FilePath;
        public int LineNumber;
    }
}
#endif

