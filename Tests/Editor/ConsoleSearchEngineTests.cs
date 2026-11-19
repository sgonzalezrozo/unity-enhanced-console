using System;
using NUnit.Framework;

namespace EnhancedConsole.Editor.Tests
{
    /// <summary>The fast search gives the same answer as IndexOf with StringComparison.OrdinalIgnoreCase.</summary>
    public class ConsoleSearchEngineTests
    {
        private static readonly string[] TEXTS =
        {
            "PlayerController.Update (at Assets/Client/Player.cs:42)",
            "Ünïcödé Straße ÉCOLE istanbul İSTANBUL",
            "a", "", "[Shop] bought 3 items for 120 coins", "tab\tand\nnew line", "Ωmega ΣΙΓΜΑ σίγμα"
        };

        private static readonly string[] SEARCHES =
        {
            "player", "PLAYER", "cs:42", "straße", "STRASSE", "école", "İstanbul", "istanbul", "ı", "a", "aa",
            "[shop]", "and\nnew", "ωmega", "σιγμα", "missing", "120 coins", "Assets/Client/Player.cs:42)"
        };

        [Test]
        public void ContainsIgnoringCase_MatchesOrdinalIgnoreCase()
        {
            foreach (string text in TEXTS)
            {
                foreach (string search in SEARCHES)
                {
                    bool expected = text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                    bool actual = ConsoleSearchEngine.ContainsIgnoringCase(text, search.ToUpperInvariant());
                    Assert.AreEqual(expected, actual, $"\"{search}\" in \"{text}\"");
                }
            }
        }

        [Test]
        public void ANullText_DoesntMatch()
        {
            Assert.IsFalse(ConsoleSearchEngine.ContainsIgnoringCase(null, "A"));
        }
    }
}
