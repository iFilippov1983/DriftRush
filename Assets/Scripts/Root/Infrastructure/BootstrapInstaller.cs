using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using RaceManager;
using RaceManager.Cars.Effects;

namespace RaceManager.Infrastructure
{
    public class BootstrapInstaller : BaseInstaller
    {

        public override void InstallBindings()
        { 
            //TODO: Services installation
        }

        public override void Start()
        {
            base.Start();
            Loader.Load(Loader.Scene.MenuScene);
        }
    }
}
