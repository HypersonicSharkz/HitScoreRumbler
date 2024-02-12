using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace HitScoreRumbler
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private Harmony harmony;

        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        private MenuButton MenuButton = new MenuButton("Hit Score Rumbler", "Hit Score Rumbler", OnMenuButtonClick);

        private UI.ConfigFlowCoordinator _configViewFlowCoordinator;


        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, IPA.Config.Config conf)
        {
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
            MenuButtons.instance.RegisterButton(MenuButton);
        }

        private static void OnMenuButtonClick()
        {
            if (Instance._configViewFlowCoordinator == null)
            {
                Instance._configViewFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<UI.ConfigFlowCoordinator>();
            }
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(Instance._configViewFlowCoordinator);
        }
    }
}
