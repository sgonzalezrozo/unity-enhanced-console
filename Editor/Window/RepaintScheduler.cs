#if UNITY_EDITOR
namespace EnhancedConsole.Editor.Window
{
    /// <summary>
    /// Coalesces repaint requests from any thread into at most one repaint per interval.
    /// </summary>
    internal sealed class RepaintScheduler
    {
        private const double REPAINT_INTERVAL_SECONDS = 1d / 30d;

        private volatile bool isRequested;
        private double lastRepaintTime;

        public void Request()
        {
            isRequested = true;
        }

        public bool TryConsume(double now)
        {
            if (!isRequested || now - lastRepaintTime < REPAINT_INTERVAL_SECONDS)
                return false;

            isRequested = false;
            lastRepaintTime = now;
            return true;
        }
    }
}
#endif
