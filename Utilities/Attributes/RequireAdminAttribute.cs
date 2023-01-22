using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Interactions;

using Atlas.Services;
using Atlas.Data;

namespace Atlas.Utilities
{
    public class RequireAdminAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
        {
            if (context.Client.TokenType != TokenType.Bot)
            {
                return PreconditionResult.FromError($"{nameof(RequireAdminAttribute)} is not supported by this {nameof(TokenType)}");
            }

            // Ignore non-guild contexts
            if (context.Guild is null)
            {
                return PreconditionResult.FromError("Command can only be run in a server.");
            }

            IDataService? data = services.GetService<IDataService>();
            if (data is null)
            {
                return PreconditionResult.FromError($"{nameof(RequireAdminAttribute)} requires a {nameof(IDataService)} to be present");
            }

            GuildConfig config = await data.GetGuildConfigAsync(context.Guild);
            IGuildUser member = await context.Guild.GetUserAsync(context.User.Id);
            if (config.IsUserAdmin(member))
            {
                return PreconditionResult.FromSuccess();
            }

            return PreconditionResult.FromError(ErrorMessage ?? "Command can only be run by server administrators.");
        }
    }
}