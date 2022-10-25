using RaceManager.Tools;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Cars/CarProfile", fileName = "CarProfile", order = 1)]
    public class CarProfile : ScriptableObject
    {
        public CarName CarName;
        public CarRarity Rarity;
        public bool isAvailable;
        public CarConfig CarConfig;
        public CarVisualContainer CarVisualContainer;

        private GameObject _prefab;
        private Car _car;
        private CarVisual _carVisual;

        public GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                    _prefab = ResourcesLoader.LoadPrefab(ResourcePath.CarPrefabsFolder + CarName.ToString());
                return _prefab;
            }
        }

        public Car Car
        {
            get
            {
                if (_car == null)
                    _car = Prefab.GetComponent<Car>();
                return _car;
            }
        }

        public CarVisual CarVisual
        {
            get
            {
                if (_carVisual == null)
                    _carVisual = Prefab.GetComponent<CarVisual>();
                return _carVisual;
            }
        }
    }
}
