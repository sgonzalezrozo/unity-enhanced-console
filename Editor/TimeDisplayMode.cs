#if UNITY_EDITOR
namespace EnhancedConsole.Editor
{
    /// <summary>What the time column shows for each log.</summary>
    internal enum TimeDisplayMode
    {
        /// <summary><c>Time.realtimeSinceStartup</c> when the log arrived, as mm:ss.mmm.</summary>
        SinceStartup,

        /// <summary>The local wall-clock time when the log arrived, as HH:mm:ss.fff.</summary>
        WallClock,

        /// <summary><c>Time.frameCount</c> when the log arrived.</summary>
        Frame
    }
}
#endif
