using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Listen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using static OsuRTDataProvider.Listen.OsuListenerManager;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace OsuLiveDataExport
{
    class OsuData
    {
        public string player;

        public int marv;
        public int perf;
        public int grea;
        public int good;
        public int ooof;
        public int miss;

        public int score;
        public int combo;
        public int time;

        public double errormin;
        public double errormax;
        public double ur;
        
        public PlayType play_type;
        public List<HitEvent> hit_events;

        public string osu_status;
    }

    class LiveData
    {
        private OsuListenerManager Listener;
        private OsuData Data;
        private string OutputFile;

        private String websocketAddr = "ws://127.0.0.1:3388";
        private WebSocketServer ws;

        public LiveData(OsuListenerManager listenerManager, string file)
        {
            Listener = listenerManager;
            OutputFile = file ;
            Data = new OsuData();

            Listener.OnCount300Changed  += val => Data.marv = val;
            Listener.OnCountGekiChanged += val => Data.perf = val;
            Listener.OnCountKatuChanged += val => Data.grea = val;
            Listener.OnCount100Changed  += val => Data.good = val;
            Listener.OnCount50Changed   += val => Data.ooof = val;
            Listener.OnCountMissChanged += val => Data.miss = val;

            Listener.OnScoreChanged         += val => Data.score = val;
            Listener.OnComboChanged         += val => Data.combo = val;
            Listener.OnPlayerChanged        += val => Data.player = val;

            Listener.OnBeatmapChanged       += ResetData;
            Listener.OnPlayingTimeChanged   += UpdateData;

            Listener.OnErrorStatisticsChanged += (es) =>
            {
                Data.errormin   = es.ErrorMin;
                Data.errormax   = es.ErrorMax;
                Data.ur         = es.UnstableRate;
            };

            Listener.OnHitEventsChanged += (pt, he) =>
            {
                Data.play_type = pt;
                Data.hit_events = he;
            };

            Listener.OnStatusChanged += StatusChanged;

            ws = new WebSocketServer(websocketAddr);
            Console.WriteLine("Starting websocket server for OsuLiveDataExport at "+websocketAddr);
            ws.AddWebSocketService<WebSocketHandler>("/ws");
            ws.Start();
        }

        public void ResetData(Beatmap beatmap = null)
        {
            Data.marv = 0;
            Data.perf = 0;
            Data.grea = 0;
            Data.good = 0;
            Data.ooof = 0;
            Data.miss = 0;

            Data.score = 0;
            Data.combo = 0;
            Data.time = 0;
            Data.errormin = 0;
            Data.errormax = 0;
            Data.ur = 0;
        }

        public void StatusChanged(OsuStatus before, OsuStatus now)
        {
            Data.osu_status = now.ToString();
            Byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data));
            SaveData(data);
        }

        public void UpdateData(int timeplayed)
        {
            Data.time = timeplayed;
            String dataStr = JsonConvert.SerializeObject(Data);
            Byte[] data = Encoding.UTF8.GetBytes(dataStr);
            SaveData(data);
            ws.WebSocketServices.Broadcast(dataStr);
        }

        public void SaveData(Byte[] data)
        {
            File.WriteAllText(OutputFile, string.Empty);
            FileStream stream = File.OpenWrite(OutputFile);
            stream.Write(data, 0, data.Length);
            stream.Close();
        }
    }

    public class WebSocketHandler : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Send("Connected");
        }
    }
}
