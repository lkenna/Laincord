using Laincord.Hoarder;
using DSharpPlus.Entities;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vanara.Extensions.Reflection;

namespace Laincord.Controls
{
    public class MessageParser : UserControl
    {
        public static Dictionary<string, BitmapSource> EmojiCache = new();
        private static readonly Regex MarkdownRegex = new(
            @"```(?:\w*\n)?([\s\S]*?)```|`([^`\n]+)`|\|\|(.+?)\|\||(\*\*)(.+?)\4|(__)(.+?)\6|(\*|_)(.+?)\8|~~(.+?)~~|(?m)^(?:\*|-)\s+(.+)|(?m)^>\s+(.+)|(?m)^(#{1,6})\s+(.+)",
            RegexOptions.Compiled);
        private static readonly Regex DiscordTokenRegex = new(
            @"(<a?:[^:]+:\d+>|<[@#][&!]?\d+>)",
            RegexOptions.Compiled);

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(DiscordMessage), typeof(MessageParser), new PropertyMetadata(null, OnMessageChanged));

        public DiscordMessage Message
        {
            get { return (DiscordMessage)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked;
        public event EventHandler<ContextMenuEventArgs> TextBlockContextMenuOpening;

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MessageParser)d;
            control.RenderMessage();
        }

        private void RenderMessage()
        {
            MainPanel.Children.Clear();
            if (Message == null)
            {
                return;
            }

#if FEATURE_SELECTABLE_MESSAGE_TEXT
            var textBlock = new SelectableTextBlock();
#else
            var textBlock = new TextBlock();
#endif

            //// Prevent the usual context menu from showing up:
            //textBlock.ContextMenu = null;

            //Throwing this in bcos i can :3 (messy nullcheck sawry :()
            if (Message.MentionedUsers != null)
            {
                if (Message.MentionedUsers.Contains(Discord.Client.CurrentUser) && Settings.SettingsManager.Instance.HighlightMentions)
                {
                    textBlock.Foreground = new SolidColorBrush(Color.FromRgb(73, 164, 218));
                }
            }
            // Ensure spaces around Discord tokens so "boss<:devious:123>" splits properly
            var normalizedContent = DiscordTokenRegex.Replace(Message.Content, " $1 ");
            var words = normalizedContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                string text = word;

                if (text.StartsWith("<") && text.EndsWith(">"))
                {
                    string id = text.Replace("<", "").Replace(">", "");
                    var link = new Hyperlink();
                    HyperlinkType? type = null;
                    object associatedObject = null;

                    // if there's no element at 0, continue
                    if (id.Length != 0)
                    {
                        switch (id.ElementAt(0))
                        {
                            case '@':
                                id = id.Replace("@", "");
                                if (id.Length == 0) break;
                                switch (id.ElementAt(0))
                                {
                                    case '&':
                                        {
                                            id = id.Replace("&", "");
                                            if (!ulong.TryParse(id, out ulong parsedId)) break;
                                            var role = Message.MentionedRoles?.FirstOrDefault(x => x?.Id == parsedId);
                                            // Fall back to guild role list
                                            if (role == null && Message?.Channel?.Guild?.Roles != null)
                                                Message.Channel.Guild.Roles.TryGetValue(parsedId, out role);
                                            if (role == null && Hoarder.Discord.Client?.Guilds != null)
                                                foreach (var g in Hoarder.Discord.Client.Guilds.Values)
                                                    if (g.Roles != null && g.Roles.TryGetValue(parsedId, out role))
                                                        break;
                                            if (role == null)
                                            {
                                                text = "@unknown-role";
                                                break;
                                            }
                                            link.Inlines.Add($"@{role.Name} ");
                                            type = HyperlinkType.Role;
                                            associatedObject = role;
                                            break;
                                        }
                                    default:
                                        {
                                            if (!ulong.TryParse(id, out ulong parsedId)) break;
                                            DiscordUser user = Message.MentionedUsers?.FirstOrDefault(x => x?.Id == parsedId);
                                            // Fall back to guild member list
                                            if (user == null && Message?.Channel?.Guild?.Members != null &&
                                                Message.Channel.Guild.Members.TryGetValue(parsedId, out var localMember))
                                                user = localMember;
                                            if (user == null && Hoarder.Discord.Client?.Guilds != null)
                                                foreach (var g in Hoarder.Discord.Client.Guilds.Values)
                                                    if (g.Members != null && g.Members.TryGetValue(parsedId, out var m))
                                                        { user = m; break; }
                                            if (user == null)
                                            {
                                                text = "@unknown-user";
                                                break;
                                            }
                                            link.Inlines.Add($"@{user.DisplayName} ");
                                            type = HyperlinkType.User;
                                            associatedObject = user;
                                            break;
                                        }
                                }
                                break;
                            case '#':
                                {
                                    id = id.Replace("#", "");
                                    if (!ulong.TryParse(id, out ulong parsedId)) break;
                                    var channel = Message.MentionedChannels?.FirstOrDefault(x => x?.Id == parsedId);
                                    if (channel == null)
                                    {
                                        text = "#unknown-channel";
                                        break;
                                    }
                                    link.Inlines.Add($"#{channel.Name} ");
                                    type = HyperlinkType.Channel;
                                    associatedObject = channel;
                                    break;
                                }
                            case 'a':
                            case ':':
                                {
                                    bool isAnimated = id.StartsWith("a:");
                                    string emojiStr = isAnimated ? id.Substring(1) : id;
                                    string[] emojiParts = emojiStr.Split(":");

                                    if (emojiParts.Length != 3)
                                    {
                                        break;
                                    }

                                    string emojiName = emojiParts[1];
                                    string emojiIdStr = emojiParts[2];

                                    if (!ulong.TryParse(emojiIdStr, out ulong emojiId))
                                    {
                                        break;
                                    }

                                    // Try current guild first, then search all guilds
                                    string emojiUrl = null;
                                    var localEmoji = Message?.Channel?.Guild?.Emojis?.FirstOrDefault(x => x.Key == emojiId);
                                    if (localEmoji?.Value?.Url != null)
                                    {
                                        emojiUrl = localEmoji.Value.Value.Url;
                                    }
                                    else if (Hoarder.Discord.Client?.Guilds != null)
                                    {
                                        foreach (var guild in Hoarder.Discord.Client.Guilds.Values)
                                        {
                                            if (guild.Emojis != null && guild.Emojis.TryGetValue(emojiId, out var found))
                                            {
                                                emojiUrl = found.Url;
                                                break;
                                            }
                                        }
                                    }
                                    // Fall back to CDN URL if not found in any guild cache
                                    string ext = isAnimated ? "gif" : "png";
                                    emojiUrl ??= $"https://cdn.discordapp.com/emojis/{emojiId}.{ext}";

                                    InlineUIContainer inlineContainer = new();
                                    Image emojiImage = new();
                                    emojiImage.Source = new BitmapImage(new Uri(emojiUrl));
                                    emojiImage.Width = 19;
                                    emojiImage.Height = 19;
                                    emojiImage.VerticalAlignment = VerticalAlignment.Center;
                                    emojiImage.ToolTip = $":{emojiName}:";
                                    inlineContainer.Child = emojiImage;

                                    textBlock.Inlines.Add(inlineContainer);
                                    textBlock.Inlines.Add(new Run(" "));
                                    textBlock.TextWrapping = TextWrapping.Wrap;

                                    // Make the below loop continue:
                                    type = HyperlinkType.ServerEmoji;

                                    break;
                                }
                        }

                        if (link.Inlines.Count > 0 && type != null)
                        {
                            link.Click += (s, e) => OnHyperlinkClicked(type.Value, associatedObject);
                            textBlock.Inlines.Add(link);
                            continue;
                        }
                        else if (type == HyperlinkType.ServerEmoji)
                        {
                            continue;
                        }
                    }
                }
                else if (text.StartsWith("http://") || text.StartsWith("https://") || text.StartsWith("ftp://") || text.StartsWith("gopher://"))
                {
                    // This is a link. Links cannot contain spaces, so we can easily just consider the
                    // whole part a link (in the case of standard links). Of course, we try to parse an
                    // actual URI here, and if we cannot deduce one, then we disregard the part.
                    if (Uri.IsWellFormedUriString(text, UriKind.Absolute))
                    {
                        Hyperlink link = new();
                        Uri uriSanitised = new(text);

                        link.Click += (s, e) => OnHyperlinkClicked(HyperlinkType.WebLink, uriSanitised.ToString());
                        link.Inlines.Add(uriSanitised.ToString());
                        textBlock.Inlines.Add(link);
                        textBlock.Inlines.Add(" ");
                        continue;
                    }
                }

                List<Inline> inlines = new();
                Run currentRun = new Run();

                if (ContainsEmoji(text)) // stops the iteration if the text can't possibly contain emoji
                {
                    DiscordEmoji? emoji = null;
                    StringInfo info = new(text);
                    int loopCount = info.LengthInTextElements;
                    for (int i = 0; i < loopCount; i++)
                    {
                        string c = info.SubstringByTextElements(i, 1);

                        if (text.StartsWith(":") && text.EndsWith(":"))
                        {
                            try
                            {
                                emoji = DiscordEmoji.FromName(Discord.Client, text);
                                loopCount = 1;
                            }
                            catch { }
                        }

                        else
                        {
                            DiscordEmoji.TryFromUnicode(c, out emoji);
                        }

                        if (emoji == null)
                        {
                            // If the character is likely an emoji (surrogate pair or symbol)
                            // but DSharpPlus doesn't recognize it, render as Twemoji image
                            if (c.Length > 1 || char.IsSurrogate(c[0]) || char.GetUnicodeCategory(c[0]) == System.Globalization.UnicodeCategory.OtherSymbol)
                            {
                                inlines.Add(currentRun);
                                currentRun = new Run();
                                var twemojiInline = CreateTwemojiInline(c);
                                if (twemojiInline != null)
                                    inlines.Add(twemojiInline);
                                else
                                    currentRun.Text += c;
                            }
                            else
                            {
                                currentRun.Text += c;
                            }
                            continue;
                        }

                        // emoji is not null; add the current run to the inlines list
                        inlines.Add(currentRun);
                        currentRun = new Run();
                        if (!EmojiDictionary.Map.TryGetValue(emoji.SearchName.Replace(":", ""), out var emojiName))
                            emojiName = null; // fallback

                        if (emojiName is null)
                        {
                            var twemojiInline = CreateTwemojiInline(emoji.Name);
                            if (twemojiInline != null)
                                inlines.Add(twemojiInline);
                            else
                                inlines.Add(new Run(emoji.Name));
                        }
                        else
                        {
                            InlineUIContainer inline = new();
                            Image image = new();
                            //image.Source = new BitmapImage(new Uri($"pack://application:,,,/Resources/Emoji/{emojiName}"));
                            // see if its in the cache
                            if (!EmojiCache.TryGetValue(emojiName, out BitmapSource? value))
                            {
                                value = new BitmapImage(new Uri($"pack://application:,,,/Resources/Emoji/{emojiName}"));
                                value.Freeze();
                                EmojiCache[emojiName] = value;
                            }
                            image.Source = value;
                            image.Width = 19;
                            image.Height = 19;
                            inline.Child = image;
                            inlines.Add(inline);
                        }
                    }
                }

                else
                {
                    currentRun.Text = text;
                }

                if (inlines.Count == 0) inlines.Add(currentRun);

                foreach (var inline in inlines)
                {
                    textBlock.Inlines.Add(inline);
                }
                // add a space
                textBlock.Inlines.Add(new Run(" "));
                textBlock.TextWrapping = TextWrapping.Wrap;
            }
            MainPanel.Children.Add(FormatFullText(textBlock));
        }

        public TextBlock FormatFullText(TextBlock sourceTextBlock)
        {
            var newTextBlock = new TextBlock
            {
                TextWrapping = sourceTextBlock.TextWrapping,

                Foreground = sourceTextBlock.Foreground,
                TextAlignment = sourceTextBlock.TextAlignment
            };

            var inlinesCopy = sourceTextBlock.Inlines.ToList();
            sourceTextBlock.Inlines.Clear();

            for (int i = 0; i < inlinesCopy.Count; i++)
            {
                var inline = inlinesCopy[i];

                if (inline is Run currentRun)
                {
                    var combinedText = new StringBuilder(currentRun.Text);

                    int nextIndex = i + 1;
                    while (nextIndex < inlinesCopy.Count && inlinesCopy[nextIndex] is Run nextRun)
                    {
                        combinedText.Append(nextRun.Text);
                        i = nextIndex;
                        nextIndex++;
                    }

                    var inlines = new List<Inline>();
                    int pos = 0;
                    string input = combinedText.ToString();

                    foreach (Match m in MarkdownRegex.Matches(input))
                    {
                        if (m.Index > pos)
                            inlines.Add(new Run(input.Substring(pos, m.Index - pos)));

                        if (m.Groups[1].Success) // code block
                        {
                            var run = new Run(m.Groups[1].Value.TrimEnd());
                            run.FontFamily = new FontFamily("Consolas");
                            run.Background = new SolidColorBrush(Color.FromRgb(242, 243, 245));
                            run.Foreground = Brushes.Black;
                            inlines.Add(new LineBreak());
                            inlines.Add(run);
                            inlines.Add(new LineBreak());
                        }
                        else if (m.Groups[2].Success) // inline code
                        {
                            var run = new Run(m.Groups[2].Value);
                            run.FontFamily = new FontFamily("Consolas");
                            run.Background = new SolidColorBrush(Color.FromRgb(242, 243, 245));
                            run.Foreground = Brushes.Black;
                            inlines.Add(run);
                        }
                        else if (m.Groups[3].Success) // spoiler
                        {
                            var spoilerRun = new Run(m.Groups[3].Value);
                            spoilerRun.Background = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                            spoilerRun.Foreground = new SolidColorBrush(Color.FromRgb(70, 70, 70));
                            spoilerRun.Cursor = Cursors.Hand;
                            spoilerRun.MouseLeftButtonDown += (s, e) =>
                            {
                                var r = (Run)s;
                                r.Foreground = Brushes.White;
                            };
                            inlines.Add(spoilerRun);
                        }
                        else if (m.Groups[4].Success) // bold
                        {
                            var run = new Run(m.Groups[5].Value);
                            run.FontWeight = FontWeights.Bold;
                            inlines.Add(run);
                        }
                        else if (m.Groups[6].Success) // underline
                        {
                            var run = new Run(m.Groups[7].Value);
                            run.TextDecorations = TextDecorations.Underline;
                            inlines.Add(run);
                        }
                        else if (m.Groups[8].Success) // italic
                        {
                            var run = new Run(m.Groups[9].Value);
                            run.FontStyle = FontStyles.Italic;
                            inlines.Add(run);
                        }
                        else if (m.Groups[10].Success) // strikethrough
                        {
                            var span = new Span(new Run(m.Groups[10].Value));
                            span.TextDecorations = TextDecorations.Strikethrough;
                            inlines.Add(span);
                        }
                        else if (m.Groups[11].Success) // list
                        {
                            var run = new Run(" " + m.Groups[11].Value);
                            inlines.Add(run);
                        }
                        else if (m.Groups[12].Success) // quote
                        {
                            var run = new Run("" + m.Groups[12].Value.Trim() + "");
                            run.FontStyle = FontStyles.Italic;
                            run.Foreground = Brushes.DimGray;
                            inlines.Add(run);
                        }
                        else if (m.Groups[13].Success) // header
                        {
                            var headerText = m.Groups[14].Value.Trim();
                            var run = new Run(headerText);
                            switch (m.Groups[13].Value.Length)
                            {
                                case 1: run.FontSize = 24; break;
                                case 2: run.FontSize = 20; break;
                                case 3: run.FontSize = 18; break;
                                default: run.FontSize = 16; break;
                            }
                            run.FontWeight = FontWeights.Bold;
                            inlines.Add(run);
                            inlines.Add(new LineBreak());
                        }
                        pos = m.Index + m.Length;
                    }

                    if (pos < input.Length)
                        inlines.Add(new Run(input.Substring(pos)));

                    foreach (Inline mdInline in inlines)
                        newTextBlock.Inlines.Add(mdInline);
                }
                else
                {
                    newTextBlock.Inlines.Add(inline);
                }
            }

            return newTextBlock;
        }

        private static InlineUIContainer? CreateTwemojiInline(string emojiText)
        {
            try
            {
                string url = Helpers.TwemojiHelper.GetUrl(emojiText);
                if (url == null) return null;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(url);
                bmp.CacheOption = BitmapCacheOption.OnDemand;
                bmp.EndInit();

                Image image = new();
                image.Source = bmp;
                image.Width = 19;
                image.Height = 19;
                image.VerticalAlignment = VerticalAlignment.Center;
                image.ToolTip = emojiText;
                return new InlineUIContainer(image);
            }
            catch
            {
                return null;
            }
        }

        private bool ContainsEmoji(string text)
        {
            if (text.Contains(':') && text.IndexOf(':') != text.LastIndexOf(':'))
                return true;

            foreach (char c in text)
            {
                if (char.IsSurrogate(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.OtherSymbol)
                    return true;
            }

            return false;
        }

        private void TextBlock_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            TextBlockContextMenuOpening?.Invoke(this, e);
        }

        private void OnHyperlinkClicked(HyperlinkType type, object associatedObject)
        {
            HyperlinkClicked?.Invoke(this, new HyperlinkClickedEventArgs(type, associatedObject));
        }

        public WrapPanel MainPanel { get; set; }

        public MessageParser()
        {
            MainPanel = new WrapPanel();
            Content = MainPanel;
            Loaded += (_, _) =>
            {
                var window = Window.GetWindow(this);
                if (window != null) window.Closing += (s, e) => { };
            };
        }
    }

    public enum HyperlinkType
    {
        Channel,
        Role,
        User,
        WebLink,
        ServerEmoji, // Internal parsing purposes only.
    }

    public class HyperlinkClickedEventArgs : EventArgs
    {
        public HyperlinkType Type { get; }
        public object AssociatedObject { get; }

        public HyperlinkClickedEventArgs(HyperlinkType type, object associatedObject)
        {
            Type = type;
            AssociatedObject = associatedObject;
        }
    }
}
