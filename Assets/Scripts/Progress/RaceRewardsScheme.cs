using RaceManager.Cars;
using RaceManager.Root;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RaceManager.Progress
{
    [Serializable]
    [CreateAssetMenu(menuName = "Progress/RaceRewardsScheme", fileName = "RaceRewardsScheme", order = 1)]
    public class RaceRewardsScheme : SerializedScriptableObject
    {
        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Place", ValueLabel = "Reward")]
        private Dictionary<PositionInRace, RaceReward> _scheme = new Dictionary<PositionInRace, RaceReward>()
        {
            { PositionInRace.First, new RaceReward() { Money = 100, Cups = 10 } },
            { PositionInRace.Second, new RaceReward() { Money = 90, Cups = 8 } },
            { PositionInRace.Third, new RaceReward() { Money = 80, Cups = 6 } },
            { PositionInRace.Fourth, new RaceReward() { Money = 70, Cups = 4 } },
            { PositionInRace.Fifth, new RaceReward() { Money = 60, Cups = 3 } },
            { PositionInRace.Sixth, new RaceReward() { Money = 50, Cups = 2 } },
            { PositionInRace.DNF, new RaceReward() { Money = 0, Cups = 0 } }
        };

        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Chest Rarity", ValueLabel = "Probability To Get (%)")]
        private Dictionary<Rarity, float> GetChestProbabilities = new Dictionary<Rarity, float>()
        {
            { Rarity.Common, 100f },
            { Rarity.Uncommon, 10f },
            { Rarity.Rare, 5f },
            { Rarity.Epic, 3f },
            { Rarity.Legendary, 1f }
        };

        public RaceReward RewardFor(PositionInRace position) => _scheme[position];

        public bool TryGetChanceConsequently(out Rarity rarity)
        {
            rarity = Rarity.Uncommon;
            float probability = GetChestProbabilities[rarity];
            if (Random.Range(0f, 100f) < probability)
                return true;

            rarity = Rarity.Rare;
            probability = GetChestProbabilities[rarity];
            if (Random.Range(0f, 100f) < probability)
                return true;

            rarity = Rarity.Epic;
            probability = GetChestProbabilities[rarity];
            if (Random.Range(0f, 100f) < probability)
                return true;

            rarity = Rarity.Legendary;
            probability = GetChestProbabilities[rarity];
            if (Random.Range(0f, 100f) < probability)
                return true;

            rarity = Rarity.Common;
            return false;
        }

        public bool TryGetChance(out Rarity rarity)
        {
            float pU = GetChestProbabilities[Rarity.Uncommon];
            float pR = GetChestProbabilities[Rarity.Rare];
            float pE = GetChestProbabilities[Rarity.Epic];
            float pL = GetChestProbabilities[Rarity.Legendary];

            float value = Random.value * 100; //Random.Range(0f, 100f);
            if (value <= pU)
            {
                rarity = Rarity.Uncommon;
                return true;
            }
            else if (pU < value & value <= pU + pR)
            {
                rarity = Rarity.Rare;
                return true;
            }
            else if (pU + pR < value && value <= pU + pR + pE)
            {
                rarity = Rarity.Epic;
                return true;
            }
            else if (pU + pR + pE < value && value <= pU + pR + pE + pL)
            {
                rarity = Rarity.Legendary;
                return true;
            }

            rarity = Rarity.Common;
            return false;
        }

        [ShowInInspector, ReadOnly]
        private int _counter = 0;
        [SerializeField]
        private bool _consequently = true;

        private int _yes = 0;
        private int _no = 0;

        private int _uncommon = 0;
        private int _rare = 0;
        private int _epic = 0;
        private int _legendary = 0;

        [Button]
        private void TestProbability()
        { 
            Rarity rarity;
            bool haveGot = _consequently
                ? TryGetChanceConsequently(out rarity)
                : TryGetChance(out rarity);
            
            _counter++;

            if (haveGot)
            {
                _yes++;

                switch (rarity)
                {
                    case Rarity.Common:
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
            }
            else
                _no++;

            Debug.Log($"Got chest: {haveGot} | Rarity: {rarity} | Atempts made: {_counter} | YES({_yes}) - NO({_no})");
            Debug.Log($"U: {_uncommon} | R: {_rare} | E: {_epic} | L: {_legendary}");
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
            _yes = 0;
            _no = 0;
            _uncommon = 0;
            _rare = 0;
            _epic = 0;
            _legendary = 0;
        }
    }
}