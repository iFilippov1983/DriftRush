using RaceManager.Cars;
using RaceManager.Race;
using RaceManager.Root;
using System;
using System.Collections.Generic;

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

        private int _victoriesCycleCounter;
        private int _cardsAmount;

        private CarName _carName;
        private LevelName _levelName;
        private PositionInRace _positionInRace;
        private Lootbox _lootboxToAdd;

        private List<Lootbox> _lootboxes = new List<Lootbox>();
        private List<LevelName> _levels = new List<LevelName>();

        public int Money => _money;
        public int Gems => _gems;
        public int Cups => _cups;

        public int MoneyCost => _moneyCost;
        public int GemsCost => _gemsCost;
        public float IncomeFactor => _incomeFactor;

        public bool CanStartImmediate => _playerProfile.CanStartImmediate;
        public bool GotFirstFreeLootbox => _playerProfile.GotFirstFreeLootbox;
        public bool GotSpecialOffer => _playerProfile.GotSpecialOffer;
        public bool LootboxForRaceEnabled => _playerProfile.LootboxForRaceEnabled;
        
        public int VictoriesCycleCounter => _victoriesCycleCounter;
        public int CardsAmount => _cardsAmount;

        public CarName CarName => _carName;
        public LevelName LevelName => _levelName;
        public PositionInRace LastInRacePosition => _positionInRace;
        public Lootbox LootboxToAdd => _lootboxToAdd;
        public List<Lootbox> Lootboxes => _lootboxes;
        public List<LevelName> AvailableLevels
        {
            get 
            {
                _playerProfile.GiveLevelsTo(this);
                return _levels;
            }
        }

        public Action<Lootbox> OnLootboxOpen;
        public Action<CarName, int> OnCarCardsAmountChange;

        public Profiler(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        public void SetLootboxList(List<Lootbox> lootboxes) => _lootboxes = lootboxes;
        public void SetLevelsList(List<LevelName> levels) => _levels = levels;
        public void SetImmediateStart() => _playerProfile.CanStartImmediate = true;
        public void SetNoAds() => _playerProfile.GotSpecialOffer = true;
        public void SetAdsOn() => _playerProfile.GotSpecialOffer = false;

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
            OnCarCardsAmountChange?.Invoke(carName, _playerProfile.CarCardsAmount(carName));
        }

        public void AddLevel(LevelName levelName)
        {
            _levelName = levelName;
            _playerProfile.AddLevel(this);
        }

        public void AddOrOpenLootbox(Lootbox lootbox)
        {
            if (lootbox.IsOpen && _playerProfile.GotFirstFreeLootbox)
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
            if(_playerProfile.GotFirstFreeLootbox == false)
                _playerProfile.GotFirstFreeLootbox = true;

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
            _playerProfile.AddRaceCount(this);

            if (_playerProfile.VictoriesTotalCounter >= PlayerProfile.UnlockLootboxForRaceThreshold)
            {
                _playerProfile.LootboxForRaceEnabled = true;
            }
        }

        public bool TryBuyWithMoney(int cost)
        {
            if (cost > _playerProfile.Money)
                return false;

            _moneyCost = cost;
            _playerProfile.SubstractMoney(this);
            return true;
        }

        public bool TryBuyWithGems(int cost)
        {
            if (cost > _playerProfile.Gems)
                return false;

            _gemsCost = cost;
            _playerProfile.SubstractGems(this);
            return true;
        }

        public bool TryBuyWithCards(CarName carName, int cost)
        {
            int availableCards = _playerProfile.CarCardsAmount(carName);
            if(cost > availableCards)
                return false;

            _cardsAmount = availableCards - cost;
            _carName = carName;
            _playerProfile.SetCardsAmount(this);
            OnCarCardsAmountChange?.Invoke(carName, _playerProfile.CarCardsAmount(carName));
            return true;
        }

        public bool TryGetLootboxWhithActiveTimer(out Lootbox lootbox)
        {
            _playerProfile.GiveLootboxesTo(this);
            lootbox = _lootboxes.Find(l => l.OpenTimerActivated == true);
            return lootbox != null;
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

        public int GetCardsAmount(CarName carName) => _playerProfile.CarCardsAmount(carName);

        public int GetVictoriesTotalCount() => _playerProfile.VictoriesTotalCounter;

        public void RemoveLootboxWithId(string Id)
        {
            _playerProfile.GiveLootboxesTo(this);
            int index = _lootboxes.FindIndex(l => l.Id == Id);
            _lootboxes.RemoveAt(index);
            _playerProfile.TakeLooboxesFrom(this);
        }

        public void CountVictoryCycle()
        { 
            _victoriesCycleCounter = _playerProfile.VictoriesCycleCounter + 1;
            if(_victoriesCycleCounter > PlayerProfile.VictoriesCycle)
                _victoriesCycleCounter = 1;

            _playerProfile.SetVictoryCycleCounter(this);
        }

        public void ResetVictoriesCounter()
        {
            _victoriesCycleCounter = 0;
            _playerProfile.SetVictoryCycleCounter(this);
        }
    }
}
