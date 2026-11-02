#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// PlayModeStateManager: Manages play mode state and refresh behavior.
	/// Responsibility: Track play mode state, control refresh updates.
	/// </summary>
	public class PlayModeStateManager
	{
		private bool _isInPlayMode;
		private Action _onRefresh;

		public bool IsInPlayMode => _isInPlayMode;

		public PlayModeStateManager(Action onRefresh)
		{
			_onRefresh = onRefresh;
		}

		public void HandlePlayModeChanged(PlayModeStateChange state, Action onClearRequested)
		{
			switch (state)
			{
				case PlayModeStateChange.EnteredPlayMode:
					_isInPlayMode = true;
					EditorApplication.update += OnEditorUpdate;
					break;

				case PlayModeStateChange.ExitingPlayMode:
					_isInPlayMode = false;
					EditorApplication.update -= OnEditorUpdate;
					break;

				case PlayModeStateChange.ExitingEditMode:
					onClearRequested?.Invoke();
					break;
			}
		}

		private void OnEditorUpdate()
		{
			if (_isInPlayMode)
				_onRefresh?.Invoke();
		}

		public void Cleanup()
		{
			if (_isInPlayMode)
				EditorApplication.update -= OnEditorUpdate;
			_isInPlayMode = false;
		}
	}
}
#endif



