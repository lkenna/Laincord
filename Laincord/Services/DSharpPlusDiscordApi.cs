using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Laincord.Services
{
    public interface IDiscordApi
    {
        Task<DiscordMessage> SendMessageAsync(ulong channelId, DiscordMessageBuilder builder);
        Task TriggerTypingAsync(ulong channelId);
        Task DeleteMessageAsync(ulong channelId, ulong messageId, DiscordMessage discordMessage);
        Task<DiscordChannel> GetChannelAsync(ulong channelId);
        Task<DiscordGuild> GetGuildAsync(ulong guildId);
    }

    public class DiscordUnauthorizedException : Exception
    {
        public DiscordUnauthorizedException(string? message = null) : base(message) { }
    }

    public sealed class DSharpPlusDiscordApi : IDiscordApi
    {
        private readonly DiscordClient _client;

        public DSharpPlusDiscordApi(DiscordClient client) => _client = client;

        private async Task<DiscordChannel> GetCachedOrFetchChannelAsync(ulong channelId)
        {
            if (_client.TryGetCachedChannel(channelId, out var channel))
                return channel;
            return await _client.GetChannelAsync(channelId).ConfigureAwait(false);
        }

        public async Task<DiscordMessage> SendMessageAsync(ulong channelId, DiscordMessageBuilder builder)
        {
            try
            {
                var channel = await GetCachedOrFetchChannelAsync(channelId).ConfigureAwait(false);
                return await channel.SendMessageAsync(builder).ConfigureAwait(false);
            }
            catch (UnauthorizedException ex)
            {
                throw new DiscordUnauthorizedException(ex.Message);
            }
        }

        public async Task TriggerTypingAsync(ulong channelId)
        {
            try
            {
                var channel = await GetCachedOrFetchChannelAsync(channelId).ConfigureAwait(false);
                await channel.TriggerTypingAsync().ConfigureAwait(false);
            }
            catch (UnauthorizedException ex)
            {
                throw new DiscordUnauthorizedException(ex.Message);
            }
        }

        public async Task DeleteMessageAsync(ulong channelId, ulong messageId, DiscordMessage discordMessage)
        {
            try
            {
                var channel = await GetCachedOrFetchChannelAsync(channelId).ConfigureAwait(false);
                await channel.DeleteMessageAsync(discordMessage).ConfigureAwait(false);
            }
            catch (UnauthorizedException ex)
            {
                throw new DiscordUnauthorizedException(ex.Message);
            }
        }

        public Task<DiscordChannel> GetChannelAsync(ulong channelId)
            => GetCachedOrFetchChannelAsync(channelId);

        public Task<DiscordGuild> GetGuildAsync(ulong guildId)
            => _client.GetGuildAsync(guildId);
    }
}
