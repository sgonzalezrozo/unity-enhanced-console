#if UNITY_EDITOR
namespace EnhancedConsole.Editor
{
    /// <summary>What the time column shows for each log.</summary>
    internal enum TimeDisplayMode
    {
        // Time.realtimeSinceStartup when the log arrived, as mm:ss.mmm.
        SinceStartup,

        // The local wall-clock time when the log arrived, as HH:mm:ss.fff.
        WallClock,

        // Time.frameCount when the log arrived.
        Frame
    }
}
#endif
