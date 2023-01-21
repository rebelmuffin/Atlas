using Discord;

namespace Atlas.Data
{
    public interface IAtlasData
    {
        public Task InitialiseAsync(IDiscordClient client);

        public bool IsValid();
    }
}