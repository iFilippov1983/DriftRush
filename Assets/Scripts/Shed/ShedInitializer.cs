using RaceManager.Cameras;
using RaceManager.Cars;
using RaceManager.Root;
using RaceManager.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace RaceManager.Shed
{
    public class ShedInitializer : MonoBehaviour
    {
        [SerializeField] private MaterialsContainer _materialsContainer;
        private CarsDepot _playerCarDepot;
        private CarVisual _carVisual;
        private MainUI _mainUI;
        private Podium _podium;
        private MenuCamerasHandler _menuCamerasHandler;

        [Inject]
        private void Construct(CarsDepot playerCarDepot, MainUI mainUI, Podium podium)
        {
            _menuCamerasHandler = Singleton<MenuCamerasHandler>.Instance;

            _mainUI = mainUI;
            _podium = podium;
            _playerCarDepot = playerCarDepot;
        }

        void Start()
        {
            InitCameras();
            InitCar();
        }

        private void InitCameras()
        {
            _menuCamerasHandler.LookAt(_podium.CarPlace);
        }

        private void InitCar()
        {
            CarFactory carFactory = new CarFactory(_playerCarDepot, _materialsContainer, _podium.CarPlace);
            carFactory.ConstructCarForShed(out _carVisual);
        }


    }
}
