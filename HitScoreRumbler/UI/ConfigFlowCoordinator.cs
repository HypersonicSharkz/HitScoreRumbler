using BeatSaberMarkupLanguage;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitScoreRumbler.UI
{
    public class ConfigFlowCoordinator : FlowCoordinator
    {
        private ConfigMenuController _mainPanel;

        public void Awake()
        {
            if (_mainPanel == null)
            {
                _mainPanel = BeatSaberUI.CreateViewController<ConfigMenuController>();
            }
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (firstActivation)
                {
                    SetTitle("HitScoreRumbler");
                    showBackButton = true;
                }

                if (addedToHierarchy)
                {
                    ProvideInitialViewControllers(_mainPanel);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
        }

        protected override void BackButtonWasPressed(ViewController topController)
        {
            base.BackButtonWasPressed(topController);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
