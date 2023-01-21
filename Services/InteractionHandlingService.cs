using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Services
{
    public class InteractionHandlingService
    {
        private DiscordSocketClient Client { get; init; }
        private InteractionService Interactions { get; init; }
        private IServiceProvider Services { get; init; }

        public InteractionHandlingService(IServiceProvider services)
        {
            Services = services;

            Client = Services.GetRequiredService<DiscordSocketClient>();
            Interactions = Services.GetRequiredService<InteractionService>();
        }

        public async Task InitialiseAsync()
        {
            Client.Ready += ReadyAsync;
            Interactions.Log += LogAsync;

            await Interactions.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

            Client.InteractionCreated += HandleInteraction;
        }

        public Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);

            return Task.CompletedTask;
        }

        public async Task ReadyAsync()
        {
            Console.WriteLine("Loaded Commands: ");
            foreach (var command in Interactions.SlashCommands)
            {
                Console.WriteLine($"\t/{command.Name}");
            }

            // #TODO: Might be worth adding an #ifdef to do it for single test server
            await Interactions.RegisterCommandsGloballyAsync(true);
        }

        public async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(Client, interaction);

                var result = await Interactions.ExecuteCommandAsync(context, Services);

                if (!result.IsSuccess)
                {
                    switch (result.Error)
                    {
                        case InteractionCommandError.BadArgs:
                            // #TODO: Implement error handling.
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
                if (interaction.Type is InteractionType.ApplicationCommand)
                {
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }
    }
}