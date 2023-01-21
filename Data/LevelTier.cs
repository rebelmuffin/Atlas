using Discord;

using System.Text.Json.Serialization;

namespace Atlas.Data
{
    /// <summary>
    /// The data class that holds information about a level tier(or rank). This is used by ranking system to assign levelled roles.
    /// </summary>
    [Serializable]
    public class LevelTier : IAtlasData
    {
        [JsonIgnore]
        public IRole? RewardRole = null;
        [JsonIgnore]
        public IGuild? Guild = null;

        public ulong RewardRoleId { get; set; }
        public ulong GuildId { get; set; }
        public int MinLevel { get; set; }
        public uint Id { get; init; }

        public LevelTier()
        {
        }

        public LevelTier(IGuild? guild, IRole? rewardRole, uint tierId)
        {
            GuildId = guild?.Id ?? 0;
            RewardRoleId = rewardRole?.Id ?? 0;
            Id = tierId;
        }

        public bool IsValid()
        {
            return GuildId != 0;
        }

        public async Task InitialiseAsync(IDiscordClient client)
        {
            Guild = await client.GetGuildAsync(GuildId);

            if (RewardRoleId != 0)
            {
                RewardRole = Guild.GetRole(RewardRoleId);
            }
        }

        public override string ToString()
        {
            var text = $"Tier: {Id}, MinLevel: {MinLevel}";
            if (RewardRole is not null)
            {
                text += $", Role: {RewardRole.Mention}";
            }

            return text;
        }
    }
}