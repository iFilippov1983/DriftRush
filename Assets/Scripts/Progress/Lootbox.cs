using RaceManager.Cars;
using RaceManager.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Cameras;

namespace RaceManager.Progress
{
    [Serializable]
    public class Lootbox
    {
        private readonly string _id;

        private int _gemsToOpen;
        private LootboxModel _lootboxModel;

        public float TimeToOpenLeft;
        public bool OpenTimerActivated;

        public Lootbox(Rarity rarity)
        {
            _lootboxModel = ResourcesLoader.LoadObject<LootboxModel>(ResourcePath.LootboxModelPath(rarity));
            TimeToOpenLeft = _lootboxModel.TimeToOpen;
            _gemsToOpen = _lootboxModel.GemsToOpen;
            _id = MakeId();
        }

        public Lootbox(Rarity rarity, float timeLeft)
        {
            _lootboxModel = ResourcesLoader.LoadObject<LootboxModel>(ResourcePath.LootboxModelPath(rarity));
            TimeToOpenLeft = timeLeft;
            _gemsToOpen = CalculateGemsToOpen();
            _id = MakeId();
        }

        public Lootbox(string id, Rarity rarity, float timeLeft)
        {
            _lootboxModel = ResourcesLoader.LoadObject<LootboxModel>(ResourcePath.LootboxModelPath(rarity));
            TimeToOpenLeft = timeLeft;
            _gemsToOpen = CalculateGemsToOpen();
            _id = id;
        }

        public string Id => _id;
        public Rarity Rarity => _lootboxModel.Rarity;
        public float Price => _lootboxModel.Price;
        public int InitialTimeToOpen => _lootboxModel.HoursToOpen;
        public int GemsToOpen => _gemsToOpen;
        public bool IsOpen => TimeToOpenLeft <= 0 || _lootboxModel.IsOpen;
        public int MoneyAmountMin => _lootboxModel.MoneyAmountMin;
        public int MoneyAmountMax => _lootboxModel.MoneyAmountMax;
        public int CardsAmountMin => _lootboxModel.CardsAmountMin;
        public int CardsAmountMax => _lootboxModel.CardsAmountMax;

        public List<CarCardReward> CardsList => _lootboxModel.GetCardsList();

        public void RecalculateGemsToOpen()
        {
            _gemsToOpen = CalculateGemsToOpen();
        }

        private int CalculateGemsToOpen()
        {
            float timeFactor = TimeToOpenLeft / _lootboxModel.TimeToOpen;
            float gemsValue = _lootboxModel.GemsToOpen * timeFactor;

            int result = gemsValue < 1 ? 1 : Mathf.CeilToInt(gemsValue);

            return result;
        }

        private string MakeId()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
                .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }
    }
}
