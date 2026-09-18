using System.Text.RegularExpressions;

namespace BuddyBee.Api.Services
{
    public static class SpeechTextNormalizer
    {
        public static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var s = text;

            // 1. Remove code blocks entirely (they're rarely useful to speak)
            s = Regex.Replace(s, @"```[\s\S]*?```", "", RegexOptions.Multiline);

            // 2. Remove inline code backticks
            s = Regex.Replace(s, @"`([^`]+)`", "$1");

            // 3. Convert Markdown links [text](url) -> "text" (keep readable text)
            s = Regex.Replace(s, @"\[([^\]]+)\]\([^)]+\)", "$1");

            // 4. Remove bare URLs (http/https)
            s = Regex.Replace(s, @"https?://\S+", "");

            // 5. Remove Markdown headings (# ## ### etc.)
            s = Regex.Replace(s, @"^\s*#{1,6}\s*", "", RegexOptions.Multiline);

            // 6. Remove bold/italic markers (**, *, __)
            s = Regex.Replace(s, @"\*\*([^\*]+)\*\*", "$1");
            s = Regex.Replace(s, @"\*([^\*]+)\*", "$1");
            s = Regex.Replace(s, @"__([^_]+)__", "$1");
            s = Regex.Replace(s, @"_([^_]+)_", "$1");

            // 7. Remove stray markdown punctuation when not meaningful
            // Keep: sentence punctuation .,!?;: and numbers with decimals/commas
            // Remove isolated: *, #, _, [, ], {, }, ~, ^
            s = Regex.Replace(s, @"(?<!\d)[*#_\[\]{}~^](?!\d)", "");

            // 8. Remove citation-like patterns e.g. [1], [source]
            s = Regex.Replace(s, @"\[\s*\d+\s*\]", "");

            // 9. Normalize whitespace: collapse multiple spaces/newlines
            s = Regex.Replace(s, @"[ \t]+", " ");
            s = Regex.Replace(s, @"\n{3,}", "\n\n");
            s = Regex.Replace(s, @"\n\s*\n", " ");

            return s.Trim();
        }
    }
}