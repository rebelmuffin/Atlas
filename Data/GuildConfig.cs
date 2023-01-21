using System.Text.Json.Serialization;

using Discord;

namespace Atlas.Data
{
    /// <summary>
    /// Holds all bot related configuration data bound to a Guild.
    /// </summary>
    [Serializable]
    public class GuildConfig : IAtlasData
    {
        [JsonIgnore]
        public IGuild? Guild { get; set; }

        public ulong GuildId { get; set; }

        public GuildConfig(IGuild guild)
        {
            GuildId = guild.Id;
        }

        public bool IsValid()
        {
            return GuildId != 0;
        }

        public async Task InitialiseAsync(IDiscordClient client)
        {
            Guild = await client.GetGuildAsync(GuildId);
        }

        public override string ToString()
        {
            return $"GuildConfig: {Guild?.Name}";
        }
    }
}