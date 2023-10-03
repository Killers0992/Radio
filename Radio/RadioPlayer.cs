using MEC;
using Mirror;
using NVorbis;
using PluginAPI.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEngine;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;

namespace Radio
{
    public class RadioPlayer : MonoBehaviour
    {
        public static RadioPlayer MainRadio;

        public ReferenceHub Owner;

        public CoroutineHandle Streamhandler;
        public CoroutineHandle InfoHandler;

        public Queue<float> StreamBuffer { get; } = new Queue<float>();
        public float Volume { get; set; } = 100f;

        public bool IsReconnecting = false;

        public void Init(ReferenceHub hub)
        {
            MainRadio = this;
            Volume = MainClass.Singleton.Config.RadioVolume;
            Owner = hub;
        }

        public void OnDestroy()
        {
            Timing.KillCoroutines(Streamhandler);
        }

        public void Play(string url)
        {
            if (Streamhandler.IsValid)
                Timing.KillCoroutines(Streamhandler);

            if (InfoHandler.IsValid)
                Timing.KillCoroutines(InfoHandler);

            Streamhandler = Timing.RunCoroutine(PlayStream(url));
            InfoHandler = Timing.RunCoroutine(InfoBroadcast());
        }

        public void Update()
        {
            if (IsReconnecting)
            {
                IsReconnecting = false;
                Play(lastUrl);
                return;
            }

            if (Owner == null || StreamBuffer.Count == 0) return;

            allowedSamples += Time.deltaTime * VoiceChatSettings.SampleRate;

            int toCopy = Mathf.Min(Mathf.FloorToInt(allowedSamples), StreamBuffer.Count);

            if (toCopy > 0)
            {
                for (int i = 0; i < toCopy; i++)
                {
                    PlaybackBuffer.Write(StreamBuffer.Dequeue() * (Volume / 100f));
                }
            }

            allowedSamples -= toCopy;

            while (PlaybackBuffer.Length >= 480)
            {
                PlaybackBuffer.ReadTo(SendBuffer, (long)480, 0L);
                int dataLen = Encoder.Encode(SendBuffer, EncodedBuffer, 480);

                foreach (var plr in Player.GetPlayers())
                {
                    if (!plr.TemporaryData.Contains("IsRadioEnabled"))
                        continue;

                    if ((bool)plr.TemporaryData.StoredData["IsRadioEnabled"])
                    {
                        plr.Connection.Send(new VoiceMessage(Owner, VoiceChatChannel.Intercom, EncodedBuffer, dataLen, false));
                    }
                }
            }
        }

        private static OpusEncoder Encoder { get; } = new OpusEncoder(OpusApplicationType.Voip);
        private static HttpClient HttpClient { get; } = new HttpClient();

        private const int HeadSamples = 1920;
        private static int MaxBufferSize => VoiceChatSettings.SampleRate / 5 + HeadSamples;

        private PlaybackBuffer PlaybackBuffer { get; } = new PlaybackBuffer();
        private float allowedSamples;

        private byte[] EncodedBuffer { get; } = new byte[512];
        private float[] SendBuffer, ReadBuffer;

        public string lastUrl;

        public string CurrentArtist = "None";
        public TimeSpan CurrentTime = TimeSpan.MinValue;

        private IEnumerator<float> InfoBroadcast()
        {
            while (true)
            {
                try
                {
                    foreach(var player in Player.GetPlayers())
                    {
                        if (!MainClass.Singleton.Config.DisplayRadioInfoForRoles.Contains(player.Role)) continue;

                        player.ReceiveHint($"<size=20>\n\n\n\n\n\n\n<color=green>Server Radio</color>\n{CurrentArtist}\nUse <color=yellow>.radio enable/radio</color> to disable/enable.</size>");
                    }
                }
                catch (Exception) { }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        private IEnumerator<float> PlayStream(string url)
        {
            lastUrl = url;
            Stream radioStream = null;
            bool error = false;
            int count = 0;

            Log.Info($"Trying to play stream " + url);

            try
            {
                radioStream = HttpClient.GetStreamAsync(url).Result;
            }
            catch(Exception ex)
            {
                Log.Error(ex.Message + " ( RETRYING )");
                error = true;
            }

            if (error)
            {
                radioStream?.Dispose();
                yield return Timing.WaitForSeconds(3f);
                IsReconnecting = true;
                yield break;
            }

            using (radioStream)
            {
                VorbisReader reader = new VorbisReader(radioStream);
                Log.Info($"Start reading stream {url}, Channels {reader.Channels}, Sample rate {reader.SampleRate}Hz");

                SendBuffer = new float[MaxBufferSize];
                ReadBuffer = new float[MaxBufferSize];

                while ((count = reader.ReadSamples(ReadBuffer, 0, ReadBuffer.Length)) > 0 && !IsReconnecting)
                {
                    CurrentTime = reader.TimePosition;

                    if (CurrentArtist != reader.Tags.Title)
                    {
                        Log.Info($"{CurrentArtist} -> {reader.Tags.Title}");
                        CurrentArtist = reader.Tags.Title;
                    }

                    while (StreamBuffer.Count >= ReadBuffer.Length)
                    {
                        yield return Timing.WaitForOneFrame;
                    }

                    for (int x = 0; x < ReadBuffer.Length; x++)
                    {
                        StreamBuffer.Enqueue(ReadBuffer[x]);
                    }
                }

                reader.Dispose();

                if (IsReconnecting) yield break;

                Log.Info("Disconnected from stream... ( RECONNECTING )");
                yield return Timing.WaitForSeconds(3f);
            }

            IsReconnecting = true;
        }
    }
}
