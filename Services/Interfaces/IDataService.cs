using Discord;
using Atlas.Data;

namespace Atlas.Services
{
    /// <summary>
    /// Service responsible for loading/saving all data classes.
    /// Need to find a better way to do this since this results in a giga-class doing all serialisation.
    /// </summary>
    public interface IDataService
    {
        public Task<UserProfile> GetUserProfileAsync(IUser user, IGuild? guild);
        public Task SaveUserProfileAsync(UserProfile profile);

        public Task<LevelTier?> GetLevelTierAsync(IGuild guild, uint tierId);
        public Task<LevelTier> CreateLevelTierAsync(IGuild guild, IRole? role);
        public Task<IEnumerable<LevelTier>> GetLevelTiersAsync(IGuild guild);
        public Task SaveLevelTierAsync(LevelTier tier);
    
        public Task<GuildConfig> GetGuildConfigAsync(IGuild guild);
        public Task SaveGuildConfigAsync(GuildConfig config);
    }
}