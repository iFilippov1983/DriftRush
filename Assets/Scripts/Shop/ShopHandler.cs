using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.UI;
using System;
using UnityEngine;
using Zenject;

namespace RaceManager.Shop
{
    [RequireComponent(typeof(ShopCore))]
    public class ShopHandler : MonoBehaviour, ILateInitializable
    {
        private ShopCore _shopCore;
        private MainUI _mainUI;
        private Profiler _profiler;
        private ShopScheme _shopScheme;
        private SaveManager _saveManager;

        [Inject]
        private void Construct
            (
            ShopCore shopCore, 
            MainUI mainUI, 
            Profiler profiler, 
            ShopScheme shopScheme,
            SaveManager saveManager
            )
        {
            _shopCore = shopCore;
            _mainUI = mainUI;
            _profiler = profiler;
            _shopScheme = shopScheme;
            _saveManager = saveManager;
        }

        public void LateInitialize()
        {
            InstallPanels();
        }

        private void InstallPanels()
        {
            _mainUI.ShopPanel.InstallAllPanels(_shopScheme.Installers);
        }
    }
}
