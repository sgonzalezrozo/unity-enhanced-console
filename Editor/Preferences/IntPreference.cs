#if UNITY_EDITOR
using UnityEditor;

namespace EnhancedConsole.Editor.Preferences
{
    /// <summary>
    /// An int stored in EditorPrefs. Read once, then saved on every change.
    /// </summary>
    internal sealed class IntPreference
    {
        private readonly string key;

        public IntPreference(string preferenceKey, int defaultValue)
        {
            key = preferenceKey;
            Value = EditorPrefs.GetInt(key, defaultValue);
        }

        public int Value { get; private set; }

        public void Set(int value)
        {
            if (Value == value)
                return;

            Value = value;
            EditorPrefs.SetInt(key, value);
        }
    }
}
#endif
