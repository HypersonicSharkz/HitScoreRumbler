using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.MenuButtons;
using HarmonyLib;
using HitScoreRumbler.Configuration;
using HitScoreRumbler.Installers;
using HitScoreRumbler.UI;
using HitScoreRumbler.Utils;
using IPA;
using IPA.Config.Stores;
using IPA.Utilities;
using SiraUtil.Zenject;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HitScoreRumbler
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private Harmony harmony;

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        public void Init(IPALogger logger, IPA.Config.Config conf, Zenjector zenject)
        {
            zenject.UseLogger(logger);
            zenject.Install<HSRMenuInstaller>(Location.Menu);

            PluginConfig.Instance = conf.Generated<PluginConfig>();
            Instance = this;
            Log = logger;
            Log.Info("HitScoreRumbler initialized.");
            harmony = new Harmony("HypersonicSharkz.BeatSaber.HitScoreRumbler");

            string folder = Path.Combine(UnityGame.UserDataPath, "HitScoreRumbler");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            if (string.IsNullOrEmpty(PluginConfig.Instance.ChosenPreset))
                PluginConfig.Instance.ChosenPreset = "default";

            Preset preset = PresetHelper.GetPreset(PluginConfig.Instance.ChosenPreset);
            PluginConfig.Instance.LoadedPreset = preset;
        }

        [OnStart] 
        public void OnApplicationStart()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnEnable]
        public void OnEnable() 
        {

        }
    }
}
