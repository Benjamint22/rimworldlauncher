using System.Text.RegularExpressions;

namespace RimWorldLauncher
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Returns a version of <paramref name="dirtyStr" /> that is safe for Windows file paths.
        /// </summary>
        /// <param name="dirtyStr">The string to sanitize.</param>
        /// <returns>The sanitized string.</returns>
        public static string Sanitize(this string dirtyStr)
        {
            return Regex.Replace(Regex.Replace(dirtyStr.ToLower(), @"[\s\-]", "_"), @"[^a-zA-Z0-9_\.]", "");
        }
    }
}