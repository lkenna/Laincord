using Laincord.Hoarder;
using System.IO;
using System.Text.Json;

namespace Laincord.Helpers
{
    public static class FrequentEmojiTracker
    {
        private static readonly string FilePath;
        private static Dictionary<string, int> _counts;
        private static bool _discordLoaded;
        private const int MaxDisplay = 36;

        static FrequentEmojiTracker()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Laincord");
            Directory.CreateDirectory(dir);
            FilePath = Path.Combine(dir, "frequent_emoji.json");
            Load();
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    _counts = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new();
                    return;
                }
            }
            catch { }
            _counts = new();
        }

        private static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_counts);
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }

        public static void Track(string emojiName)
        {
            if (string.IsNullOrEmpty(emojiName)) return;
            _counts.TryGetValue(emojiName, out int count);
            _counts[emojiName] = count + 1;
            Save();
        }

        public static async Task LoadFromDiscordAsync()
        {
            if (_discordLoaded || Discord.Client == null) return;
            _discordLoaded = true;
            try
            {
                string base64 = await Discord.Client.GetFrecencySettingsProto();
                if (string.IsNullOrEmpty(base64)) return;

                byte[] data = Convert.FromBase64String(base64);
                var parsed = ParseFrecencyEmoji(data);

                bool changed = false;
                foreach (var (name, uses) in parsed)
                {
                    if (!_counts.ContainsKey(name))
                    {
                        _counts[name] = uses;
                        changed = true;
                    }
                }
                if (changed) Save();
            }
            catch { }
        }

        private static List<(string Name, int Uses)> ParseFrecencyEmoji(byte[] data)
        {
            var results = new List<(string, int)>();
            try
            {
                // Field 6 = EmojiFrecency (map<string, FrecencyItem>)
                int pos = 0;
                while (pos < data.Length)
                {
                    int fieldTag = ReadVarint(data, ref pos);
                    int fieldNumber = fieldTag >> 3;
                    int wireType = fieldTag & 0x7;

                    if (fieldNumber == 6 && wireType == 2)
                    {
                        int len = ReadVarint(data, ref pos);
                        int end = pos + len;
                        ParseEmojiMap(data, pos, end, results);
                        pos = end;
                    }
                    else
                    {
                        SkipField(data, ref pos, wireType);
                    }
                }
            }
            catch { }
            return results;
        }

        private static void ParseEmojiMap(byte[] data, int start, int end, List<(string, int)> results)
        {
            // map<string, FrecencyItem> encodes as repeated field 1 sub-messages
            int pos = start;
            while (pos < end)
            {
                int fieldTag = ReadVarint(data, ref pos);
                int fieldNumber = fieldTag >> 3;
                int wireType = fieldTag & 0x7;

                if (fieldNumber == 1 && wireType == 2)
                {
                    int len = ReadVarint(data, ref pos);
                    int entryEnd = pos + len;
                    var entry = ParseMapEntry(data, pos, entryEnd);
                    if (entry.Name != null && entry.Uses > 0)
                        results.Add(entry);
                    pos = entryEnd;
                }
                else
                {
                    SkipField(data, ref pos, wireType);
                }
            }
        }

        private static (string Name, int Uses) ParseMapEntry(byte[] data, int start, int end)
        {
            // Map entry: field 1 = key (string emoji name), field 2 = value (FrecencyItem)
            string name = null;
            int totalUses = 0;
            int pos = start;

            while (pos < end)
            {
                int fieldTag = ReadVarint(data, ref pos);
                int fieldNumber = fieldTag >> 3;
                int wireType = fieldTag & 0x7;

                switch (fieldNumber)
                {
                    case 1 when wireType == 2:
                        int klen = ReadVarint(data, ref pos);
                        name = System.Text.Encoding.UTF8.GetString(data, pos, klen);
                        pos += klen;
                        break;
                    case 2 when wireType == 2:
                        int slen = ReadVarint(data, ref pos);
                        int statsEnd = pos + slen;
                        totalUses = ParseFrecencyItem(data, pos, statsEnd);
                        pos = statsEnd;
                        break;
                    default:
                        SkipField(data, ref pos, wireType);
                        break;
                }
            }

            if (string.IsNullOrEmpty(name) || totalUses <= 0) return (null, 0);
            return (name, totalUses);
        }

        private static int ParseFrecencyItem(byte[] data, int start, int end)
        {
            // FrecencyItem: field 1 = total_uses (uint32)
            int pos = start;
            int totalUses = 0;
            while (pos < end)
            {
                int fieldTag = ReadVarint(data, ref pos);
                int fieldNumber = fieldTag >> 3;
                int wireType = fieldTag & 0x7;

                if (fieldNumber == 1 && wireType == 0)
                    totalUses = ReadVarint(data, ref pos);
                else
                    SkipField(data, ref pos, wireType);
            }
            return totalUses;
        }

        private static int ReadVarint(byte[] data, ref int pos)
        {
            int result = 0;
            int shift = 0;
            while (pos < data.Length)
            {
                byte b = data[pos++];
                result |= (b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }

        private static void SkipField(byte[] data, ref int pos, int wireType)
        {
            switch (wireType)
            {
                case 0: ReadVarint(data, ref pos); break;
                case 1: pos += 8; break;
                case 2: int len = ReadVarint(data, ref pos); pos += len; break;
                case 5: pos += 4; break;
            }
        }

        public static List<string> GetFrequent()
        {
            return _counts
                .OrderByDescending(kv => kv.Value)
                .Take(MaxDisplay)
                .Select(kv => kv.Key)
                .ToList();
        }

        public static bool HasAny() => _counts.Count > 0;
    }
}
