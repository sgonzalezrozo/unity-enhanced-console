#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using EnhancedConsole.Editor.LogManagement;
using EnhancedConsole.Editor.Window;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace EnhancedConsole.Editor.LogCapture
{
    /// <summary>
    /// Reads compiler errors and warnings straight from the compilation pipeline, so they're listed whether or
    /// not Unity also sends them to Application.logMessageReceived. Each assembly's results replace its
    /// previous ones.
    /// </summary>
    internal sealed class CompilerMessageListener
    {
        private readonly ConsoleContext context;

        public CompilerMessageListener(ConsoleContext consoleContext)
        {
            context = consoleContext;
        }

        public void Register()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        public void Unregister()
        {
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
        }

        private void OnCompilationStarted(object compilationContext)
        {
            if (context.Preferences.ClearOnRecompile.Value)
                context.Logs.ClearKeepingCompilerErrors();

            context.Logs.BeginCompilation();
        }

        private void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            context.Logs.SetCompilerMessages(assemblyPath, ToLogMessages(messages));
            context.Repaints.Request();
        }

        private static List<CapturedLog> ToLogMessages(CompilerMessage[] messages)
        {
            messages ??= Array.Empty<CompilerMessage>();
            List<CapturedLog> results = new(messages.Length);
            foreach (CompilerMessage compilerMessage in messages)
            {
                if (compilerMessage.type == CompilerMessageType.Error)
                    results.Add(CreateLog(compilerMessage.message, LogType.Error));
                else if (compilerMessage.type == CompilerMessageType.Warning)
                    results.Add(CreateLog(compilerMessage.message, LogType.Warning));
            }

            return results;
        }

        private static CapturedLog CreateLog(string message, LogType type)
        {
            return new CapturedLog
            {
                message = message,
                stackTrace = "",
                type = type,
                realtimeSinceStartup = Time.realtimeSinceStartup,
                wallClockTicks = DateTime.Now.Ticks,
                frameCount = Time.frameCount
            };
        }

        private void OnCompilationFinished(object compilationContext)
        {
            EditorApplication.delayCall += RepaintOnceMore;
            context.Repaint();
        }

        private void RepaintOnceMore()
        {
            EditorApplication.delayCall -= RepaintOnceMore;
            context.Repaint();
        }
    }
}
#endif
