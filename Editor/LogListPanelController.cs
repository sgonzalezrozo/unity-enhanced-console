#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// LogListPanelController: Renders the log list panel.
	/// Responsibility: Draw log list, individual entries, and handle scrolling.
	/// </summary>
	public class LogListPanelController
	{
		private const int LOG_ENTRY_HEIGHT = 32;
		private const int TOOLBAR_HEIGHT = 20;

		private readonly object _colorScheme;
		private readonly object _formatter;
		private readonly object _sectionOrganizer;
		private readonly object _filterManager;

		private Vector2 _scrollPosition;

		public Vector2 ScrollPosition => _scrollPosition;

		public LogListPanelController(object colorScheme, object formatter, object sectionOrganizer, object filterManager)
		{
			_colorScheme = colorScheme;
			_formatter = formatter;
			_sectionOrganizer = sectionOrganizer;
			_filterManager = filterManager;
		}

		public void DrawLogList(object filteredLogs, Rect windowRect, bool showStackTrace, object selectionManager, Action<object> onDoubleClick)
		{
			float minLogListHeight = 120f;
			float availableHeight = showStackTrace
				? windowRect.height - TOOLBAR_HEIGHT - 200f - 10
				: windowRect.height - TOOLBAR_HEIGHT - 10;

			var logList = filteredLogs as List<LogEntry>;
			float totalContentHeight = logList != null ? Mathf.Max(logList.Count * LOG_ENTRY_HEIGHT, minLogListHeight) : minLogListHeight;
			float constrainedHeight = Mathf.Min(availableHeight, totalContentHeight);

			Rect scrollViewRect = GUILayoutUtility.GetRect(0, constrainedHeight, GUILayout.ExpandWidth(true));

			try
			{
				_scrollPosition = GUI.BeginScrollView(scrollViewRect, _scrollPosition,
					new Rect(0, 0, scrollViewRect.width - 20, logList != null ? logList.Count * LOG_ENTRY_HEIGHT : 0));

				if (logList != null)
				{
					for (int i = 0; i < logList.Count; i++)
						DrawLogListEntry(logList, i, scrollViewRect, selectionManager);
				}
			}
			finally
			{
				GUI.EndScrollView();
			}

			HandleLogListInput(logList, scrollViewRect, selectionManager, onDoubleClick);
		}

		private void DrawLogListEntry(List<LogEntry> filteredLogs, int i, Rect scrollViewRect, object selectionManager)
		{
			LogEntry log = filteredLogs[i];
			Rect entryRect = new(0, i * LOG_ENTRY_HEIGHT, scrollViewRect.width - 20, LOG_ENTRY_HEIGHT);
			EditorGUI.DrawRect(entryRect, Color.gray);
		}

		private void HandleLogListInput(List<LogEntry> filteredLogs, Rect scrollViewRect, object selectionManager, Action<object> onDoubleClick)
		{
			Event e = Event.current;

			if (e.type != EventType.MouseDown || filteredLogs == null)
				return;

			Rect scrollArea = GUILayoutUtility.GetLastRect();
			if (!scrollArea.Contains(e.mousePosition))
				return;

			int clickedIndex = (int)((e.mousePosition.y - scrollArea.y + _scrollPosition.y) / LOG_ENTRY_HEIGHT);
			if (clickedIndex < 0 || clickedIndex >= filteredLogs.Count)
				return;

			if (e.clickCount == 2)
			{
				onDoubleClick?.Invoke(filteredLogs[clickedIndex]);
				e.Use();
			}
		}
	}
}
#endif



