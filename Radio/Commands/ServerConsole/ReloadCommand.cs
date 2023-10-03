using CommandSystem;
using PluginAPI.Core;
using System;

namespace Radio.Commands.ServerConsole
{
    public class ReloadCommand : ICommand
    {
        public string Command { get; } = "reload";

        public string[] Aliases { get; } = new string[] { "reload" };

        public string Description { get; } = "Reloads radio.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Player.TryGet(sender, out Player plr))
            {
                response = "Only server can execute this command!";
                return false;
            }

            MainClass.Singleton.Handler.ReloadConfig(MainClass.Singleton);

            RadioPlayer.MainRadio.Volume = MainClass.Singleton.Config.RadioVolume;
            RadioPlayer.MainRadio.lastUrl = MainClass.Singleton.Config.StreamUrl;
            RadioPlayer.MainRadio.IsReconnecting = true;

            response = "Radio reloaded!";
            return true;
        }
    }
}
