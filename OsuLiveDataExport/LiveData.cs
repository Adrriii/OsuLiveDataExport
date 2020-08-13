using OsuRTDataProvider.BeatmapInfo;
using OsuRTDataProvider.Listen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using static OsuRTDataProvider.Listen.OsuListenerManager;

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

        public string osu_status;
    }

    class LiveData
    {
        private OsuListenerManager Listener;
        private OsuData Data;
        private string OutputFile;

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

            Listener.OnStatusChanged += StatusChanged;
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
            SaveData();
        }

        public void UpdateData(int timeplayed)
        {
            Data.time = timeplayed;
            SaveData();
        }

        public void SaveData()
        {
            Byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Data));
            File.WriteAllText(OutputFile, string.Empty);
            FileStream stream = File.OpenWrite(OutputFile);
            stream.Write(data, 0, data.Length);
            stream.Close();
        }
    }
}
