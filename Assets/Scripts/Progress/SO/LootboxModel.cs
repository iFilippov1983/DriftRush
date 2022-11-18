using RaceManager.Cars;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace RaceManager.Progress
{
    [Serializable]
    [CreateAssetMenu(menuName = "Progress/LootboxModel", fileName = "LootboxModel", order = 1)]
    public class LootboxModel : SerializedScriptableObject
    {
        [SerializeField] private Rarity _rarity;
        [SerializeField] private bool _isOpen;
        [SerializeField] private CarsRarityScheme _rarityScheme;
        [Space]
        [SerializeField] private int _price;
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

        public List<IReward> Cards => GetRandomCardsList();

        private List<IReward> GetRandomCardsList()
        {
            List<IReward> list = new List<IReward>();
            int lostAmount = Random.Range(_lotsAmountMin, _lotsAmountMax + 1);

            for (int i = 0; i < lostAmount; i++)
            {
                CarCard cardReward = GetRandomCarCard();
                list.Add(cardReward);
            }

            return list;
        }

        private CarCard GetRandomCarCard()
        { 
            CarCard cardReward;

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
            int amount = Random.Range(_cardsAmountMin, _cardsAmountMax + 1);

            cardReward = new CarCard(name, amount);
            return cardReward;
        }


        [ShowInInspector, ReadOnly]
        private int _counter = 0;
        [ShowInInspector]
        private bool _showCarNameAndAmount;

        private int _common = 0;
        private int _uncommon = 0;
        private int _rare = 0;
        private int _epic = 0;
        private int _legendary = 0;

        [Button]
        private void TestProbability()
        {
            CarCard reward = GetRandomCarCard();
            Rarity rarity = Rarity.Common;
            foreach (var pair in _rarityScheme.Scheme)
            {
                if (pair.Value.Contains(reward.CarName))
                    rarity = pair.Key;
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
            Debug.Log($"C: {_common}; U: {_uncommon} | R: {_rare} | E: {_epic} | L: {_legendary}");
            if (_showCarNameAndAmount)
                Debug.Log($"Reward => CarName: {reward.CarName}; Cards Amount: {reward.CardsAmount};");
        }

        [Button]
        private void TestProbability100()
        {
            for (int i = 0; i < 100; i++)
            {
                TestProbability();
            }
        }

        [Button]
        private void ResetCounters()
        {
            _counter = 0;

            _common = 0;
            _uncommon = 0;
            _rare = 0;
            _epic = 0;
            _legendary = 0;
        }
    }
}
