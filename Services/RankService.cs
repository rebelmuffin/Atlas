using Discord;
using Discord.WebSocket;

using Atlas.Data;
using Atlas.Data.Structures;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Services
{
    public class RankService
    {
        private IDataService Data { get; init; }
        private DiscordSocketClient Client { get; init; }

        private readonly IDictionary<ulong, DateTime> LastUpdates = new Dictionary<ulong, DateTime>();

        public RankService(IServiceProvider services)
        {
            Data = services.GetRequiredService<IDataService>();
            Client = services.GetRequiredService<DiscordSocketClient>();
        }

        /// <summary>
        /// Checks is a user profile is eligible and if so, levels it up.
        /// </summary>
        public static bool CheckLevelUp(UserProfile profile)
        {
            if (!profile.Level.ShouldLevelUp)
            {
                return false;
            }

            profile.LevelUp();
            return true;
        }

        /// <summary>
        /// Checks is a user profile is eligible and if so, levels it up.
        /// In addition to levelling up the profile, this also sends a notification message in the channel.
        /// </summary>
        /// <param name="profile">Profile of user to check level up for</param>
        /// <param name="message">The message that triggered this check</param>
        public static async Task CheckLevelUpAsync(UserProfile profile, SocketMessage message)
        {
            if (!CheckLevelUp(profile))
            {
                return;
            }

            // Notify user if possible
            var channel = message.Channel;
            try
            {
                // #TODO: Make the text data-driven
                var text = $"Congratulations {profile.User?.Mention}! You have reached level `{profile.Level.Level}`!";
                await channel.SendMessageAsync(text);
            }
            catch
            {
            }
        }

        public Task InitialiseAsync()
        {
            Client.MessageReceived += HandleMessage;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if the user is on cooldown for experience increment.
        /// </summary>
        public bool CanIncrementExperience(IUser user)
        {
            // #TODO: Make the interval data-driven
            const int intervalSecs = 60;

            bool exists = LastUpdates.TryGetValue(user.Id, out DateTime lastUpdate);
            if (!exists)
            {
                LastUpdates[user.Id] = DateTime.UtcNow;
                return true;
            }

            var span = DateTime.UtcNow - lastUpdate;
            if (span.TotalSeconds >= intervalSecs)
            {
                LastUpdates[user.Id] = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks all ranks in the guild and adds missing roles if necessary.
        /// </summary>
        public async Task CheckNewRanks(UserProfile profile)
        {
            IEnumerable<LevelTier> ranks = await Data.GetLevelTiersAsync(profile.Guild!);
            IGuildUser? member = await profile.Guild!.GetUserAsync(profile.UserId);
            if (member is null)
            {
                return;
            }

            List<ulong> roles = new();
            foreach (var rank in ranks)
            {
                if (rank.MinLevel <= profile.Level.Level && rank.RewardRole is not null)
                {
                    roles.Add(rank.RewardRole.Id);
                }
            }

            if (roles.Any())
            {
                await member.AddRolesAsync(roles);
            }
        }

        /// <summary>
        /// Message handler event. Filter out the messages and level up/rank up corresponding user profiles.
        /// </summary>
        public async Task HandleMessage(SocketMessage message)
        {
            // Only user messages
            if (message is not SocketUserMessage userMessage || userMessage.Author.IsBot)
            {
                return;
            }

            // Only guild messages
            if (message.Channel is not SocketGuildChannel guildChannel)
            {
                return;
            }

            // Only if interval allows
            if (!CanIncrementExperience(userMessage.Author))
            {
                return;
            }

            // Add Experience and check level up
            var profile = await Data.GetUserProfileAsync(userMessage.Author, guildChannel.Guild);
            profile.AddExp();
            await CheckLevelUpAsync(profile, userMessage);

            // Save data
            await Data.SaveUserProfileAsync(profile);

            // Manage roles
            await CheckNewRanks(profile);

            Console.WriteLine($"Exp Increment: {profile.User}");
        }
    }
}