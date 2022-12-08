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
        public List<CarProfile> CarProfiles;

        public CarProfile CurrentCarProfile => CarProfiles.Find(c => c.CarName == CurrentCarName);

        public void UpdateProfile(CarProfile newCP)
        {
            CarProfile currentCP = CarProfiles.Find(c => c.CarName == newCP.CarName);
            CarProfiles.Remove(currentCP);
            CarProfiles.Add(newCP);
            CarProfiles.Sort((a, b) => a.CarCharacteristics.Rarity.CompareTo(b.CarCharacteristics.Rarity));
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object saveObject)
        {
            SaveData data = (SaveData)saveObject;
            DriverType = data.driverType;
            CurrentCarName = data.currentCarName;
            CarProfiles = data.carProfiles;
        }

        public object Save()
        {
            SaveData saveData = new SaveData();
            saveData.driverType = DriverType;
            saveData.currentCarName = CurrentCarName;
            saveData.carProfiles = CarProfiles;

            return saveData;
        }

        public class SaveData
        {
            public DriverType driverType = DriverType.Player;
            public CarName currentCarName = CarName.ToyotaSupra;
            public List<CarProfile> carProfiles = new List<CarProfile>();
        }

        [Button]
        public void ResetCars()
        {
            foreach (var profile in CarProfiles)
            {
                foreach (var r in profile.RankingScheme.Ranks)
                {
                    r.IsGranted = false;
                    r.IsReached = false;
                }

                if (profile.CarName == CarName.ToyotaSupra)
                {
                    profile.RankingScheme.Ranks[0].IsGranted = true;
                    profile.RankingScheme.Ranks[0].IsReached = true;
                }

                var c = profile.CarCharacteristics;
                c.CurrentFactorsProgress = c.FactorsMaxTotal / 2;
                c.CurrentAccelerationFactor = c.MaxAccelerationFactor / 2;
                c.CurrentDurabilityFactor = c.MaxDurabilityFactor / 2;
                c.CurrentMobilityFactor = c.MaxMobilityFactor / 2;
                c.CurrentSpeedFactor = c.MaxSpeedFactor / 2;
                c.FactorsMaxCurrent = 0;
            }

            Debug.Log($"All cars ranks IsReached and IsGranted properties are set to False (exept default car - {CarName.ToyotaSupra}) | All CurrentFactors amount are set to Max/2");
        }
    }
}
