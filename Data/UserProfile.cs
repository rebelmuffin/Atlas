using Discord;

using System.Text.Json.Serialization;

using Atlas.Data.Structures;
using Atlas.Services;

namespace Atlas.Data
{
    /// <summary>
    /// The data class that holds information about a discord user. Holds all data structures related to a single user/member
    /// </summary>
    [Serializable]
    public class UserProfile : IAtlasData
    {
        [JsonIgnore]
        public IUser? User = null;
        [JsonIgnore]
        public IGuild? Guild = null;

        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public LevelInfo Level;

        public UserProfile()
        {
        }

        public UserProfile(IUser? user = null, IGuild? guild = null)
        {
            UserId = user?.Id ?? 0;
            GuildId = guild?.Id ?? 0;
        }

        public bool IsValid()
        {
            return UserId != 0;
        }

        public async Task InitialiseAsync(IDiscordClient client)
        {
            User = await client.GetUserAsync(UserId);

            if (GuildId != 0)
            {
                Guild = await client.GetGuildAsync(GuildId);
            }
        }

        public override string ToString()
        {
            var userText = Guild is null ? "User" : "Member";
            return $"{userText}: {User?.ToString()}, LevelInfo: {Level}";
        }

        /// <summary>
        /// Adds the default amount of experience to the profile dictated by <see cref="LevelInfo.GetExperienceIncrementAmount"/>
        /// </summary>
        public void AddExp()
        {
            Level.Experience += LevelInfo.GetExperienceIncrementAmount();
        }

        /// <summary>
        /// Adds the given amount of experience to the user profile
        /// </summary>
        public void AddExp(ulong exp)
        {
            Level.Experience += exp;
        }

        /// <summary>
        /// Deducts the given amount of experience from the user profile
        /// </summary>
        public void DeductExp(ulong exp)
        {
            if (Level.Experience > exp)
            {
                Level.Experience -= exp;
            }
            else
            {
                Level.Experience = 0;
            }
        }


        /// <summary>
        /// Levels up the user profile. Not a particularly important implementation but good to keep centralised.
        /// </summary>
        internal void LevelUp()
        {
            Level.Level++;
        }
    }
}