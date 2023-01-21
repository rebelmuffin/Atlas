using Atlas.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Atlas.Modules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Interactions { get; set; } = default!;
        public DiscordSocketClient Client { get; set; } = default!;
        public IDataService Data { get; set; } = default!;
        public RankService Ranks { get; set; } = default!;

        [SlashCommand("ping", "Biggest pong for the biggest ping!")]
        public async Task PingAsync()
        {
            await RespondAsync($"Pong! Latency: {Context.Client.Latency}", ephemeral: true);
        }

        [SlashCommand("usertest", "Testing command for User storage")]
        public async Task UserTestAsync()
        {
            var profile = await Data.GetUserProfileAsync(Context.User, Context.Guild);

            await RespondAsync(profile.ToString());
        }

        [SlashCommand("addxp", "Adds the specified amount of experience")]
        [RequireOwner]
        public async Task AddExp(int exp, IUser user)
        {
            IUser targetUser = user;
            var profile = await Data.GetUserProfileAsync(targetUser, Context.Guild);

            profile.AddExp((ulong)exp);
            
            await Data.SaveUserProfileAsync(profile);

            await RespondAsync($"Successfully added {exp} experience to {targetUser.Username}'s profile\nProfile: {profile}");
        }

        [SlashCommand("removexp", "Deducts the specified amount of experience")]
        [RequireOwner]
        public async Task RemoveExp(int exp, IUser user)
        {
            IUser targetUser = user;
            var profile = await Data.GetUserProfileAsync(targetUser, Context.Guild);

            profile.DeductExp((ulong)exp);
            
            await Data.SaveUserProfileAsync(profile);

            await RespondAsync($"Successfully deducted {exp} experience from {targetUser.Username}'s profile\nProfile: {profile}");
        }

        [SlashCommand("modaltest", "Command to test modal functionality")]
        public async Task ModalTest()
        {
            await RespondWithModalAsync<TestModal>("test_modal");
        }

        [ModalInteraction("test_modal", true, RunMode.Async)]
        public async Task ModalTestResponse(TestModal modal)
        {
            string name = modal.Name;
            string reason = modal.Reason;

            string message = $"{Context.User}'s name is {name} because {reason}";

            // Don't mention everyone
            AllowedMentions mentions = new();
            mentions.AllowedTypes = AllowedMentionTypes.Users;

            await Context.Interaction.RespondAsync(message, allowedMentions: mentions, ephemeral: true);
        }

        [SlashCommand("selectroles", "Select some roles!")]
        public async Task SelectRoles()
        {
            SelectMenuBuilder menu = new SelectMenuBuilder()
                .WithCustomId("selection_test")
                .WithMinValues(1)
                .WithPlaceholder("Select a role!");

            foreach(var role in Context.Guild.Roles)
            {
                SelectMenuOptionBuilder builder = new SelectMenuOptionBuilder()
                    .WithLabel(role.Name)
                    .WithValue(role.Id.ToString());

                menu.AddOption(builder);
            }

            menu.MaxValues = menu.Options.Count;

            var components = new ComponentBuilder()
                .WithSelectMenu(menu);

            await RespondAsync(components: components.Build());
        }


        [ComponentInteraction("selection_test")]
        public async Task SelectionTest(string[] roles)
        {
            string message = string.Join<string>(',', roles);
            await RespondAsync(message);
        }
    }

    public class TestModal : IModal
    {
        public string Title => "Test Modal";

        [ModalTextInput("text_input")]
        [RequiredInput]
        public string Name { get; set; } = default!;

        [ModalTextInput("reason_input")]
        [RequiredInput]
        public string Reason { get; set; } = default!;
   }
}