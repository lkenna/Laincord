namespace Laincord.Helpers
{
    public static class TwemojiHelper
    {
        public static string GetUrl(string emojiText)
        {
            var codepoints = new List<string>();
            for (int j = 0; j < emojiText.Length; j++)
            {
                int cp;
                if (char.IsHighSurrogate(emojiText[j]) && j + 1 < emojiText.Length && char.IsLowSurrogate(emojiText[j + 1]))
                {
                    cp = char.ConvertToUtf32(emojiText[j], emojiText[j + 1]);
                    j++;
                }
                else
                {
                    cp = emojiText[j];
                }
                // Skip variation selectors (U+FE0E, U+FE0F)
                if (cp == 0xFE0E || cp == 0xFE0F) continue;
                codepoints.Add(cp.ToString("x"));
            }
            if (codepoints.Count == 0) return null;

            string twemojiId = string.Join("-", codepoints);
            return $"https://cdn.jsdelivr.net/gh/twitter/twemoji@latest/assets/72x72/{twemojiId}.png";
        }
    }
}
