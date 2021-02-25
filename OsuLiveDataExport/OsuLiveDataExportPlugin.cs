using System;
using System.Linq;
using OsuRTDataProvider;
using Sync.Plugins;
using Sync.Tools;

namespace OsuLiveDataExport
{
    [SyncPluginDependency("7216787b-507b-4eef-96fb-e993722acf2e", Version = "^1.5.0", Require = true)]
    [SyncPluginID("03596843-0364-8425-7468-948376594857", VERSION)]
    public class OsuLiveDataExportPlugin : Plugin
    {
        public const string PLUGIN_NAME = "OsuLiveDataExport";
        public const string PLUGIN_AUTHOR = "Adri";
        public const string VERSION = "0.0.1";

        private LiveData data;

        public OsuLiveDataExportPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
            I18n.Instance.ApplyLanguage(new Language());
        }
        public override void OnEnable()
        {

            Plugin plugin = (OsuRTDataProviderPlugin)getHoster().EnumPluings().FirstOrDefault(p => p.Name == "OsuRTDataProvider");
            if (plugin is OsuRTDataProviderPlugin dataprovider)
            {
                data = new LiveData(dataprovider.ListenerManager, @"../OsuLiveDataExport.json");
            }
        }

        public override void OnDisable()
        {
            data = null;
        }

        public override void OnExit()
        {
            OnDisable();
        }
    }
}
