#if UNITY_EDITOR
namespace Tools.EnhanceConsole.Editor
{
    internal struct StackTraceLineInfo
    {
        public bool HasFileInfo;
        public string FilePath;
        public int LineNumber;
    }
}
#endif

