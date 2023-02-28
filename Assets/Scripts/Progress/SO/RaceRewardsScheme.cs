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
        [Title("Scores Count Sceme")]
        [SerializeField] private float _scoresFactorDrift = 1.0f;
        [SerializeField] private float _availableDriftPause = 5f;
        [Space]
        [SerializeField] private float _scoresForBump = 100f;
        [SerializeField] private float _scoresForCrush = 100f;
        [SerializeField] private float _minCollisionInterval = 0.1f;
        [Space(20)]
        [SerializeField]
        [DictionaryDrawerSettings(KeyLabel = "Place", ValueLabel = "Reward")]
        private Dictionary<PositionInRace, RaceReward> _scheme = new Dictionary<PositionInRace, RaceReward>()
        {
            { PositionInRace.First, new RaceReward() },
            { PositionInRace.Second, new RaceReward() },
            { PositionInRace.Third, new RaceReward() },
            { PositionInRace.Fourth, new RaceReward() },
            { PositionInRace.Fifth, new RaceReward() },
            { PositionInRace.Sixth, new RaceReward() },
            { PositionInRace.DNF, new RaceReward() }
        };
        [Space(20)]
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

        public float DriftFactor => _scoresFactorDrift;
        public float AvailableDriftPause => _availableDriftPause;
        public float BumpScores => _scoresForBump;
        public float MinCollisionInterval => _minCollisionInterval;
        public float CrushScores => _scoresForCrush;

        public RaceReward GetRewardFor(PositionInRace position) => _scheme[position];

        public bool TryLuckWithNotCommonLootbox(out Rarity rarity)
        {
            float pU = GetChestProbabilities[Rarity.Uncommon];
            float pR = GetChestProbabilities[Rarity.Rare];
            float pE = GetChestProbabilities[Rarity.Epic];
            float pL = GetChestProbabilities[Rarity.Legendary];

            float value = Random.Range(0f, 100f);

            if (value <= pL)
            {
                rarity = Rarity.Legendary;
                return true;
            }
            else if (pL < value & value <= pL + pE)
            {
                rarity = Rarity.Epic;
                return true;
            }
            else if (pL + pE < value && value <= pL + pE + pR)
            {
                rarity = Rarity.Rare;
                return true;
            }
            else if (pL + pE + pR < value && value <= pL + pE + pR + pU)
            {
                rarity = Rarity.Uncommon;
                return true;
            }
            else
            {
                rarity = Rarity.Common;
                return false;
            }
        }

        [ShowInInspector, ReadOnly]
        private int _counter = 0;

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
            bool haveGot = TryLuckWithNotCommonLootbox(out rarity); 
            
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