#if UNITY_EDITOR
using UnityEditor;

namespace Tools.EnhanceConsole.Editor
{
	/// <summary>
	/// PreferencesManager: Manages all preference loading/saving.
	/// Responsibility: Handle EditorPrefs for the Enhanced Console.
	/// </summary>
	public class PreferencesManager
	{
		private const string PREF_CLEAR_ON_PLAY = "EnhancedConsole_ClearOnPlay";
		private const string PREF_CLEAR_ON_BUILD = "EnhancedConsole_ClearOnBuild";
		private const string PREF_CLEAR_ON_RECOMPILE = "EnhancedConsole_ClearOnRecompile";
		private const string PREF_SHOW_TIME = "EnhancedConsole_ShowTime";
		private const string PREF_DETAILS_HEIGHT = "EnhancedConsole_DetailsHeight";

		private bool _clearOnPlay;
		private bool _clearOnBuild;
		private bool _clearOnRecompile;
		private bool _showTime;
		private float _detailsHeight;

		public bool ClearOnPlay => _clearOnPlay;
		public bool ClearOnBuild => _clearOnBuild;
		public bool ClearOnRecompile => _clearOnRecompile;
		public bool ShowTime => _showTime;
		public float DetailsHeight => _detailsHeight;

		public void LoadPreferences()
		{
			_clearOnPlay = EditorPrefs.GetBool(PREF_CLEAR_ON_PLAY, false);
			_clearOnBuild = EditorPrefs.GetBool(PREF_CLEAR_ON_BUILD, false);
			_clearOnRecompile = EditorPrefs.GetBool(PREF_CLEAR_ON_RECOMPILE, false);
			_showTime = EditorPrefs.GetBool(PREF_SHOW_TIME, false);
			_detailsHeight = EditorPrefs.GetFloat(PREF_DETAILS_HEIGHT, 200f);
		}

		public void SavePreferences()
		{
			EditorPrefs.SetBool(PREF_CLEAR_ON_PLAY, _clearOnPlay);
			EditorPrefs.SetBool(PREF_CLEAR_ON_BUILD, _clearOnBuild);
			EditorPrefs.SetBool(PREF_CLEAR_ON_RECOMPILE, _clearOnRecompile);
			EditorPrefs.SetBool(PREF_SHOW_TIME, _showTime);
			EditorPrefs.SetFloat(PREF_DETAILS_HEIGHT, _detailsHeight);
		}

		public void SetClearOnPlay(bool value) => _clearOnPlay = value;
		public void SetClearOnBuild(bool value) => _clearOnBuild = value;
		public void SetClearOnRecompile(bool value) => _clearOnRecompile = value;
		public void SetShowTime(bool value) => _showTime = value;
		public void SetDetailsHeight(float value) => _detailsHeight = value;
	}
}
#endif



