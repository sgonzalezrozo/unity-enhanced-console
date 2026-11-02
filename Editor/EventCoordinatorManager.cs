#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// EventCoordinatorManager: Manages event handler registration and unregistration.
	/// Responsibility: Coordinate all EditorApplication and Compilation event subscriptions.
	/// </summary>
	public class EventCoordinatorManager
	{
		private static bool _buildPlayerHandlerRegistered;

		public void RegisterEventHandlers(
			Action<string, string, LogType> onLogReceived,
			Action<PlayModeStateChange> onPlayModeChanged,
			Action onBeforeAssemblyReload,
			Action<object> onCompilationStarted,
			Action<object> onCompilationFinished,
			Action<BuildPlayerOptions> onBuildPlayer)
		{
			Application.logMessageReceived += (msg, stack, type) => onLogReceived(msg, stack, type);
			EditorApplication.playModeStateChanged += onPlayModeChanged;

			// AssemblyReloadCallback doesn't take parameters, so we wrap it
			if (onBeforeAssemblyReload != null)
				AssemblyReloadEvents.beforeAssemblyReload += () => onBeforeAssemblyReload();

			CompilationPipeline.compilationStarted += onCompilationStarted;
			CompilationPipeline.compilationFinished += onCompilationFinished;

			if (!_buildPlayerHandlerRegistered)
			{
				BuildPlayerWindow.RegisterBuildPlayerHandler(onBuildPlayer);
				_buildPlayerHandlerRegistered = true;
			}
		}

		public void UnregisterEventHandlers(
			Action<string, string, LogType> onLogReceived,
			Action<PlayModeStateChange> onPlayModeChanged,
			Action onBeforeAssemblyReload,
			Action<object> onCompilationStarted,
			Action<object> onCompilationFinished)
		{
			Application.logMessageReceived -= (msg, stack, type) => onLogReceived(msg, stack, type);
			EditorApplication.playModeStateChanged -= onPlayModeChanged;

			if (onBeforeAssemblyReload != null)
				AssemblyReloadEvents.beforeAssemblyReload -= () => onBeforeAssemblyReload();

			CompilationPipeline.compilationStarted -= onCompilationStarted;
			CompilationPipeline.compilationFinished -= onCompilationFinished;
		}
	}
}
#endif



