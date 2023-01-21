using System.Text.Json.Serialization;

namespace Atlas.Data.Structures
{
    /// <summary>
    /// Holds information about a user's level and total experience.
    /// </summary>
    [Serializable]
    public struct LevelInfo
    {
        public ulong Experience = 0;
        public uint Level = 0;

        private static readonly Random s_rng = new();

        public LevelInfo()
        {
        }

        public LevelInfo(ulong exp, uint level)
        {
            Experience = exp;
            Level = level;
        }

        public override string ToString()
        {
            return $"[Level: {Level}, Experience: {Experience}/{GetMaxExperienceForLevel(Level)}]";
        }

        [JsonIgnore]
        public bool ShouldLevelUp => Experience >= GetMaxExperienceForLevel(Level);

        public static ulong GetMaxExperienceForLevel(uint level)
        {
            // stripped directly from mee6 ¯\_(ツ)_/¯
            return 5 * level * level + 50 * level + 100;
        }

        public static ulong GetExperienceIncrementAmount()
        {
            const int minExp = 15;
            const int maxExp = 25;

            return (ulong)s_rng.Next(minExp, maxExp);
        }
    }
}