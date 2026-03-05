using DSharpPlus.Entities;

namespace Laincord
{
    public static class EmojiCategories
    {
        public static readonly (string Name, string IconEmoji, List<(string DiscordName, string Unicode)> Emojis)[] Categories;

        static EmojiCategories()
        {
            // Build deduplicated emoji list (first name wins per unicode value)
            var seen = new Dictionary<string, string>(); // unicode -> first name
            foreach (var kvp in DiscordEmoji.UnicodeEmojis)
            {
                string name = kvp.Key.Trim(':');
                string unicode = kvp.Value;
                if (!seen.ContainsKey(unicode))
                    seen[unicode] = name;
            }

            // Bucket into categories
            var buckets = new Dictionary<string, List<(string, string)>>
            {
                ["Custom"] = new(),
                ["Smileys & People"] = new(),
                ["Animals & Nature"] = new(),
                ["Food & Drink"] = new(),
                ["Activities"] = new(),
                ["Travel & Places"] = new(),
                ["Objects"] = new(),
                ["Symbols"] = new(),
                ["Flags"] = new(),
            };

            // Add custom emoji from EmojiDictionary
            var customSeen = new HashSet<string>();
            foreach (var kvp in EmojiDictionary.Map)
            {
                if (customSeen.Add(kvp.Value))
                    buckets["Custom"].Add((kvp.Key, null));
            }

            // Categorize unicode emoji
            foreach (var kvp in seen)
            {
                string unicode = kvp.Key;
                string name = kvp.Value;
                string category = CategorizeEmoji(name, unicode);
                buckets[category].Add((name, unicode));
            }

            Categories = new (string, string, List<(string, string)>)[]
            {
                ("Custom", "\U0001f4be", buckets["Custom"]),           // floppy disk
                ("Smileys & People", "\U0001f600", buckets["Smileys & People"]),
                ("Animals & Nature", "\U0001f43e", buckets["Animals & Nature"]),
                ("Food & Drink", "\U0001f354", buckets["Food & Drink"]),
                ("Activities", "\u26bd", buckets["Activities"]),
                ("Travel & Places", "\U0001f697", buckets["Travel & Places"]),
                ("Objects", "\U0001f4a1", buckets["Objects"]),
                ("Symbols", "\u2764\ufe0f", buckets["Symbols"]),
                ("Flags", "\U0001f3f4", buckets["Flags"]),
            };
        }

        private static string CategorizeEmoji(string name, string unicode)
        {
            // Flags: name-based (most reliable)
            if (name.StartsWith("flag_") || name.StartsWith("regional_indicator"))
                return "Flags";

            int cp = GetFirstSignificantCodepoint(unicode);

            // Smileys & People
            if (cp >= 0x1F600 && cp <= 0x1F64F) return "Smileys & People"; // Emoticons
            if (cp >= 0x1F910 && cp <= 0x1F92F) return "Smileys & People"; // Face supplements
            if (cp >= 0x1F970 && cp <= 0x1F97A) return "Smileys & People"; // More faces
            if (cp >= 0x1FAE0 && cp <= 0x1FAEF) return "Smileys & People"; // Extended faces
            if (cp >= 0x1FAF0 && cp <= 0x1FAFF) return "Smileys & People"; // Extended hands
            if (cp >= 0x1F440 && cp <= 0x1F465) return "Smileys & People"; // Body parts, people silhouettes
            if (cp >= 0x1F466 && cp <= 0x1F487) return "Smileys & People"; // People
            if (cp >= 0x1F9D0 && cp <= 0x1F9FF) return "Smileys & People"; // People supplement
            if (cp >= 0x1F9B0 && cp <= 0x1F9B9) return "Smileys & People"; // Hair, superhero
            if (cp >= 0x1F90C && cp <= 0x1F90F) return "Smileys & People"; // Hand gestures
            if (cp >= 0x1F918 && cp <= 0x1F91F) return "Smileys & People"; // More hand gestures
            if (cp >= 0x1F930 && cp <= 0x1F93A) return "Smileys & People"; // Pregnant, fencing
            if (cp == 0x263A || cp == 0x2639) return "Smileys & People";   // Relaxed, frowning
            if (cp >= 0x270A && cp <= 0x270D) return "Smileys & People";   // Fist, hand writing
            if (cp == 0x261D) return "Smileys & People";                   // Index finger
            if (cp >= 0x1F4AA && cp <= 0x1F4AA) return "Smileys & People"; // Muscle
            if (cp == 0x1F9BE || cp == 0x1F9BF) return "Smileys & People"; // Prosthetic
            if (cp >= 0x1F48B && cp <= 0x1F498) return "Smileys & People"; // Kiss, love, hearts (people context)
            if (cp == 0x1F9E0) return "Smileys & People";                  // Brain
            if (cp >= 0x1FAC0 && cp <= 0x1FAC5) return "Smileys & People"; // Anatomical parts
            if (cp == 0x1F471 || cp == 0x1F473 || cp == 0x1F472) return "Smileys & People";
            if (cp >= 0x1F574 && cp <= 0x1F57A) return "Smileys & People"; // Levitating, dancing
            if (cp >= 0x1F645 && cp <= 0x1F64F) return "Smileys & People"; // Gestures

            // Animals & Nature
            if (cp >= 0x1F400 && cp <= 0x1F43F) return "Animals & Nature"; // Animals
            if (cp >= 0x1F980 && cp <= 0x1F9AF) return "Animals & Nature"; // More animals
            if (cp >= 0x1F330 && cp <= 0x1F343) return "Animals & Nature"; // Plants
            if (cp == 0x1F490) return "Animals & Nature";                  // Bouquet
            if (cp >= 0x1FAB0 && cp <= 0x1FABF) return "Animals & Nature"; // More animals/plants (14.0)
            if (cp >= 0x1F335 && cp <= 0x1F33E) return "Animals & Nature"; // More plants
            if (cp == 0x2618 || cp == 0x2619) return "Animals & Nature";   // Shamrock
            if (cp >= 0x1F340 && cp <= 0x1F344) return "Animals & Nature"; // Four leaf clover, mushroom

            // Food & Drink
            if (cp >= 0x1F345 && cp <= 0x1F37F) return "Food & Drink";
            if (cp >= 0x1F950 && cp <= 0x1F96F) return "Food & Drink";
            if (cp >= 0x1F9C0 && cp <= 0x1F9CB) return "Food & Drink";
            if (cp >= 0x1FAD0 && cp <= 0x1FADF) return "Food & Drink";
            if (cp == 0x2615) return "Food & Drink"; // Hot beverage

            // Activities
            if (cp >= 0x1F3A0 && cp <= 0x1F3CF) return "Activities";
            if (cp >= 0x1F93C && cp <= 0x1F93E) return "Activities";
            if (cp >= 0x1F94A && cp <= 0x1F94F) return "Activities";
            if (cp >= 0x1F396 && cp <= 0x1F39F) return "Activities";
            if (cp == 0x26BD || cp == 0x26BE) return "Activities";
            if (cp >= 0x1FA70 && cp <= 0x1FA7C) return "Activities";
            if (cp == 0x265F || cp == 0x2660 || cp == 0x2663 || cp == 0x2665 || cp == 0x2666) return "Activities"; // Cards, chess
            if (cp >= 0x1F0CF && cp <= 0x1F0CF) return "Activities"; // Joker
            if (cp == 0x1F3D0 || cp == 0x1F3D1 || cp == 0x1F3D2 || cp == 0x1F3D3) return "Activities"; // Sports balls

            // Travel & Places
            if (cp >= 0x1F680 && cp <= 0x1F6FF) return "Travel & Places";
            if (cp >= 0x1F300 && cp <= 0x1F32F) return "Travel & Places"; // Weather, globe
            if (cp >= 0x1F3D4 && cp <= 0x1F3DF) return "Travel & Places"; // Landscapes
            if (cp >= 0x1F3E0 && cp <= 0x1F3F0) return "Travel & Places"; // Buildings
            if (cp == 0x2708 || cp == 0x26F5 || cp == 0x2693) return "Travel & Places";
            if (cp >= 0x26EA && cp <= 0x26FA) return "Travel & Places";
            if (cp == 0x1F30D || cp == 0x1F30E || cp == 0x1F30F) return "Travel & Places";
            if (cp == 0x26F0 || cp == 0x26F1 || cp == 0x26F2 || cp == 0x26F3) return "Travel & Places";
            if (cp == 0x2600 || cp == 0x2601 || cp == 0x2602 || cp == 0x2603 || cp == 0x2604) return "Travel & Places"; // Weather
            if (cp == 0x26C4 || cp == 0x26C5 || cp == 0x26C8) return "Travel & Places";
            if (cp == 0x1F549 || cp == 0x1F54A) return "Symbols"; // Om, dove -> symbols
            if (cp >= 0x1F6E0 && cp <= 0x1F6EC) return "Travel & Places";
            if (cp >= 0x1F6F0 && cp <= 0x1F6FC) return "Travel & Places";

            // Objects
            if (cp >= 0x1F4A1 && cp <= 0x1F4A9) return "Objects"; // Lightbulb through poop
            if (cp >= 0x1F4A0 && cp <= 0x1F4A0) return "Objects"; // Diamond
            if (cp >= 0x1F4AB && cp <= 0x1F4FF) return "Objects";
            if (cp >= 0x1F500 && cp <= 0x1F573) return "Objects";
            if (cp >= 0x1F578 && cp <= 0x1F5FF) return "Objects";
            if (cp >= 0x1F9E1 && cp <= 0x1F9FF) return "Objects"; // Various objects supplement
            if (cp >= 0x1FA80 && cp <= 0x1FA8F) return "Objects";
            if (cp >= 0x1FA90 && cp <= 0x1FAA8) return "Objects";
            if (cp >= 0x1FAA9 && cp <= 0x1FAAF) return "Objects";
            if (cp == 0x231A || cp == 0x231B) return "Objects"; // Watch, hourglass
            if (cp == 0x2328) return "Objects"; // Keyboard
            if (cp >= 0x23E9 && cp <= 0x23F3) return "Objects"; // Media controls
            if (cp == 0x23F0 || cp == 0x23F1 || cp == 0x23F2 || cp == 0x23F3) return "Objects"; // Clocks
            if (cp >= 0x1F3F3 && cp <= 0x1F3F5) return "Objects"; // Flags (non-country)
            if (cp == 0x2702 || cp == 0x2709 || cp == 0x270F || cp == 0x2712) return "Objects"; // Scissors, envelope, pencil
            if (cp >= 0x1F489 && cp <= 0x1F48A) return "Objects"; // Syringe, pill

            // Symbols
            if (cp >= 0x2600 && cp <= 0x26FF) return "Symbols"; // Misc symbols (catch-all)
            if (cp >= 0x2700 && cp <= 0x27BF) return "Symbols"; // Dingbats
            if (cp >= 0x2B05 && cp <= 0x2B55) return "Symbols"; // Arrows, shapes
            if (cp >= 0x1F170 && cp <= 0x1F1FF) return "Symbols"; // Enclosed characters
            if (cp >= 0x2194 && cp <= 0x21AA) return "Symbols"; // Arrows
            if (cp >= 0x25AA && cp <= 0x25FE) return "Symbols"; // Geometric shapes
            if (cp >= 0x2764 && cp <= 0x2764) return "Symbols"; // Heart
            if (cp >= 0x2733 && cp <= 0x274E) return "Symbols";
            if (cp >= 0x2753 && cp <= 0x2757) return "Symbols"; // Question marks, exclamation
            if (cp >= 0x2795 && cp <= 0x2797) return "Symbols"; // Plus, minus, divide
            if (cp >= 0x3030 && cp <= 0x3030) return "Symbols"; // Wavy dash
            if (cp >= 0x303D && cp <= 0x303D) return "Symbols";
            if (cp >= 0x3297 && cp <= 0x3299) return "Symbols";
            if (cp == 0x00A9 || cp == 0x00AE) return "Symbols"; // Copyright, registered
            if (cp == 0x2122) return "Symbols"; // TM
            if (cp >= 0x1F500 && cp <= 0x1F53D) return "Symbols"; // Shuffle, repeat, play buttons
            if (cp >= 0x1F549 && cp <= 0x1F54E) return "Symbols"; // Religious symbols
            if (cp >= 0x1F550 && cp <= 0x1F567) return "Symbols"; // Clock faces
            if (cp >= 0x200D && cp <= 0x200D) return "Smileys & People"; // ZWJ sequences
            if (cp == 0x2934 || cp == 0x2935) return "Symbols"; // Arrows

            // Flags catch-all
            if (cp >= 0x1F1E0 && cp <= 0x1F1FF) return "Flags";

            // Default to Symbols
            return "Symbols";
        }

        private static int GetFirstSignificantCodepoint(string unicode)
        {
            for (int i = 0; i < unicode.Length; i++)
            {
                int cp;
                if (char.IsHighSurrogate(unicode[i]) && i + 1 < unicode.Length && char.IsLowSurrogate(unicode[i + 1]))
                {
                    cp = char.ConvertToUtf32(unicode[i], unicode[i + 1]);
                    i++;
                }
                else
                {
                    cp = unicode[i];
                }

                // Skip ZWJ, variation selectors, skin tone modifiers
                if (cp == 0x200D) continue;
                if (cp == 0xFE0E || cp == 0xFE0F) continue;
                if (cp >= 0x1F3FB && cp <= 0x1F3FF) continue;

                return cp;
            }
            return 0;
        }
    }
}
