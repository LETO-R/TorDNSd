using System.Text.RegularExpressions;

namespace TorDNSd.Utils
{
    /// <summary>
    /// Glob matching utility methods.
    /// </summary>
    public static class Glob
    {
        /// <summary>
        /// Match the input versus the specified pattern (pattern can include ? and * to match)
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="pattern">Glob pattern.</param>
        /// <returns>True when it matches, false otherwise.</returns>
        public static bool Match(string input, string pattern)
        {
            return Regex.Match(input, Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + @"$").Success;
        }
    }
}
