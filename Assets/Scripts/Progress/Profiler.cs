﻿using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaceManager.Progress
{
    public class Profiler : IProfiler
    {
        private PlayerProfile _playerProfile;

        private int _money;
        private int _gems;
        private int _cups;
        private float _incomeFactor;

        private int _moneyCost;
        private int _gemsCost;

        private int _victoriesCounter;
        private int _cardsAmount;

        private CarName _carName;
        private LevelName _levelName;
        private PositionInRace _positionInRace;
        private Lootbox _lootboxToAdd;

        private List<Lootbox> _lootboxes = new List<Lootbox>();

        public int Money => _money;
        public int Gems => _gems;
        public int Cups => _cups;

        public int MoneyCost => _moneyCost;
        public int GemsCost => _gemsCost;
        public float IncomeFactor => _incomeFactor;

        public int VictoriesCounter => _victoriesCounter;
        public int CardsAmount => _cardsAmount;

        public CarName CarName => _carName;
        public LevelName LevelName => _levelName;
        public PositionInRace LastInRacePosition => _positionInRace;
        public Lootbox LootboxToAdd => _lootboxToAdd;
        public List<Lootbox> Lootboxes => _lootboxes;

        public Action<Lootbox> OnLootboxOpen;

        public Profiler(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        public void SetLootboxList(List<Lootbox> lootboxes) => _lootboxes = lootboxes;

        public void AddMoney(int money, bool ignorIncomeFactor = false)
        { 
            _money = money;
            if (ignorIncomeFactor)
                _playerProfile.AddMoney(this);
            else
            { 
                float v = money * _playerProfile.IncomeFactor;
                if (int.MaxValue > v && v > int.MinValue)
                    _playerProfile.AddMoney(this);
            }
        }

        public void AddCups(int cups)
        { 
            _cups = cups;
            _playerProfile.AddCups(this);
        }

        public void AddGems(int gems)
        {
            _gems = gems;
            _playerProfile.AddGems(this);
        }

        public void AddCarCards(CarName carName, int cardsAmount)
        {
            _carName = carName;
            _cardsAmount = cardsAmount;
            _playerProfile.AddCards(this);
        }

        public void AddLevel(LevelName levelName)
        {
            _levelName = levelName;
            _playerProfile.AddLevel(this);
        }

        public void AddOrOpenLootbox(Lootbox lootbox)
        {
            if (lootbox.IsOpen)
                OnLootboxOpen?.Invoke(lootbox);
            else
            {
                _lootboxToAdd = lootbox;
                _playerProfile.AddLootbox(this);
            }

            _playerProfile.GiveLootboxesTo(this);
        }

        public void OpenLootboxWithId(string id)
        {
            Lootbox lootbox = GetLootboxWithId(id);
            RemoveLootboxWithId(id);
            OnLootboxOpen?.Invoke(lootbox);
        }

        public void ModifyIcomeFactor(float incomeBonusInPercents)
        {
            _incomeFactor = _playerProfile.IncomeFactor + incomeBonusInPercents / 100f;
            _playerProfile.SetIcomeFactor(this);
        }

        public void SetNextLevel(LevelName levelName)
        {
            _levelName = levelName;
            _playerProfile.SetNextLevelFrom(this);
        }

        public void SetInRacePosition(PositionInRace positionInRace)
        {
            _positionInRace = positionInRace;
            _playerProfile.SetLastInRacePosition(this);
        }

        public bool TryBuyForMoney(int cost)
        {
            if (cost > _playerProfile.Money)
                return false;

            _moneyCost = cost;
            _playerProfile.SubstractMoney(this);
            return true;
        }

        public bool TryBuyForGems(int cost)
        {
            if (cost > _playerProfile.Gems)
                return false;

            _gemsCost = cost;
            _playerProfile.SubstractGems(this);
            return true;
        }

        public Lootbox GetLootboxIndexOf(int index)
        {
            _playerProfile.GiveLootboxesTo(this);
            if(index >= _lootboxes.Count)
                return null;

            return _lootboxes[index];
        }

        public Lootbox GetLootboxWithId(string Id)
        {
            _playerProfile.GiveLootboxesTo(this);
            Lootbox lootbox = null;
            foreach (var l in _lootboxes)
                if (l.Id == Id)
                {
                    lootbox = l; 
                    break;
                }
                    
            return lootbox;
        }

        public void RemoveLootboxWithId(string Id)
        {
            _playerProfile.GiveLootboxesTo(this);
            int index = _lootboxes.FindIndex(l => l.Id == Id);
            _lootboxes.RemoveAt(index);
            _playerProfile.TakeLooboxesFrom(this);
        }

        public void CountVictory()
        { 
            _victoriesCounter = _playerProfile.VictoriesCounter + 1;
            if(_victoriesCounter > PlayerProfile.VictoriesCounterMax)
                _victoriesCounter = 1;

            _playerProfile.SetVictoryCounter(this);
        }

        public void ResetVictoriesCounter()
        {
            _victoriesCounter = 0;
            _playerProfile.SetVictoryCounter(this);
        }
    }
}