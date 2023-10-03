using CommandSystem;
using PluginAPI.Core;
using System;

namespace Radio.Commands.GameConsole
{
    public class EnableCommand : ICommand
    {
        public string Command { get; } = "enable";

        public string[] Aliases { get; } = new string[] { "1", "true", "on" };

        public string Description { get; } = "Enables radio.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player plr))
            {
                response = "Only player can execute this command!";
                return false;
            }

            if (plr.TemporaryData.Contains("IsRadioEnabled"))
                plr.TemporaryData.StoredData["IsRadioEnabled"] = true;
            else
                plr.TemporaryData.StoredData.Add("IsRadioEnabled", true);

            response = "Radio enabled!";
            return true;
        }
    }
}
