using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using VoiceChat;

namespace Radio
{
    public class Config
    {
        [Description("Enabled or disables plugin from loading on server startup.")]
        public bool IsEnabled { get; set; } = true;

        [Description("If radio is enabled by default when player joins server.")]
        public bool EnabledByDefault { get; set; } = true;

        [Description("Which roles will see radio info broadcast.")]
        public List<RoleTypeId> DisplayRadioInfoForRoles { get; set; } = new List<RoleTypeId>() { RoleTypeId.Spectator, RoleTypeId.None };

        [Description("Radio volume.")]
        public float RadioVolume { get; set; } = 100f;

        [Description("Displayname of radio.")]
        public string RadioName { get; set; } = "Server Radio";

        [Description("Which voice channel radio will use for broadcasting voice.")]
        public VoiceChatChannel RadioChannel { get; set; } = VoiceChatChannel.Intercom;

        [Description("Url of VORBIS stream used for radio. ( Bitrate = 48000, Channels = 1 )")]
        public string StreamUrl { get; set; } = "https://radio.killers.dev/stream";
    }
}
