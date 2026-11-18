#if UNITY_EDITOR
using UnityEditor;

namespace EnhancedConsole.Editor.Preferences
{
    /// <summary>
    /// A float stored in EditorPrefs. It can change without saving, for values that change while dragging.
    /// </summary>
    internal sealed class FloatPreference
    {
        private readonly string key;

        public FloatPreference(string preferenceKey, float defaultValue)
        {
            key = preferenceKey;
            Value = EditorPrefs.GetFloat(key, defaultValue);
        }

        public float Value { get; private set; }

        public void SetUnsaved(float value)
        {
            Value = value;
        }

        public void Save()
        {
            EditorPrefs.SetFloat(key, Value);
        }
    }
}
#endif
