using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Cars/CarProfile", fileName = "CarProfile_Name")]
    public class CarProfile : ScriptableObject
    {
        public string Name;
        public CarRarity Rarity;
        public GameObject Prefab;
        //TODO
        //base stats
        //body parts
    }
}
