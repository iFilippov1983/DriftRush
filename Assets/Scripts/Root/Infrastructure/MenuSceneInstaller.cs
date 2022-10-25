using RaceManager.Cars.Effects;
using RaceManager.Root;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Infrastructure
{
    public class MenuSceneInstaller : BaseInstaller
    {
        [SerializeField] private MainUI mainUI;

        public override void InstallBindings()
        {
            Bind(mainUI);
        }


       
    }
}
