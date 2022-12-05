using RaceManager.Cars;
using RaceManager.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityStandardAssets.Cameras;

namespace RaceManager.Progress
{
    [Serializable]
    public class Lootbox
    {
        private readonly string _id;

        private LootboxModel _lootboxModel;

        public float TimeToOpenLeft;
        public bool OpenTimerActivated;

        public Lootbox(Rarity rarity)
        {
            _lootboxModel = ResourcesLoader.LoadObject<LootboxModel>(ResourcePath.LootboxModelPath(rarity));
            TimeToOpenLeft = _lootboxModel.TimeToOpen;
            _id = MakeId();
        }

        public Lootbox(Rarity rarity, float timeLeft)
        {
            _lootboxModel = ResourcesLoader.LoadObject<LootboxModel>(ResourcePath.LootboxModelPath(rarity));
            TimeToOpenLeft = timeLeft;
            _id = MakeId();
        }

        public Lootbox(string id, Rarity rarity, float timeLeft)
        {
            _lootboxModel = ResourcesLoader.LoadObject<LootboxModel>(ResourcePath.LootboxModelPath(rarity));
            TimeToOpenLeft = timeLeft;
            _id = id;
        }

        public string Id => _id;
        public Rarity Rarity => _lootboxModel.Rarity;
        public float Price => _lootboxModel.Price;
        public int InitialTimeToOpen => _lootboxModel.HoursToOpen;
        public int GemsToOpen => _lootboxModel.GemsToOpen;
        public bool IsOpen => TimeToOpenLeft <= 0 || _lootboxModel.IsOpen;
        public int MoneyAmountMin => _lootboxModel.MoneyAmountMin;
        public int MoneyAmountMax => _lootboxModel.MoneyAmountMax;
        public int CardsAmountMin => _lootboxModel.CardsAmountMin;
        public int CardsAmountMax => _lootboxModel.CardsAmountMax;

        public List<CarCardReward> CardsList => _lootboxModel.GetCardsList();

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
