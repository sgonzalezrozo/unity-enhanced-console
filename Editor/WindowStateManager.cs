#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Tools.EnhanceConsole.Editor
{
    /// <summary>
    /// WindowStateManager: Manages window state like focus and initialization.
    /// Responsibility: Track window focus, styles initialization.
    /// </summary>
    public class WindowStateManager
    {
        private bool _hadFocus = true;
        private bool _isStylesInitialized;
        private SelectiveLogsScriptableObject _selectiveLogsConfig;

        public bool IsStylesInitialized => _isStylesInitialized;
        public SelectiveLogsScriptableObject SelectiveLogsConfig => _selectiveLogsConfig;

        public void UpdateFocus(EditorWindow window)
        {
            if (!_hadFocus && window.hasFocus)
            {
                _hadFocus = true;
                LoadSelectiveLogsConfig();
                return;
            }

            _hadFocus = window.hasFocus;
        }

        public void InitializeStyles(object viewRenderer)
        {
            if (_isStylesInitialized)
                return;

            _isStylesInitialized = true;
        }

        public void LoadSelectiveLogsConfig()
        {
            _selectiveLogsConfig = Resources.Load<SelectiveLogsScriptableObject>("Tools/EnhanceConsole/SelectiveLogsConfig");
        }

        public void Reset()
        {
            _isStylesInitialized = false;
            _hadFocus = true;
        }
    }
}
#endif

