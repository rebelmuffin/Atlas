using System.Text.Json;
using System.Globalization;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

using Atlas.Data;

namespace Atlas.Services
{
    public partial class JsonDataService : IDataService
    {
        public string DataPath { get; init; }
        
        private readonly string _guildPath = "guilds";

        private IServiceProvider Services { get; init; }
        private IDiscordClient Client { get; init; }

        private static JsonSerializerOptions SerializerOptions
        {
            get
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IncludeFields = true
                };

                return options;
            }
        }

        public JsonDataService(IServiceProvider services)
        {
            Services = services;
            Client = Services.GetRequiredService<DiscordSocketClient>();

            var dataEnv = Environment.GetEnvironmentVariable("DATA_PATH");
            if (dataEnv is null)
            {
                DataPath = "Data";
                return;
            }

            DataPath = dataEnv;

            // Create directory if doesn't exist
            if (!Directory.Exists(DataPath))
            {
                Directory.CreateDirectory(DataPath);
            }
        }

        public string EnsureFilePath(params string?[] paths)
        {
            string path = Path.Join(DataPath, Path.Join(paths));
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                string directory = Path.GetDirectoryName(path)!;

                Directory.CreateDirectory(directory);
            }

            return path;
        }

        public string EnsureDirectoryPath(params string?[] paths)
        {
            string path = Path.Join(DataPath, Path.Join(paths));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// Deserialises data from given path
        /// </summary>
        /// <param name="filename">Path to data file</param>
        /// <typeparam name="T">Data type implementing <see cref="IAtlasData"/></typeparam>
        private async Task<T?> DeserialiseData<T>(string filename) where T : IAtlasData
        {
            T? data = default;

            FileStream jsonStream = File.Open(filename, FileMode.Open);
            if (jsonStream.CanRead)
            {
                try
                {
                    var deserialisedData = await JsonSerializer.DeserializeAsync<T>(jsonStream, SerializerOptions);
                    if (deserialisedData?.IsValid() ?? false)
                    {
                        await deserialisedData.InitialiseAsync(Client);
                        data = deserialisedData;
                    }
                }
                catch (JsonException) // Malformed JSON
                { }
            }
            jsonStream.Close();

            return data;
        }

        /// <summary>
        /// Serialises the given data into given path
        /// </summary>
        /// <typeparam name="T">Data type implementing <see cref="IAtlasData"/></typeparam>
        private async Task SerialiseData<T>(T data, string filename) where T : IAtlasData
        {
            FileStream jsonStream = File.Open(filename, FileMode.Create);
            if (jsonStream.CanWrite)
            {
                await JsonSerializer.SerializeAsync(jsonStream, data, SerializerOptions);
            }
            jsonStream.Close();
        }

        #region UserProfile

        private string GetUserProfilePath(ulong? guildId, ulong userId)
        {
            var filename = userId.ToString() + ".json";

            if ((guildId ?? 0) == 0) // No guild
            {
                return EnsureFilePath("users", filename);
            }

            return EnsureFilePath(_guildPath, guildId.ToString(), "members", filename);
        }

        private async Task<UserProfile> CreateUserProfileAsync(IUser user, IGuild? guild)
        {
            // Create user
            UserProfile profile = new(user, guild);
            await profile.InitialiseAsync(Client);

            // Save to disk
            await SaveUserProfileAsync(profile);

            return profile;
        }

        private async Task<UserProfile?> ReadUserProfile(string filename)
        {
            if (!Path.Exists(filename))
            {
                return null;
            }

            UserProfile? profile = await DeserialiseData<UserProfile>(filename);

            return profile;
        }

        public async Task<UserProfile> GetUserProfileAsync(IUser user, IGuild? guild)
        {
            string filename = GetUserProfilePath(guild?.Id, user.Id);
            
            // Not very comfortable with direct dependencies to UserProfile but fine for now
            UserProfile? profile = await ReadUserProfile(filename);

            if (profile is null)
            {
                return await CreateUserProfileAsync(user, guild);
            }

            return profile;
        }

        public async Task SaveUserProfileAsync(UserProfile profile)
        {
            var filename = GetUserProfilePath(profile.GuildId, profile.UserId);

            await SerialiseData(profile, filename);
        }

        #endregion UserProfile
        #region LevelTier

        private string GetTierDirectory(ulong guildId)
        {
            return EnsureDirectoryPath(_guildPath, guildId.ToString(), "ranks");
        }

        private uint GenerateTierId(ulong guildId)
        {
            var tiersDir = GetTierDirectory(guildId);
            var dataFiles = Directory.GetFiles(tiersDir);

            // Very crude, but works
            uint highestId = 0;
            foreach (var file in dataFiles)
            {
                var parseResult = uint.TryParse(Path.GetFileNameWithoutExtension(file), out uint id);
                if (parseResult == false)
                {
                    continue;
                }

                highestId = uint.Max(id, highestId);
            }

            return highestId;
        }

        private string GetTierPath(ulong guildId, uint tierId)
        {
            var tiersDir = GetTierDirectory(guildId);
            var filename = tierId.ToString() + ".json";
            return Path.Join(tiersDir, filename);
        }

        private async Task<LevelTier?> ReadLevelTierAsync(string filename)
        {
            if (!Path.Exists(filename))
            {
                return null;
            }

            return await DeserialiseData<LevelTier>(filename);
        }

        public async Task<LevelTier> CreateLevelTierAsync(IGuild guild, IRole? rewardRole)
        {
            // Create tier
            var tierId = GenerateTierId(guild.Id);
            LevelTier tier = new(guild, rewardRole, tierId);
            await tier.InitialiseAsync(Client);

            // Save to disk
            await SaveLevelTierAsync(tier);

            return tier;
        }

        public async Task<LevelTier?> GetLevelTierAsync(IGuild guild, uint tierId)
        {
            string filename = GetTierPath(guild.Id, tierId);

            return await ReadLevelTierAsync(filename);
        }

        public async Task<IEnumerable<LevelTier>> GetLevelTiersAsync(IGuild guild)
        {
            List<LevelTier> tiers = new();
            var tiersDir = GetTierDirectory(guild.Id);

            foreach (var filename in Directory.GetFiles(tiersDir))
            {
                LevelTier? tier = await ReadLevelTierAsync(filename);
                if (tier is null)
                {
                    continue;
                }

                tiers.Add(tier);
            }

            return tiers;
        }

        public async Task SaveLevelTierAsync(LevelTier tier)
        {
            var filename = GetTierPath(tier.GuildId, tier.Id);

            await SerialiseData(tier, filename);
        }

        #endregion LevelTier
        #region GuildConfig

        private string GetGuildConfigPath(ulong guildId)
        {
            var filename = "config.json";
            return EnsureFilePath(_guildPath, guildId.ToString(), filename);
        }

        private async Task<GuildConfig?> ReadGuildConfigAsync(string filename)
        {
            if (!Path.Exists(filename))
            {
                return null;
            }

            return await DeserialiseData<GuildConfig>(filename);
        }

        private async Task<GuildConfig> CreateGuildConfigAsync(IGuild guild)
        {
            // Create config
            GuildConfig config = new(guild);
            await config.InitialiseAsync(Client);

            // Save to disk
            await SaveGuildConfigAsync(config);

            return config;
        }

        public async Task<GuildConfig> GetGuildConfigAsync(IGuild guild)
        {
            string filename = GetGuildConfigPath(guild.Id);

            GuildConfig? config = await ReadGuildConfigAsync(filename);

            if (config is null)
            {
                return await CreateGuildConfigAsync(guild);
            }

            return config;
        }

        public async Task SaveGuildConfigAsync(GuildConfig config)
        {
            var filename = GetGuildConfigPath(config.GuildId);

            await SerialiseData(config, filename);
        }

        #endregion GuildConfig
    }
}