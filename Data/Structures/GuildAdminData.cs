namespace Atlas.Data.Structures
{
    /// <summary>
    /// Holds the list of administrators/moderators and their associate roles in a guild.
    /// </summary>
    [Serializable]
    public class GuildAdminData
    {
        public HashSet<ulong> AdministratorUsers = new();
        public HashSet<ulong> ModeratorUsers = new();

        public HashSet<ulong> AdministratorRoles = new();
        public HashSet<ulong> ModeratorRoles = new();
    
        public GuildAdminData()
        {
        }
    }
}