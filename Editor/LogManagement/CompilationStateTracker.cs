#if UNITY_EDITOR
using UnityEngine;

namespace Tools.EnhanceConsole.Editor.LogManagement
{
	/// <summary>
	/// Tracks compilation state and error status.
	/// Single Responsibility: Compilation state tracking only.
	/// </summary>
	internal class CompilationStateTracker
	{
		private readonly LogEntryManager logManager;

		public bool IsCompiling { get; private set; }
		public bool HasErrors { get; private set; }
		public bool PreviousCollapseState { get; set; }

		public CompilationStateTracker(LogEntryManager logManager)
		{
			this.logManager = logManager ?? throw new System.ArgumentNullException(nameof(logManager));
		}

		public void CheckForExistingErrors()
		{
			HasErrors = CheckIfErrorsExist();
		}

		public void UpdateErrorStatus()
		{
			if (IsCompiling)
				return;

			bool hasErrors = CheckIfErrorsExist();

			if (hasErrors && !HasErrors)
			{
				HasErrors = true;
			}
			else if (!hasErrors && HasErrors)
			{
				HasErrors = false;
			}
		}

		public void SetCompiling(bool isCompiling)
		{
			IsCompiling = isCompiling;
		}

		public void ClearErrors()
		{
			HasErrors = false;
		}

		private bool CheckIfErrorsExist()
		{
			foreach (LogEntry entry in logManager.GetEntries())
			{
				if (entry.Type == LogType.Error || entry.Type == LogType.Exception || entry.Type == LogType.Assert)
					return true;
			}

			return false;
		}
	}
}
#endif

