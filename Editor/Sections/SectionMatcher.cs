#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EnhancedConsole.Editor.Sections
{
    /// <summary>
    /// Matches one section's name or regex against log text, without allocating or throwing per call.
    /// Plain names match case-insensitively and ignore spaces. Create it once per section and reuse it.
    /// </summary>
    internal readonly struct SectionMatcher
    {
        private const RegexOptions REGEX_OPTIONS = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private static readonly HashSet<string> REPORTED_INVALID_PATTERNS = new();

        private readonly Regex regex;
        private readonly string upperNeedle;
        private readonly bool ignoresSpaces;

        private SectionMatcher(Regex sectionRegex, string needle, bool needleIgnoresSpaces)
        {
            regex = sectionRegex;
            upperNeedle = needle;
            ignoresSpaces = needleIgnoresSpaces;
        }

        public static SectionMatcher Create(SelectiveLogSection section)
        {
            string name = section.name ?? "";
            if (!section.useRegex)
                return new SectionMatcher(null, name.Replace(" ", "").ToUpperInvariant(), true);

            string pattern = string.IsNullOrEmpty(section.pattern) ? name : section.pattern;
            Regex parsed = ParseRegex(name, pattern);
            return parsed != null
                ? new SectionMatcher(parsed, null, false)
                : new SectionMatcher(null, name.ToUpperInvariant(), false);
        }

        public bool Matches(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (regex != null)
                return regex.IsMatch(text);

            // An empty name matches nothing.
            if (string.IsNullOrEmpty(upperNeedle))
                return false;

            return ignoresSpaces
                ? ContainsIgnoringSpaces(text, upperNeedle)
                : text.IndexOf(upperNeedle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // Returns null for an empty or invalid pattern.
        private static Regex ParseRegex(string sectionName, string pattern)
        {
            if (pattern.Length == 0)
                return null;

            try
            {
                return new Regex(pattern, REGEX_OPTIONS);
            }
            catch (ArgumentException exception)
            {
                ReportInvalidPattern(sectionName, pattern, exception);
                return null;
            }
        }

        // The section then matches its name as plain text. Each pattern is reported once, since sections are
        // rebuilt whenever the config changes.
        private static void ReportInvalidPattern(string sectionName, string pattern, ArgumentException exception)
        {
            if (!REPORTED_INVALID_PATTERNS.Add(pattern))
                return;

            Debug.LogWarning($"[EnhancedConsole] Section \"{sectionName}\" has an invalid regex \"{pattern}\", " +
                             $"so its name is matched as plain text instead. {exception.Message}");
        }

        // Case-insensitive search for an upper-case, space-free needle, skipping spaces in the text. Equivalent to
        // comparing both strings with spaces removed, without allocating.
        private static bool ContainsIgnoringSpaces(string text, string needle)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (char.ToUpperInvariant(text[i]) == needle[0] && MatchesIgnoringSpacesAt(text, i + 1, needle))
                    return true;
            }

            return false;
        }

        private static bool MatchesIgnoringSpacesAt(string text, int textIndex, string needle)
        {
            int needleIndex = 1;
            for (; needleIndex < needle.Length && textIndex < text.Length; textIndex++)
            {
                char character = text[textIndex];
                if (character == ' ')
                    continue;

                if (char.ToUpperInvariant(character) != needle[needleIndex])
                    return false;

                needleIndex++;
            }

            return needleIndex == needle.Length;
        }
    }
}
#endif
