using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.MenuButtons;
using HarmonyLib;
using HitScoreRumbler.UI;
using IPA;
using IPA.Config.Stores;
using SiraUtil.Zenject;
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
            zenject.Install(Location.Menu, container =>
            {
                container.BindInterfacesAndSelfTo<ConfigMenuController>().FromNewComponentOnRoot().AsSingle();
            });

            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Instance = this;
            Log = logger;
            Log.Info("HitScoreRumbler initialized.");
            harmony = new Harmony("HypersonicSharkz.BeatSaber.HitScoreRumbler");
        }

        [OnStart] 
        public void OnApplicationStart()
        {
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
