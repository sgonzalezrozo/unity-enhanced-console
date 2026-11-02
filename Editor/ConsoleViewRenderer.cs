#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// Handles all UI rendering for the console window.
	/// Single Responsibility: Rendering only (no logic).
	/// Delegate rendering decisions to external components.
	/// </summary>
	internal class ConsoleViewRenderer
	{
		private const int TOOLBAR_HEIGHT = 20;
		private const int LOG_ENTRY_HEIGHT = 32;
		private const float MIN_DETAILS_HEIGHT = 100f;
		private const float MIN_LOG_LIST_HEIGHT = 120f;
		private const float COUNT_CIRCLE_SIZE = 20f;
		private const float ICON_SIZE = 16f;

		private LogLevelColorScheme colorScheme;
		private LogEntryFormatter formatter;
		private StackTraceParser stackTraceParser;

		private GUIStyle logStyle;
		private GUIStyle logStyleOdd;
		private GUIStyle logStyleSelected;
		private GUIStyle textStyle;
		private GUIStyle timeStyle;
		private GUIStyle countStyle;
		private GUIStyle messageStyle;
		private GUIStyle stackTraceLineStyle;
		private GUIStyle stackTraceLineStyleClickable;

		public ConsoleViewRenderer(LogLevelColorScheme colorScheme, LogEntryFormatter formatter)
		{
			this.colorScheme = colorScheme;
			this.formatter = formatter;
			stackTraceParser = new();
		}

		public void InitializeStyles()
		{
			InitializeLogStyles();
			InitializeTextStyles();
		}

		public void DrawToolbar(Action onDrawContent)
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			onDrawContent?.Invoke();
			EditorGUILayout.EndHorizontal();
		}

		public void DrawLogList(
			List<LogEntry> filteredLogs,
			Vector2 scrollPosition,
			ref Vector2 outScrollPosition,
			int selectedIndex,
			HashSet<int> multiSelectedIndices,
			bool isCollapsed,
			Action<int, int> onEntryClicked,
			Action<int> onEntryDoubleClicked)
		{
			float availableHeight = CalculateLogListHeight(filteredLogs);
			Rect scrollViewRect = GUILayoutUtility.GetRect(0, availableHeight, GUILayout.ExpandWidth(true));

			try
			{
				outScrollPosition = GUI.BeginScrollView(
					scrollViewRect,
					scrollPosition,
					new Rect(0, 0, scrollViewRect.width - 20, filteredLogs.Count * LOG_ENTRY_HEIGHT));

				for (int i = 0; i < filteredLogs.Count; i++)
				{
					DrawLogEntry(
						filteredLogs[i],
						i,
						scrollViewRect,
						selectedIndex,
						multiSelectedIndices,
						isCollapsed,
						onEntryClicked,
						onEntryDoubleClicked);
				}
			}
			finally
			{
				GUI.EndScrollView();
			}
		}

		public void DrawDetailsPanel(
			LogEntry selectedLog,
			bool showStackTrace,
			Vector2 scrollPosition,
			ref Vector2 outScrollPosition,
			float detailsHeight,
			float windowWidth,
			float windowHeight,
			Action onCopyClicked,
			Action<string, int> onStackTraceLineClicked)
		{
			if (selectedLog == null)
				return;

			Rect detailsRect = new(0, windowHeight - detailsHeight, windowWidth, detailsHeight);

			DrawDetailsSeparator(detailsRect);
			DrawMessagePanel(detailsRect, selectedLog, onCopyClicked);

			if (showStackTrace && !string.IsNullOrEmpty(selectedLog.StackTrace))
			{
				DrawStackTracePanel(
					detailsRect,
					selectedLog.StackTrace,
					scrollPosition,
					ref outScrollPosition,
					onStackTraceLineClicked);
			}
		}

		public void DrawResizeHandle(
			float detailsHeight,
			float windowHeight,
			float windowWidth,
			ref float outDetailsHeight,
			ref bool isResizing)
		{
			Rect resizeRect = new(0, windowHeight - detailsHeight - 5, windowWidth, 5);
			EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeVertical);

			float centerY = windowHeight - detailsHeight - 2.5f;
			float centerX = windowWidth * 0.5f;

			DrawResizeSeparator(centerY, windowWidth);
			DrawResizeGrips(centerX, centerY);

			HandleResizeInput(resizeRect, windowHeight, ref outDetailsHeight, ref isResizing);
		}

		private void InitializeLogStyles()
		{
			logStyle = new GUIStyle(EditorStyles.label)
			{
				padding = new RectOffset(10, 0, 2, 2),
				margin = new RectOffset(0, 0, 0, 0),
				richText = true,
				wordWrap = false,
				normal = { background = MakeColorTexture(2, 2, colorScheme.EvenRowBackground) }
			};

			logStyleOdd = new GUIStyle(logStyle)
			{
				normal = { background = MakeColorTexture(2, 2, colorScheme.OddRowBackground) }
			};

			logStyleSelected = new GUIStyle(logStyle)
			{
				normal = { background = MakeColorTexture(2, 2, colorScheme.SelectedRowBackground) }
			};
		}

		private void InitializeTextStyles()
		{
			textStyle = new(EditorStyles.label)
			{
				richText = true,
				wordWrap = false,
				normal = { textColor = colorScheme.TextDefault }
			};

			timeStyle = new(EditorStyles.miniLabel)
			{
				alignment = TextAnchor.MiddleRight,
				normal = { textColor = colorScheme.TextMuted }
			};

			countStyle = new(EditorStyles.miniLabel)
			{
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleCenter,
				normal = { textColor = colorScheme.CountCircleText }
			};

			messageStyle = new(EditorStyles.wordWrappedLabel)
			{
				padding = new RectOffset(6, 6, 4, 4)
			};

			stackTraceLineStyle = new(EditorStyles.label)
			{
				padding = new RectOffset(4, 4, 2, 2),
				fontSize = 11,
				wordWrap = false,
				richText = true
			};

			stackTraceLineStyleClickable = new(stackTraceLineStyle)
			{
				normal = { textColor = colorScheme.TextLink },
				hover = { textColor = colorScheme.TextLinkHover }
			};
		}

		private void DrawLogEntry(
			LogEntry log,
			int index,
			Rect scrollViewRect,
			int selectedIndex,
			HashSet<int> multiSelectedIndices,
			bool isCollapsed,
			Action<int, int> onEntryClicked,
			Action<int> onEntryDoubleClicked)
		{
			Rect entryRect = new(0, index * LOG_ENTRY_HEIGHT, scrollViewRect.width - 20, LOG_ENTRY_HEIGHT);

			Color backgroundColor = GetEntryBackgroundColor(index, selectedIndex, multiSelectedIndices);
			EditorGUI.DrawRect(entryRect, backgroundColor);

			DrawSectionBubble(entryRect, log);
			DrawLogIcon(entryRect, log.Type);

			float textStartX = entryRect.x + 15 + ICON_SIZE + 5;
			float textWidth = CalculateTextWidth(entryRect, isCollapsed);

			Rect textRect = new(textStartX, entryRect.y + 2, textWidth, LOG_ENTRY_HEIGHT);
			string displayText = formatter.TruncateMessage(log.Message);
			GUI.Label(textRect, displayText, textStyle);

			DrawTimeDisplay(entryRect, textRect, log);
			DrawCollapseCount(entryRect, log, isCollapsed, selectedIndex, index);

			HandleLogEntryInput(entryRect, index, onEntryClicked, onEntryDoubleClicked);
		}

		private void DrawMessagePanel(Rect detailsRect, LogEntry selectedLog, Action onCopyClicked)
		{
			Rect headerRect = new(detailsRect.x, detailsRect.y, detailsRect.width, 24);
			EditorGUI.DrawRect(headerRect, colorScheme.MessagePanelBackground);

			Rect copyButtonRect = new(headerRect.xMax - 54, headerRect.y + 2, 50, 20);
			if (GUI.Button(copyButtonRect, "Copy", EditorStyles.miniButton))
				onCopyClicked?.Invoke();

			float messageHeight = messageStyle.CalcHeight(new GUIContent(selectedLog.Message), detailsRect.width);
			messageHeight = Mathf.Max(messageHeight + 5, 20);

			Rect messageRect = new(detailsRect.x, headerRect.yMax, detailsRect.width, messageHeight);
			EditorGUI.DrawRect(messageRect, colorScheme.MessagePanelBackground);

			Rect messageContentRect = new(messageRect.x + 6, messageRect.y + 3, messageRect.width - 12, messageRect.height - 6);
			EditorGUI.SelectableLabel(messageContentRect, selectedLog.Message, messageStyle);
		}

		private void DrawStackTracePanel(
			Rect detailsRect,
			string stackTrace,
			Vector2 scrollPosition,
			ref Vector2 outScrollPosition,
			Action<string, int> onLineClicked)
		{
			float stackTraceStartY = detailsRect.y + 34 + 10;
			float stackTraceHeight = detailsRect.yMax - stackTraceStartY;

			Rect stackTraceRect = new(detailsRect.x, stackTraceStartY, detailsRect.width, stackTraceHeight);
			EditorGUI.DrawRect(stackTraceRect, colorScheme.StackTracePanelBackground);

			Rect scrollViewRect = new(stackTraceRect.x + 4, stackTraceRect.y + 4, stackTraceRect.width - 8, stackTraceRect.height - 8);

			string[] stackTraceLines = stackTrace.Split('\n');
			float lineHeight = 16f;
			float totalContentHeight = stackTraceLines.Length * lineHeight;

			try
			{
				outScrollPosition = GUI.BeginScrollView(
					scrollViewRect,
					scrollPosition,
					new Rect(0, 0, scrollViewRect.width - 20, totalContentHeight),
					false,
					totalContentHeight > scrollViewRect.height);

				for (int i = 0; i < stackTraceLines.Length; i++)
				{
					string line = stackTraceLines[i];
					if (string.IsNullOrEmpty(line))
						continue;

					Rect lineRect = new(0, i * lineHeight, scrollViewRect.width - 20, lineHeight);
					StackTraceLineInfo lineInfo = stackTraceParser.ParseLine(line);

					if (lineInfo.HasFileInfo)
					{
						if (GUI.Button(lineRect, "", GUIStyle.none))
							onLineClicked?.Invoke(lineInfo.FilePath, lineInfo.LineNumber);

						EditorGUIUtility.AddCursorRect(lineRect, MouseCursor.Link);
						GUI.Label(lineRect, line, stackTraceLineStyleClickable);
					}
					else
					{
						GUI.Label(lineRect, line, stackTraceLineStyle);
					}
				}
			}
			finally
			{
				GUI.EndScrollView();
			}
		}

		private void DrawDetailsSeparator(Rect detailsRect)
		{
			Rect separatorLine1 = new(0, detailsRect.y - 3, detailsRect.width, 1);
			Rect separatorLine2 = new(0, detailsRect.y - 1, detailsRect.width, 1);
			EditorGUI.DrawRect(separatorLine1, colorScheme.SeparatorLineDim);
			EditorGUI.DrawRect(separatorLine2, colorScheme.SeparatorLineDim);
		}

		private void DrawResizeSeparator(float centerY, float windowWidth)
		{
			Rect separatorRect = new(0, centerY - 0.5f, windowWidth, 1f);
			EditorGUI.DrawRect(separatorRect, colorScheme.SeparatorLine);
		}

		private void DrawResizeGrips(float centerX, float centerY)
		{
			float gripSpacing = 4f;
			float gripSize = 3f;

			for (int i = -1; i <= 1; i++)
			{
				Rect gripRect = new(centerX + (i * gripSpacing) - gripSize * 0.5f, centerY - gripSize * 0.5f, gripSize, gripSize);
				EditorGUI.DrawRect(gripRect, colorScheme.GripHandle);
			}
		}

		private void DrawSectionBubble(Rect entryRect, LogEntry log)
		{
			// To be called by external code that provides section info
		}

		private void DrawLogIcon(Rect entryRect, LogType type)
		{
			Rect iconRect = new(entryRect.x + 15, entryRect.y + 8, ICON_SIZE, ICON_SIZE);
			Texture2D icon = formatter.GetIconForLogType(type);
			GUI.DrawTexture(iconRect, icon);
		}

		private void DrawTimeDisplay(Rect entryRect, Rect textRect, LogEntry log)
		{
			// Time display handled by caller
		}

		private void DrawCollapseCount(Rect entryRect, LogEntry log, bool isCollapsed, int selectedIndex, int index)
		{
			if (!isCollapsed || log.Count <= 1)
				return;

			Rect circleRect = new(
				entryRect.xMax - 5 - COUNT_CIRCLE_SIZE,
				entryRect.y + (LOG_ENTRY_HEIGHT - COUNT_CIRCLE_SIZE) * 0.5f,
				COUNT_CIRCLE_SIZE,
				COUNT_CIRCLE_SIZE);

			Color circleColor = selectedIndex == index
				? colorScheme.SelectedCountCircle
				: colorScheme.CountCircleBackground;

			EditorGUI.DrawRect(circleRect, circleColor);
			GUI.Label(circleRect, log.Count.ToString(), countStyle);
		}

		private void HandleLogEntryInput(
			Rect entryRect,
			int index,
			Action<int, int> onEntryClicked,
			Action<int> onEntryDoubleClicked)
		{
			Event currentEvent = Event.current;
			if (currentEvent.type != EventType.MouseDown || !entryRect.Contains(currentEvent.mousePosition))
				return;

			if (currentEvent.clickCount == 2)
			{
				onEntryDoubleClicked?.Invoke(index);
				currentEvent.Use();
			}
			else if (currentEvent.clickCount == 1)
			{
				onEntryClicked?.Invoke(index, currentEvent.button);
				currentEvent.Use();
			}
		}

		private void HandleResizeInput(
			Rect resizeRect,
			float windowHeight,
			ref float detailsHeight,
			ref bool isResizing)
		{
			UnityEngine.Event currentEvent = UnityEngine.Event.current;

			if (currentEvent.type == EventType.MouseDown && resizeRect.Contains(currentEvent.mousePosition))
			{
				isResizing = true;
				currentEvent.Use();
			}

			if (isResizing)
			{
				if (currentEvent.type == EventType.MouseDrag)
				{
					float maxDetailsHeight = windowHeight - TOOLBAR_HEIGHT - MIN_LOG_LIST_HEIGHT - 10;
					detailsHeight = Mathf.Clamp(windowHeight - currentEvent.mousePosition.y, MIN_DETAILS_HEIGHT, maxDetailsHeight);
					currentEvent.Use();
				}
				else if (currentEvent.type == EventType.MouseUp)
				{
					isResizing = false;
					currentEvent.Use();
				}
			}
		}

		private Color GetEntryBackgroundColor(int index, int selectedIndex, HashSet<int> multiSelectedIndices)
		{
			if (selectedIndex == index || multiSelectedIndices.Contains(index))
				return colorScheme.SelectedRowBackground;

			return index % 2 == 0 ? colorScheme.EvenRowBackground : colorScheme.OddRowBackground;
		}

		private float CalculateTextWidth(Rect entryRect, bool isCollapsed)
		{
			float rightPadding = 5f;
			float countCircleSize = isCollapsed ? COUNT_CIRCLE_SIZE : 0;
			return entryRect.width - 20 - rightPadding - countCircleSize;
		}

		private float CalculateLogListHeight(List<LogEntry> logs)
		{
			float availableHeight = EditorGUIUtility.currentViewWidth - TOOLBAR_HEIGHT - 10;
			float totalContentHeight = Mathf.Max(logs.Count * LOG_ENTRY_HEIGHT, MIN_LOG_LIST_HEIGHT);
			return Mathf.Min(availableHeight, totalContentHeight);
		}

		private Texture2D MakeColorTexture(int width, int height, Color color)
		{
			Color[] pixels = new Color[width * height];
			for (int i = 0; i < pixels.Length; i++)
				pixels[i] = color;

			Texture2D texture = new(width, height);
			texture.SetPixels(pixels);
			texture.Apply();
			return texture;
		}
	}
}
#endif

