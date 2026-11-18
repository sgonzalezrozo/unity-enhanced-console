#if UNITY_EDITOR
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EnhancedConsole.Editor.Sections
{
    /// <summary>
    /// Hashes of a section config, so edits are noticed without comparing every field. The match signature
    /// covers what decides which section a log matches; the state signature covers what's shown.
    /// </summary>
    internal static class SectionSignature
    {
        private const int SEED = 17;
        private const int MULTIPLIER = 31;

        // 0 means "not computed yet", so a computed signature is never 0.
        public const int NONE = 0;

        public static int ComputeMatchSignature(SelectiveLogsScriptableObject config)
        {
            unchecked
            {
                int hash = HashMatchFields(SEED, config.SystemDefinedSections);
                return HashMatchFields(hash, config.UserDefinedSections) | 1;
            }
        }

        // Enabled state and color don't change which section matches. The section reference is included so
        // replaced objects (undo, reimport) are picked up.
        public static int ComputeStateSignature(SelectiveLogsScriptableObject config)
        {
            unchecked
            {
                int hash = HashStateFields(SEED, config.SystemDefinedSections);
                return HashStateFields(hash, config.UserDefinedSections) | 1;
            }
        }

        private static int HashMatchFields(int hash, IReadOnlyList<SelectiveLogSection> sections)
        {
            unchecked
            {
                hash = hash * MULTIPLIER + sections.Count;
                for (int i = 0; i < sections.Count; i++)
                {
                    SelectiveLogSection section = sections[i];
                    hash = hash * MULTIPLIER + (section?.name?.GetHashCode() ?? 0);
                    hash = hash * MULTIPLIER + (section?.pattern?.GetHashCode() ?? 0);
                    hash = hash * MULTIPLIER + (section == null ? 0 : section.useRegex ? 1 : 2);
                }

                return hash;
            }
        }

        private static int HashStateFields(int hash, IReadOnlyList<SelectiveLogSection> sections)
        {
            unchecked
            {
                for (int i = 0; i < sections.Count; i++)
                {
                    SelectiveLogSection section = sections[i];
                    if (section == null)
                        continue;

                    hash = hash * MULTIPLIER + (section.enabled ? 1 : 2);
                    hash = hash * MULTIPLIER + section.color.GetHashCode();
                    hash = hash * MULTIPLIER + RuntimeHelpers.GetHashCode(section);
                }

                return hash;
            }
        }
    }
}
#endif
