using CommandSystem;
using System;

namespace Radio.Commands.ServerConsole
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class ServerRadioCommand : ParentCommand, IUsageProvider
    {
        public override string Command { get; } = "radio";

        public override string[] Aliases { get; } = new string[0];

        public override string Description { get; } = "Command for radio settings.";

        public string[] Usage { get; } = new string[] { "reload" };

        public ServerRadioCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new ReloadCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Please specify a valid subcommand (" + this.Usage[0] + ")!";
            return false;
        }
    }
}
