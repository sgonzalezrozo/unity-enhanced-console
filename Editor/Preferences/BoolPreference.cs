#if UNITY_EDITOR
using UnityEditor;

namespace EnhancedConsole.Editor.Preferences
{
    /// <summary>
    /// A bool stored in EditorPrefs. Read once, then saved on every change.
    /// </summary>
    internal sealed class BoolPreference
    {
        private readonly string key;

        public BoolPreference(string preferenceKey, bool defaultValue)
        {
            key = preferenceKey;
            Value = EditorPrefs.GetBool(key, defaultValue);
        }

        public bool Value { get; private set; }

        public void Set(bool value)
        {
            if (Value == value)
                return;

            Value = value;
            EditorPrefs.SetBool(key, value);
        }

        public void Toggle()
        {
            Set(!Value);
        }
    }
}
#endif
