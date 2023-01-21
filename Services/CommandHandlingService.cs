using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Discord.Interactions;

namespace Atlas.Services
{
    public class CommandHandlingService
    {
        private CommandService Commands { get; init; }
        private DiscordSocketClient Client { get; init; }
        private IServiceProvider Services { get; init; }

        public CommandHandlingService(IServiceProvider services)
        {
            Services = services;
            Commands = Services.GetRequiredService<CommandService>();
            Client = Services.GetRequiredService<DiscordSocketClient>();

            Commands.CommandExecuted += CommandExecutedAsync;
            Client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitialiseAsync()
        {
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage userMessage) // Ignore system messages
            {
                return;
            }
            if (userMessage.Source != MessageSource.User) // Ignore bots
            {
                return;
            }

            var argPos = 0;

            if (userMessage.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(Client, userMessage);
            await Commands.ExecuteAsync(context, argPos, Services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
        {
            if (command.IsSpecified == false)
            {
                return;
            }

            if (result.IsSuccess)
            {
                return;
            }

            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}