using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [CreateAssetMenu(menuName = "Cars/CarsDepot", fileName = "CarsDepot", order = 1)]
    public class CarsDepot : ScriptableObject
    {
        public List<CarProfile> Cars; 
    }
}
