using Atlas.Services;
using Atlas.Utilities;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;


namespace Atlas
{
    static class Program
    {
        static void Main(string[] _)
            => MainAsync()
            .GetAwaiter()
            .GetResult();

        public static async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                // Create client instance
                var client = services.GetRequiredService<DiscordSocketClient>();

                // #TODO: Subscribe events
                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                // get token
                var token = Environment.GetEnvironmentVariable("TOKEN");
                Error.IfNull(token, "You must set the TOKEN environment variable.");

                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                // Initialise command handler
                await services.GetRequiredService<CommandHandlingService>().InitialiseAsync();
                await services.GetRequiredService<InteractionHandlingService>().InitialiseAsync();
                await services.GetRequiredService<RankService>().InitialiseAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        private static Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage);

            return Task.CompletedTask;
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(
                    new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.All
                    }
                )
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<InteractionHandlingService>()
                .AddSingleton<IDataService, JsonDataService>()
                .AddSingleton<RankService>()
                .BuildServiceProvider();
        }
    }
}