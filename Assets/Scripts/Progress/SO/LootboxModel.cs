using RaceManager.Cars;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RaceManager.Progress
{
    [Serializable]
    [CreateAssetMenu(menuName = "Progress/LootboxModel", fileName = "LootboxModel", order = 1)]
    public class LootboxModel : SerializedScriptableObject
    {
        private const float CommonFactor = 0.55f;
        private const float UncommonFactor = 0.65f;
        private const float RareFactor = 0.8f;
        private const float EpicFactor = 0.9f;
        private const float LegendaryFactor = 1f;

        [SerializeField] private Rarity _rarity;
        [SerializeField] private bool _isOpen;
        [SerializeField] private CarsRarityScheme _rarityScheme;
        [Space]
        [Tooltip("How much gems costs to BUY this Lootbox")]
        [SerializeField] private float _price;
        [Title("Loot")]
        [SerializeField] private int _cardsAmountMax;
        [SerializeField] private int _cardsAmountMin;
        [Space]
        [SerializeField] private int _moneyAmountMax;
        [SerializeField] private int _moneyAmountMin;
        [Space]
        [SerializeField] private int _lotsAmountMax;
        [SerializeField] private int _lotsAmountMin;
        [Title("Open settings")]
        [SerializeField] private int _hoursToOpen;
        [SerializeField] private int _gemsToOpen;
        [Title("Cards rate (%)")]
        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Card rarity", ValueLabel = "Probability (%)")]
        private Dictionary<Rarity, float> _probabilities = new Dictionary<Rarity, float>()
        {
            { Rarity.Common, 0f },
            { Rarity.Uncommon, 0f },
            { Rarity.Rare, 0f },
            { Rarity.Epic, 0f },
            { Rarity.Legendary, 0f }
        };

        public Rarity Rarity => _rarity;
        public bool IsOpen => _isOpen;
        public float Price => _price;
        public int HoursToOpen => _hoursToOpen;
        public float TimeToOpen => _hoursToOpen * 60f * 60f; // * seconds in min * minutes in hour
        public int GemsToOpen => _gemsToOpen;
        public int MoneyAmountMin => _moneyAmountMin;
        public int MoneyAmountMax => _moneyAmountMax;
        public int CardsAmountMin => _cardsAmountMin;
        public int CardsAmountMax => _cardsAmountMax;

        [Button]
        public List<CarCardReward> GetCardsList()
        {
            List<CarCardReward> list = new List<CarCardReward>();
            int lotsAmount = Random.Range(_lotsAmountMin, _lotsAmountMax + 1);
            int cardsAmount = Random.Range(_cardsAmountMin, _cardsAmountMax + 1);
            float factor = GetFactor();

            int[] amounts = GetAmounts(new int[lotsAmount], cardsAmount, factor);

            for (int i = 0; i < lotsAmount; i++)
            {
                (CarName name, Rarity rarity) = GetRandomCarName();

                CarCardReward cardReward = new CarCardReward(name, rarity, amounts[i]);
                list.Add(cardReward);

                //Debug.Log($"Name: {name}; Amount: {amounts[i]};");
            }

            return list;
        }

        private float GetFactor()
        {
            float factor = _rarity switch
            {
                Rarity.Common => CommonFactor,
                Rarity.Uncommon => UncommonFactor,
                Rarity.Rare => RareFactor,
                Rarity.Epic => EpicFactor,
                Rarity.Legendary => LegendaryFactor,
                _ => throw new NotImplementedException(),
            };

            return factor;
        }

        private int[] GetAmounts(int[] array, int cardsTotal, float factor)
        {
            //Debug.Log($"<color=red><i><b>Cards total: {cardsTotal}</b></i></color>");
            int counter = array.Length;
            int amount = 0;
            for (int i = 0; i < array.Length; i++)
            {
                int amountPotential = cardsTotal / counter;

                int min = Mathf.RoundToInt(amountPotential * factor);
                amount = Random.Range(min, amountPotential + 1);

                cardsTotal -= amount;
                counter--;

                array[i] = amount;
            }

            if (cardsTotal != 0)
                array[array.Length - 1] = cardsTotal + amount;

            int reverseIndex = Random.Range(0, array.Length);
            Array.Reverse(array, reverseIndex, array.Length - reverseIndex);
            return array;
        }

        private (CarName, Rarity) GetRandomCarName()
        { 
            float pU = _probabilities[Rarity.Uncommon];
            float pR = _probabilities[Rarity.Rare];
            float pE = _probabilities[Rarity.Epic];
            float pL = _probabilities[Rarity.Legendary];

            float value = Random.Range(0f, 100f);
            CarName name;
            List<CarName> list;
            Rarity rarity;

            if (value <= pL)
            {
                rarity = Rarity.Legendary;
            }
            else if (pL < value && value <= pL + pE)
            {
                rarity = Rarity.Epic;
            }
            else if (pL + pE < value && value <= pL + pE + pR)
            {
                rarity = Rarity.Rare;
            }
            else if (pL + pE + pR < value && value <= pL + pE + pR + pU)
            {
                rarity = Rarity.Uncommon;
            }
            else
            {
                rarity = Rarity.Common;
            }

            list = _rarityScheme.Scheme[rarity];
            name = list[Random.Range(0, list.Count)];

            return (name, rarity);
        }

        #region Test methods
        [ShowInInspector, ReadOnly]
        private int _counter = 0;

        private int _common = 0;
        private int _uncommon = 0;
        private int _rare = 0;
        private int _epic = 0;
        private int _legendary = 0;

        private Dictionary<CarName, CarTestInfo> _carProbabilities = new Dictionary<CarName, CarTestInfo>();

        [Button]
        private void TestProbability()
        {
            (CarName name, Rarity rarity) = GetRandomCarName();

            //Rarity rarity = Rarity.Common;
            //foreach (var pair in _rarityScheme.Scheme)
            //{
            //    if (pair.Value.Contains(name))
            //        rarity = pair.Key;
            //}

            if (_carProbabilities.ContainsKey(name))
            {
                _carProbabilities[name].counter++;
            }
            else
            { 
                _carProbabilities.Add(name, new CarTestInfo() { rarity = rarity, counter = 1});
            }

            _counter++;

            switch (rarity)
            {
                case Rarity.Common:
                    _common++;
                    break;
                case Rarity.Uncommon:
                    _uncommon++;
                    break;
                case Rarity.Rare:
                    _rare++;
                    break;
                case Rarity.Epic:
                    _epic++;
                    break;
                case Rarity.Legendary:
                    _legendary++;
                    break;
            }

            Debug.Log($"Chest rarity: {_rarity} | Card rarity: {rarity} | Atempts made: {_counter}");
            Debug.Log($"Rarities => C: {_common}; U: {_uncommon} | R: {_rare} | E: {_epic} | L: {_legendary}");
        }

        [Button]
        private void TestProbabilityTimes(int times)
        {
            for (int i = 0; i < times; i++)
            {
                TestProbability();
            }

            foreach (var c in _carProbabilities)
            {
                Debug.Log($"Car [{c.Value.rarity}]: {c.Key} => {c.Value.counter}");
            }
        }

        [Button]
        private void ResetTestValues()
        {
            _counter = 0;

            _common = 0;
            _uncommon = 0;
            _rare = 0;
            _epic = 0;
            _legendary = 0;

            _carProbabilities.Clear();
        }

        private class CarTestInfo
        { 
            public Rarity rarity;
            public int counter;
        }

        #endregion
    }
}
