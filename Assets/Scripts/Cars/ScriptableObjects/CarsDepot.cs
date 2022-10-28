using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace RaceManager.Cars
{
    [Serializable]
    [CreateAssetMenu(menuName = "Cars/CarsDepot", fileName = "CarsDepot", order = 1)]
    public class CarsDepot : ScriptableObject, ISaveable
    {
        public DriverType DriverType;
        public CarName CurrentCarName;
        public List<CarProfile> Cars;

        public CarProfile CurrentCarProfile => Cars.Find(c => c.CarName == CurrentCarName);
        public Type DataType() => typeof(SaveData);

        public void Load(object saveObject)
        {
            SaveData data = (SaveData)saveObject;
            DriverType = data.driverType;
            CurrentCarName = data.currentCarName;
            Cars = data.cars;
        }

        public object Save()
        {
            SaveData saveData = new SaveData();
            saveData.driverType = DriverType;
            saveData.currentCarName = CurrentCarName;
            saveData.cars = Cars;

            return saveData;
        }

        public class SaveData
        {
            public DriverType driverType;
            public CarName currentCarName;
            public List<CarProfile> cars;
        }
    }
}
