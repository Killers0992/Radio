using GameCore;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;

namespace Radio
{
    public class MainClass
    {
        public static MainClass Singleton;

        public PluginHandler Handler;

        [PluginConfig]
        public Config Config;

        public ReferenceHub RadioHub;
        public Player Radio;

        [PluginEntryPoint("Radio", "1.0.0", "Play radio in SL.", "Killers0992")]
        public void Init()
        {
            Singleton = this;
            Handler = PluginHandler.Get(Singleton);

            PluginAPI.Core.Log.Info((Config == null).ToString());

            if (!Config.IsEnabled)
            {
                PluginAPI.Core.Log.Info("Skipped loading of plugin. ( IsEnabled=false )");
                return;
            }

            EventManager.RegisterAllEvents(this);
        }

        [PluginEvent]
        public void OnPlayerJoined(PlayerJoinedEvent ev)
        {
            ev.Player.TemporaryData.StoredData.Add("IsRadioEnabled", Singleton.Config.EnabledByDefault);
        }

        [PluginEvent]
        public void OnRespawn(TeamRespawnEvent ev)
        {
            if (ev.Players.Contains(Radio)) ev.Players.Remove(Radio);
        }

        [PluginEvent]
        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            var player = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            RadioHub = player.GetComponent<ReferenceHub>();
            Radio = Player.Get(RadioHub);

            var connection = new FakeConnection(9999);

            RadioHub.characterClassManager._privUserId = "ID_Dedicated";
            RadioHub.characterClassManager._targetInstanceMode = ClientInstanceMode.DedicatedServer;

            NetworkServer.AddPlayerForConnection(connection, player);

            try
            {
                RadioHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;
                RadioHub.nicknameSync.ViewRange = 0f;
                RadioHub.nicknameSync.SetNick(Config.RadioName);
            }
            catch (Exception)
            {
            }

            PluginAPI.Core.Log.Info((Config == null).ToString());

            var radio = player.AddComponent<RadioPlayer>();
            radio.Init(RadioHub);
            radio.Play(Singleton.Config.StreamUrl);
        }
    }
}
