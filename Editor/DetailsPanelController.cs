#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// DetailsPanelController: Renders the details and stacktrace panel.
	/// Responsibility: Draw details panel, stacktrace with clickable links, resize handle.
	/// </summary>
	public class DetailsPanelController
	{
		private const int TOOLBAR_HEIGHT = 20;

		private readonly object _colorScheme;
		private readonly object _stackTraceParser;

		private Vector2 _detailsScrollPosition;
		private bool _isResizingDetails;
		private float _detailsHeight = 200f;

		public float DetailsHeight => _detailsHeight;
		public bool IsResizingDetails => _isResizingDetails;

		public DetailsPanelController(object colorScheme, object stackTraceParser)
		{
			_colorScheme = colorScheme;
			_stackTraceParser = stackTraceParser;
		}

		public void DrawDetailsPanel(object filteredLogs, object selectionManager, Rect windowRect, Action<string, int> onOpenScript)
		{
			var logList = filteredLogs as List<LogEntry>;
			if (logList == null || logList.Count == 0)
				return;

			EditorGUILayout.LabelField("Details Panel (Stub)", EditorStyles.boldLabel);
		}

		public void DrawResizeHandle(Rect windowRect)
		{
			Rect resizeRect = new(0, windowRect.height - _detailsHeight - 5, windowRect.width, 5);
			EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeVertical);
		}

		public void SetDetailsHeight(float height)
		{
			_detailsHeight = height;
		}
	}
}
#endif



