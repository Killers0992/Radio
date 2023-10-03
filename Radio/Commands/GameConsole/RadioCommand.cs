using CommandSystem;
using System;

namespace Radio.Commands.GameConsole
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class RadioCommand : ParentCommand, IUsageProvider
    {
        public override string Command { get; } = "radio";

        public override string[] Aliases { get; } = new string[0];

        public override string Description { get; } = "Command for radio settings.";

        public string[] Usage { get; } = new string[] { "enable", "disable" };

        public RadioCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new DisableCommand());
            this.RegisterCommand(new EnableCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Please specify a valid subcommand (" + this.Usage[0] + ")!";
            return false;
        }
    }
}
