#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedConsole.Editor.Window
{
    /// <summary>
    /// Which logs are selected. The selection is stored by index into the filtered view; the selected entry is
    /// kept so the selection can be found again when the view is rebuilt (filter change, collapse, pruning).
    /// </summary>
    internal sealed class LogSelection
    {
        private readonly HashSet<int> extraIndices = new();
        private LogEntry selectedEntry;
        private int seenResetCount = -1;

        public int SelectedIndex { get; private set; } = -1;

        public bool HasSelection => SelectedIndex >= 0 || extraIndices.Count > 0;

        public bool IsSelected(int index)
        {
            return index == SelectedIndex || extraIndices.Contains(index);
        }

        public void UpdateForView(List<LogEntry> logs, int resetCount)
        {
            if (resetCount == seenResetCount)
                return;

            seenResetCount = resetCount;
            SelectedIndex = selectedEntry != null ? logs.IndexOf(selectedEntry) : -1;
            extraIndices.Clear();
        }

        public void RememberSelectedEntry(List<LogEntry> logs)
        {
            selectedEntry = SelectedIndex >= 0 && SelectedIndex < logs.Count ? logs[SelectedIndex] : null;
        }

        public void Clear()
        {
            SelectedIndex = -1;
            selectedEntry = null;
            extraIndices.Clear();
        }

        public void Select(int index)
        {
            extraIndices.Clear();
            SelectedIndex = index;
        }

        // Adds every row between the current selection and index, like Shift-click.
        public void ExtendTo(int index)
        {
            if (SelectedIndex < 0)
            {
                Select(index);
                return;
            }

            int from = Mathf.Min(SelectedIndex, index);
            int to = Mathf.Max(SelectedIndex, index);
            for (int i = from; i <= to; i++)
                extraIndices.Add(i);

            SelectedIndex = index;
        }

        public void Toggle(int index)
        {
            if (!extraIndices.Remove(index))
                extraIndices.Add(index);

            SelectedIndex = index;
        }

        public void SelectAll(int count)
        {
            extraIndices.Clear();
            for (int i = 0; i < count; i++)
                extraIndices.Add(i);

            SelectedIndex = 0;
        }

        public static bool IsNavigationKey(KeyCode key)
        {
            return key is KeyCode.UpArrow or KeyCode.DownArrow or KeyCode.PageUp or KeyCode.PageDown
                or KeyCode.Home or KeyCode.End;
        }

        // Where a navigation key moves the selection. With nothing selected, Up and Page Up start at the end.
        public int GetNavigationTarget(KeyCode key, int lastIndex, int pageSize)
        {
            bool hasSelection = SelectedIndex >= 0;
            int target = key switch
            {
                KeyCode.UpArrow => hasSelection ? SelectedIndex - 1 : lastIndex,
                KeyCode.DownArrow => hasSelection ? SelectedIndex + 1 : 0,
                KeyCode.PageUp => hasSelection ? SelectedIndex - pageSize : lastIndex,
                KeyCode.PageDown => hasSelection ? SelectedIndex + pageSize : 0,
                KeyCode.Home => 0,
                KeyCode.End => lastIndex,
                _ => SelectedIndex
            };

            return Mathf.Clamp(target, 0, lastIndex);
        }

        // The selected logs in list order.
        public List<LogEntry> GetSelectedLogs(List<LogEntry> logs)
        {
            List<int> indices = new(extraIndices);
            if (SelectedIndex >= 0 && !extraIndices.Contains(SelectedIndex))
                indices.Add(SelectedIndex);
            indices.Sort();

            List<LogEntry> selected = new(indices.Count);
            for (int i = 0; i < indices.Count; i++)
            {
                if (indices[i] < logs.Count)
                    selected.Add(logs[indices[i]]);
            }

            return selected;
        }
    }
}
#endif
