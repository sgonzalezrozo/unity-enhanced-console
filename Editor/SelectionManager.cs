#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// SelectionManager: Manages log entry selection and multi-select.
	/// Responsibility: Track selected logs, handle selection modes (single, shift, ctrl).
	/// </summary>
	public class SelectionManager
	{
		private int _selectedIndex = -1;
		private readonly HashSet<int> _multiSelectedIndices = new();

		public int SelectedIndex => _selectedIndex;
		public HashSet<int> MultiSelectedIndices => _multiSelectedIndices;

		public void HandleLogEntrySelection(int index, Event currentEvent)
		{
			if (currentEvent.shift && _selectedIndex >= 0)
				HandleShiftSelection(index);
			else if (currentEvent.control || currentEvent.command)
				HandleCtrlSelection(index);
			else
				HandleSingleSelection(index);

			_selectedIndex = index;
			GUI.FocusControl(null);
			currentEvent.Use();
		}

		private void HandleShiftSelection(int index)
		{
			int start = Mathf.Min(_selectedIndex, index);
			int end = Mathf.Max(_selectedIndex, index);
			for (int j = start; j <= end; j++)
				_multiSelectedIndices.Add(j);
		}

		private void HandleCtrlSelection(int index)
		{
			if (_multiSelectedIndices.Contains(index))
				_multiSelectedIndices.Remove(index);
			else
				_multiSelectedIndices.Add(index);
		}

		private void HandleSingleSelection(int index)
		{
			_multiSelectedIndices.Clear();
		}

		public void SelectAll(int logCount)
		{
			_multiSelectedIndices.Clear();
			for (int i = 0; i < logCount; i++)
				_multiSelectedIndices.Add(i);
			_selectedIndex = 0;
		}

		public void Clear()
		{
			_selectedIndex = -1;
			_multiSelectedIndices.Clear();
		}

		public bool HasSelection()
		{
			return _selectedIndex >= 0 || _multiSelectedIndices.Count > 0;
		}

		public List<int> GetSelectedIndices()
		{
			HashSet<int> allSelected = new(_multiSelectedIndices);
			if (_selectedIndex >= 0)
				allSelected.Add(_selectedIndex);

			List<int> sorted = new(allSelected);
			sorted.Sort();
			return sorted;
		}
	}
}
#endif



