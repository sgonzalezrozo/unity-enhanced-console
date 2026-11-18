#if UNITY_EDITOR
using System;
using EnhancedConsole.Editor.Sections;
using UnityEngine;

namespace EnhancedConsole.Editor
{
    [Serializable]
    public class SelectiveLogSection
    {
        [SerializeField] public string name;
        [SerializeField] public Color color;
        [SerializeField] public bool enabled = true;
        [SerializeField] public string pattern = "";
        [SerializeField] public bool useRegex;

        public SelectiveLogSection(string name, Color color, bool enabled = true)
        {
            this.name = name;
            this.color = color;
            this.enabled = enabled;
            this.pattern = "";
            this.useRegex = false;
        }

        public bool Matches(string text)
        {
            return SectionMatcher.Create(this).Matches(text);
        }
    }
}
#endif
