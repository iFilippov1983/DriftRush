﻿using RaceManager.Cars;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;

namespace RaceManager.Root
{
    [Serializable]
    public class PlayerProfile : ISaveable
    {
        public PositionInRace lastInRacePosition;
        public Currency currency;

        

        public class SaveData
        {
            public PositionInRace LastInRacePosition;
            public Currency Currency;
        }

        public Type DataType() => typeof(SaveData);

        public void Load(object data)
        {
            SaveData saveData = (SaveData)data;
            lastInRacePosition = saveData.LastInRacePosition;
            currency = saveData.Currency;
        }

        public object Save()
        {
            return new SaveData
            {
                LastInRacePosition = lastInRacePosition,
                Currency = currency
            };
        }

        [Serializable]
        public struct Currency
        {
            public int Money;
            public int Gems;
            public int Cups;

            public static Currency operator +(Currency a, Currency b)
            {
                return new Currency
                {
                    Money = a.Money + b.Money,
                    Gems = a.Gems + b.Gems,
                    Cups = a.Cups + b.Cups
                };
            }
        }

        public enum PositionInRace
        { 
            DNF = 0,
            First = 1,
            Second = 2,
            Third = 3,
            Fourth = 4,
            Fifth = 5,
            Sixth = 6,
        }
    }
}
