using System.Text.Json.Serialization;

using Discord;

using Atlas.Data.Structures;

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

        public GuildAdminData Admin = new();

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

        /// <summary>
        /// Checks if given guild member is an administrator
        /// </summary>
        public bool IsUserAdmin(IGuildUser user)
        {
            if (Admin.AdministratorUsers.Contains(user.Id))
            {
                return true;
            }

            // Check admin roles
            return Admin.AdministratorRoles.Intersect(user.RoleIds).Any();
        }

        /// <summary>
        /// Checks if given guild member is a moderator. Also returns true if user is admin.
        /// </summary>
        public bool IsUserModerator(IGuildUser user)
        {
            // If user is admin, no need to check for mod
            if (IsUserAdmin(user))
            {
                return true;
            }

            if (Admin.ModeratorUsers.Contains(user.Id))
            {
                return true;
            }

            // Check mod roles
            return Admin.ModeratorRoles.Intersect(user.RoleIds).Any();
        }

        public void AddAdmin(IGuildUser user)
        {
            Admin.AdministratorUsers.Add(user.Id);
        }

        public void AddAdmin(IRole role)
        {
            Admin.AdministratorRoles.Add(role.Id);
        }

        public void AddModerator(IGuildUser user)
        {
            Admin.ModeratorUsers.Add(user.Id);
        }

        public void AddModerator(IRole role)
        {
            Admin.ModeratorRoles.Add(role.Id);
        }
    }
}