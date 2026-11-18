#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    /// <summary>
    /// A built-in Editor icon, loaded on first use. Unity's null check reloads it after a skin change unloads it.
    /// </summary>
    internal sealed class CachedIcon
    {
        private readonly string iconName;
        private Texture2D icon;

        public CachedIcon(string name)
        {
            iconName = name;
        }

        public Texture2D Get()
        {
            if (icon == null)
                icon = EditorGUIUtility.IconContent(iconName).image as Texture2D;

            return icon;
        }
    }
}
#endif
