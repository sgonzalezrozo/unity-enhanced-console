#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EnhancedConsole.Editor
{
    public sealed class LogContextHandler : ILogHandler
    {
        private readonly ILogHandler defaultLogHandler;

        public LogContextHandler()
        {
            defaultLogHandler = Debug.unityLogger.logHandler;
        }

        [HideInCallstack]
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            CaptureContext(context);
            defaultLogHandler.LogFormat(logType, context, format, args);
        }

        [HideInCallstack]
        public void LogException(Exception exception, Object context)
        {
            CaptureContext(context);
            defaultLogHandler.LogException(exception, context);
        }

        // Runs on whatever thread logged. GetInstanceID, Component.gameObject and Unity's overloaded null
        // check all require the main thread, and calling them from a worker thread would make that
        // thread's Debug.Log throw. So the context is only captured for main-thread logs, and the ID is
        // resolved to a GameObject later.
        private static void CaptureContext(Object context)
        {
            if (ReferenceEquals(context, null) || !LogContextHelper.IsMainThread)
                return;

            LogContextHelper.SetLastLogContextInstanceId(context.GetInstanceID());
        }
    }

    public static class LogContextHelper
    {
        private static bool isInitialized;
        private static int mainThreadId = -1;

        // Per thread, so a context logged on a worker thread can't attach to the next main-thread log.
        [ThreadStatic] private static int lastLoggedContextInstanceId;

        public static void Initialize()
        {
            if (isInitialized)
                return;

            // Initialize is called from the Editor's main thread.
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Debug.unityLogger.logHandler = new LogContextHandler();
            isInitialized = true;
        }

        internal static bool IsMainThread => System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId;

        internal static void SetLastLogContextInstanceId(int instanceId)
        {
            lastLoggedContextInstanceId = instanceId;
        }

        /// <summary>Instance ID of the last log's context object on this thread. Can be a GameObject or a Component.</summary>
        public static int GetLastLogContextInstanceId()
        {
            return lastLoggedContextInstanceId;
        }

        public static void ClearLogContext()
        {
            lastLoggedContextInstanceId = 0;
        }

        /// <summary>Returns and clears the context captured for the log being received on this thread.</summary>
        internal static int ConsumeLogContextInstanceId()
        {
            int id = lastLoggedContextInstanceId;
            lastLoggedContextInstanceId = 0;
            return id;
        }

        /// <summary>
        /// Resolves a captured context ID in O(1). A Component resolves to its GameObject; other objects,
        /// such as assets, resolve to themselves. Returns null when the object no longer exists. Main thread only.
        /// </summary>
        internal static Object ResolveContextObject(int instanceId)
        {
            if (instanceId == 0)
                return null;

            // Obsolete in Unity 6.3 in favor of EntityIdToObject, but still works there,
            // and it's the only option on the package's minimum version (2022.3).
#pragma warning disable CS0618
            Object obj = EditorUtility.InstanceIDToObject(instanceId);
#pragma warning restore CS0618
            if (obj == null)
                return null;

            return obj is Component component ? component.gameObject : obj;
        }
    }
}
#endif
