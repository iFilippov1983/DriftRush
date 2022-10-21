using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Cars/CarProfile", fileName = "CarProfile", order = 1)]
    public class CarProfile : ScriptableObject
    {
        public CarName CarName;
        public CarRarity Rarity;
        
        public CarConfig CarConfig;
        public CarConfigVisual CarConfigVisual;

        [SerializeField] private GameObject _prefab;

        private Car _car;

        public GameObject Prefab => _prefab;
    }
}
