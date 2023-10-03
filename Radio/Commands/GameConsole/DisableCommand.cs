using CommandSystem;
using PluginAPI.Core;
using System;

namespace Radio.Commands.GameConsole
{
    public class DisableCommand : ICommand
    {
        public string Command { get; } = "disable";

        public string[] Aliases { get; } = new string[] { "0", "false", "off" };

        public string Description { get; } = "Disables radio.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player plr))
            {
                response = "Only player can execute this command!";
                return false;
            }

            if (plr.TemporaryData.Contains("IsRadioEnabled"))
                plr.TemporaryData.StoredData["IsRadioEnabled"] = false;
            else
                plr.TemporaryData.StoredData.Add("IsRadioEnabled", false);

            response = "Radio disabled!";
            return true;
        }
    }
}
