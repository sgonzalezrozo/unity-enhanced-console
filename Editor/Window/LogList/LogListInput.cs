#if UNITY_EDITOR
using System.Collections.Generic;
using EnhancedConsole.Editor.Window.Toolbar;
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor.Window.LogList
{
    /// <summary>
    /// Mouse and keyboard input for the log list. Keys typed into the search field are left to it.
    /// </summary>
    internal sealed class LogListInput
    {
        private readonly ConsoleContext context;
        private readonly LogListView listView;
        private readonly LogActions actions;

        public LogListInput(ConsoleContext consoleContext, LogListView logListView)
        {
            context = consoleContext;
            listView = logListView;
            actions = new LogActions(context);
        }

        // Enter or Return opens the selected log's source. Call this before the toolbar draws: the search field
        // uses Return to start editing whenever it has keyboard focus, even when nobody is typing in it, so the
        // key never reached the list. With nothing selected, Return still goes to the search field.
        public void HandleOpenKey(List<LogEntry> logs)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown && !EditorGUIUtility.editingTextField &&
                currentEvent.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
                OpenSelected(currentEvent, logs);
        }

        public void Handle(List<LogEntry> logs)
        {
            Event currentEvent = Event.current;
            HandleMouse(currentEvent, logs);
            if (EditorGUIUtility.editingTextField)
                return;

            HandleShortcuts(currentEvent, logs);
            HandleDelete(currentEvent, logs);
            HandleNavigation(currentEvent, logs);
        }

        private void HandleMouse(Event currentEvent, List<LogEntry> logs)
        {
            if (currentEvent.type != EventType.MouseDown)
                return;

            int index = listView.GetIndexAt(currentEvent.mousePosition);
            if (index < 0 || index >= logs.Count)
                return;

            if (currentEvent.clickCount == 2)
            {
                actions.OpenSource(logs[index]);
                return;
            }

            SelectClickedRow(currentEvent, index);
            actions.PingContext(logs[index]);
        }

        private void SelectClickedRow(Event currentEvent, int index)
        {
            if (currentEvent.shift)
                context.Selection.ExtendTo(index);
            else if (currentEvent.control || currentEvent.command)
                context.Selection.Toggle(index);
            else
                context.Selection.Select(index);

            GUI.FocusControl(null);
            currentEvent.Use();
        }

        private void HandleShortcuts(Event currentEvent, List<LogEntry> logs)
        {
            if (currentEvent.type != EventType.KeyDown || (!currentEvent.control && !currentEvent.command))
                return;

            if (currentEvent.keyCode == KeyCode.A && logs.Count > 0)
                context.Selection.SelectAll(logs.Count);
            else if (currentEvent.keyCode == KeyCode.C && context.Selection.HasSelection)
                actions.CopySelected(logs);
            else if (currentEvent.keyCode == KeyCode.F)
                SearchField.Focus();
            else
                return;

            currentEvent.Use();
            context.Repaint();
        }

        // Delete (Backspace or Fn+Delete on a Mac keyboard) removes the selected logs. Edit > Delete arrives as
        // a SoftDelete or Delete command and does the same.
        private void HandleDelete(Event currentEvent, List<LogEntry> logs)
        {
            bool isDeleteKey = currentEvent.type == EventType.KeyDown &&
                               currentEvent.keyCode is KeyCode.Delete or KeyCode.Backspace;
            bool isDeleteCommand = currentEvent.type is EventType.ValidateCommand or EventType.ExecuteCommand &&
                                   currentEvent.commandName is "SoftDelete" or "Delete";
            if ((!isDeleteKey && !isDeleteCommand) || !context.Selection.HasSelection)
                return;

            // Validating only tells Unity the command is handled here, so it sends ExecuteCommand.
            if (currentEvent.type != EventType.ValidateCommand)
                actions.DeleteSelected(logs);

            currentEvent.Use();
        }

        // Up/Down, Page Up/Page Down and Home/End move the selection (Shift extends it).
        private void HandleNavigation(Event currentEvent, List<LogEntry> logs)
        {
            if (currentEvent.type == EventType.KeyDown && logs.Count > 0 &&
                LogSelection.IsNavigationKey(currentEvent.keyCode))
                MoveSelection(currentEvent, logs.Count - 1);
        }

        private void OpenSelected(Event currentEvent, List<LogEntry> logs)
        {
            int selectedIndex = context.Selection.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= logs.Count)
                return;

            actions.OpenSource(logs[selectedIndex]);
            currentEvent.Use();
        }

        private void MoveSelection(Event currentEvent, int lastIndex)
        {
            currentEvent.Use();
            int target = context.Selection.GetNavigationTarget(currentEvent.keyCode, lastIndex, listView.PageSize);
            if (target == context.Selection.SelectedIndex)
                return;

            if (currentEvent.shift)
                context.Selection.ExtendTo(target);
            else
                context.Selection.Select(target);

            listView.RequestScrollToSelection();
            context.Repaint();
        }
    }
}
#endif
