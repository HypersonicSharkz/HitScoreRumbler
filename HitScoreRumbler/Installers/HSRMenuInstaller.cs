using HitScoreRumbler.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace HitScoreRumbler.Installers
{
    internal class HSRMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ConfigMenuController>().AsSingle();
        }
    }
}
