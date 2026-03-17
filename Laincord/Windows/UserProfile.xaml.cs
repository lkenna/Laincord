using DSharpPlus.Entities;
using Laincord.Controls;
using Laincord.Hoarder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Laincord.Windows
{
    public class RoleDisplayItem
    {
        public string Name { get; set; }
        public System.Windows.Media.Color Color { get; set; }
    }

    public partial class UserProfile : Window
    {
        private readonly ulong _userId;

        public UserProfile(DiscordUser user, DiscordMember member = null)
        {
            InitializeComponent();

            _userId = user.Id;

            // Display name and username
            string displayName = member?.DisplayName ?? user.DisplayName ?? user.Username;
            PART_DisplayName.Text = displayName;
            PART_Username.Text = $"@{user.Username}";

            // Profile picture
            string avatarUrl = member?.GuildAvatarUrl ?? user.AvatarUrl;
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(avatarUrl));
                    PART_ProfilePicture.ProfilePicture = bitmap;
                }
                catch { }
            }

            // Status
            var presence = user.Presence;
            if (presence != null)
            {
                string statusText = presence.Status switch
                {
                    UserStatus.Online => "Online",
                    UserStatus.Idle => "Idle",
                    UserStatus.DoNotDisturb => "Do Not Disturb",
                    UserStatus.Invisible => "Invisible",
                    _ => "Offline"
                };
                PART_Status.Text = statusText;
                PART_ProfilePicture.UserStatus = presence.Status;

                // Activity
                var activity = presence.Activities?.FirstOrDefault();
                if (activity != null)
                {
                    string activityText = activity.ActivityType switch
                    {
                        ActivityType.Playing => $"Playing {activity.Name}",
                        ActivityType.Streaming => $"Streaming {activity.Name}",
                        ActivityType.ListeningTo => $"Listening to {activity.Name}",
                        ActivityType.Watching => $"Watching {activity.Name}",
                        ActivityType.Competing => $"Competing in {activity.Name}",
                        ActivityType.Custom => activity.CustomStatus?.Name ?? "",
                        _ => ""
                    };
                    PART_Activity.Text = activityText;
                    if (string.IsNullOrEmpty(activityText))
                        PART_Activity.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PART_Activity.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PART_Status.Text = "Offline";
                PART_ProfilePicture.UserStatus = UserStatus.Offline;
                PART_Activity.Visibility = Visibility.Collapsed;
            }

            // Account info
            PART_CreatedAt.Text = $"Created: {user.CreationTimestamp:MMMM d, yyyy}";
            PART_UserId.Text = $"ID: {user.Id}";

            if (member?.JoinedAt != default && member?.JoinedAt != null)
            {
                PART_JoinedAt.Text = $"Joined: {member.JoinedAt:MMMM d, yyyy}";
            }
            else
            {
                PART_JoinedAt.Visibility = Visibility.Collapsed;
            }

            // Roles
            if (member != null)
            {
                var roles = member.Roles
                    .Where(r => r.Id != member.Guild.Id) // skip @everyone
                    .OrderByDescending(r => r.Position)
                    .Select(r => new RoleDisplayItem
                    {
                        Name = r.Name,
                        Color = r.Color.Value != 0
                            ? System.Windows.Media.Color.FromRgb(
                                (byte)((r.Color.Value >> 16) & 0xFF),
                                (byte)((r.Color.Value >> 8) & 0xFF),
                                (byte)(r.Color.Value & 0xFF))
                            : System.Windows.Media.Color.FromRgb(0x99, 0x99, 0x99)
                    })
                    .ToList();

                if (roles.Any())
                {
                    PART_RolesHeader.Visibility = Visibility.Visible;
                    PART_Roles.Visibility = Visibility.Visible;
                    PART_Roles.ItemsSource = roles;
                }
            }

            Title = $"{displayName}'s Profile";
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var dmChannel = Discord.Client.PrivateChannels.Values
                .FirstOrDefault(dm => dm.Recipients?.Count == 1 && dm.Recipients[0].Id == _userId);

            ulong chatId;
            if (dmChannel != null)
            {
                chatId = dmChannel.Id;
            }
            else
            {
                try
                {
                    var newDm = await Discord.Client.CreateDmChannelAsync(_userId);
                    chatId = newDm.Id;
                }
                catch { return; }
            }

            Chat? chat = Application.Current.Windows.OfType<Chat>().FirstOrDefault(x =>
                x?.ViewModel?.Recipient?.Id == _userId ||
                x?.Channel?.Id == chatId);
            if (chat is null)
            {
                chat = new Chat(chatId);
                chat.Show();
            }
            else
            {
                chat.Activate();
            }

            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
