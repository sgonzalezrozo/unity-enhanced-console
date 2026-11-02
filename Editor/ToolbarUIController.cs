#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// ToolbarUIController: Renders and manages the toolbar UI.
	/// Responsibility: Draw all toolbar buttons, dropdowns, toggles, and search field.
	/// </summary>
	public class ToolbarUIController
	{
		private readonly object _colorScheme;
		private readonly object _filterManager;
		private readonly object _sectionOrganizer;

		private bool _showStackTrace = true;
		private bool _showTime;

		public bool ShowStackTrace => _showStackTrace;
		public bool ShowTime => _showTime;

		public ToolbarUIController(object colorScheme, object filterManager, object sectionOrganizer)
		{
			_colorScheme = colorScheme;
			_filterManager = filterManager;
			_sectionOrganizer = sectionOrganizer;
		}

		public void DrawToolbar(Action onClear, Action onRecompile, Action onRecompileWithRefresh, Action<object> onCopy, object selectiveLogsConfig)
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button(new GUIContent("Clear", "Clear all logs"), EditorStyles.toolbarButton, GUILayout.Width(70)))
				onClear?.Invoke();

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		public void DrawLogTypeToggles(object logManager)
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUILayout.Label("Log Type Toggles", EditorStyles.miniLabel);
			EditorGUILayout.EndHorizontal();
		}
	}
}
#endif



