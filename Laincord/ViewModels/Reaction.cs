using DSharpPlus.Entities;
using Laincord.Hoarder;
using Laincord.Helpers;
using System.Windows.Input;
using System.Windows.Media;

namespace Laincord.ViewModels
{
    public class ReactionViewModel : ViewModelBase
    {
        private int _count;
        private bool _isMe;
        private string _emojiDisplay;
        private string _imageUrl;
        private DiscordEmoji _emoji;
        private DiscordMessage _message;

        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public bool IsMe
        {
            get => _isMe;
            set
            {
                SetProperty(ref _isMe, value);
                OnPropertyChanged(nameof(Background));
                OnPropertyChanged(nameof(BorderColor));
            }
        }

        public string EmojiDisplay
        {
            get => _emojiDisplay;
            set => SetProperty(ref _emojiDisplay, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public DiscordEmoji Emoji => _emoji;
        public DiscordMessage Message => _message;

        public Brush Background => IsMe
            ? new SolidColorBrush(Color.FromArgb(40, 59, 130, 246))
            : new SolidColorBrush(Color.FromRgb(242, 243, 245));

        public Brush BorderColor => IsMe
            ? new SolidColorBrush(Color.FromRgb(59, 130, 246))
            : new SolidColorBrush(Color.FromRgb(222, 222, 222));

        public static ReactionViewModel FromReaction(DiscordReaction reaction, DiscordMessage message)
        {
            var vm = new ReactionViewModel
            {
                Count = reaction.Count,
                IsMe = reaction.IsMe,
                _emoji = reaction.Emoji,
                _message = message,
            };

            if (reaction.Emoji.Id != 0)
            {
                // Custom guild emoji
                vm.ImageUrl = reaction.Emoji.Url;
                vm.EmojiDisplay = null;
            }
            else
            {
                // Unicode emoji — try Twemoji
                string url = TwemojiHelper.GetUrl(reaction.Emoji.Name);
                if (url != null)
                {
                    vm.ImageUrl = url;
                    vm.EmojiDisplay = null;
                }
                else
                {
                    vm.EmojiDisplay = reaction.Emoji.Name;
                    vm.ImageUrl = null;
                }
            }

            return vm;
        }

        public static ReactionViewModel FromDiscordEmoji(DiscordEmoji emoji, int count, bool isMe, DiscordMessage message)
        {
            var vm = new ReactionViewModel
            {
                Count = count,
                IsMe = isMe,
                _emoji = emoji,
                _message = message,
            };

            if (emoji.Id != 0)
            {
                vm.ImageUrl = emoji.Url;
            }
            else
            {
                string url = TwemojiHelper.GetUrl(emoji.Name);
                if (url != null)
                    vm.ImageUrl = url;
                else
                    vm.EmojiDisplay = emoji.Name;
            }

            return vm;
        }

        public async void ToggleReaction()
        {
            try
            {
                // Don't update count/IsMe here — the gateway event will handle it
                if (IsMe)
                    await _message.DeleteOwnReactionAsync(_emoji);
                else
                    await _message.CreateReactionAsync(_emoji);
            }
            catch { }
        }
    }
}
