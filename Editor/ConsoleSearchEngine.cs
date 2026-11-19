#if UNITY_EDITOR
namespace EnhancedConsole.Editor
{
    /// <summary>
    /// Matches the search text against a log's message and stack trace, ignoring case like
    /// StringComparison.OrdinalIgnoreCase.
    /// </summary>
    internal class ConsoleSearchEngine
    {
        private const char ASCII_END = (char)128;
        private const int ASCII_CASE_OFFSET = 'a' - 'A';

        private string searchFilter = "";
        private string upperSearchFilter = "";

        public void SetSearchFilter(string filter)
        {
            searchFilter = filter ?? "";
            upperSearchFilter = searchFilter.ToUpperInvariant();
        }

        public string GetSearchFilter()
        {
            return searchFilter;
        }

        public bool MatchesSearchFilter(LogEntry log)
        {
            if (upperSearchFilter.Length == 0)
                return true;

            return ContainsIgnoringCase(log.message, upperSearchFilter) ||
                   ContainsIgnoringCase(log.stackTrace, upperSearchFilter);
        }

        // Same result as text.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 for an upper-case needle,
        // several times faster in the Editor's runtime: ASCII letters are folded inline, and only other characters
        // go through char.ToUpperInvariant, which is what OrdinalIgnoreCase compares.
        internal static bool ContainsIgnoringCase(string text, string upperNeedle)
        {
            if (string.IsNullOrEmpty(text) || text.Length < upperNeedle.Length)
                return false;

            char first = upperNeedle[0];
            int lastStart = text.Length - upperNeedle.Length;
            for (int start = 0; start <= lastStart; start++)
            {
                if (ToUpper(text[start]) == first && MatchesAt(text, start, upperNeedle))
                    return true;
            }

            return false;
        }

        private static bool MatchesAt(string text, int start, string upperNeedle)
        {
            for (int i = 1; i < upperNeedle.Length; i++)
            {
                if (ToUpper(text[start + i]) != upperNeedle[i])
                    return false;
            }

            return true;
        }

        private static char ToUpper(char character)
        {
            if (character >= ASCII_END)
                return char.ToUpperInvariant(character);

            return character >= 'a' && character <= 'z' ? (char)(character - ASCII_CASE_OFFSET) : character;
        }
    }
}
#endif
