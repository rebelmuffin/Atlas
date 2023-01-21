using Discord;
using Discord.Interactions;
using Discord.WebSocket;

using Atlas.Services;

namespace Atlas.Modules
{
    /// <summary>
    /// Module responsible for creation, deletion, and modification of user defined ranks
    /// </summary>
    public class RankModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Interactions { get; set; } = default!;
        public DiscordSocketClient Client { get; set; } = default!;
        public IDataService Data { get; set; } = default!;

        [SlashCommand("makerank", "Creates a rank with a role that is achieved at a certain level.")]
        public async Task MakeRank(
            [Summary(description: "Target level that the user has to reach to achieve this rank")]
            int targetLevel,
            [Summary(description: "Role that is to be given to person who achieves this rank")]
            IRole? role)
        {
            // Create & save new tier
            var tier = await Data.CreateLevelTierAsync(Context.Guild, role);
            tier.MinLevel = targetLevel;
            await Data.SaveLevelTierAsync(tier);

            // set allowed mentions
            var mentions = new AllowedMentions
            {
                AllowedTypes = AllowedMentionTypes.Users
            };

            await RespondAsync($"Created {tier}", allowedMentions: mentions);
        }
    }
}