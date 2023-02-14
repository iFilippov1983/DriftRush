using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Race;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaceManager.UI
{
    [Serializable]
    [CreateAssetMenu(menuName = "Containers/SpritesContainerRewards", fileName = "SpritesContainerRewards", order = 1)]
    public class SpritesContainerRewards : SerializedScriptableObject
    {
        [SerializeField]
        List<ShopRewardSpriteHolder> _shopRewardSprites = new List<ShopRewardSpriteHolder>()
        {
            { new ShopRewardSpriteHolder() { RewardType = RewardType.Money, RewardSprite = null } },
            { new ShopRewardSpriteHolder() { RewardType = RewardType.Money, RewardSprite = null } },
            { new ShopRewardSpriteHolder() { RewardType = RewardType.Money, RewardSprite = null } },
            { new ShopRewardSpriteHolder() { RewardType = RewardType.Money, RewardSprite = null } },

            { new ShopRewardSpriteHolder() { RewardType = RewardType.Gems, RewardSprite = null } },
            { new ShopRewardSpriteHolder() { RewardType = RewardType.Gems, RewardSprite = null } },
            { new ShopRewardSpriteHolder() { RewardType = RewardType.Gems, RewardSprite = null } },
            { new ShopRewardSpriteHolder() { RewardType = RewardType.Gems, RewardSprite = null } },
        };

        [Space(20)]
        [SerializeField]
        private List<SimpleRewardSpriteHolder> _simpleRewardSprites = new List<SimpleRewardSpriteHolder>()
        {
            { new SimpleRewardSpriteHolder() { RewardType = RewardType.Money, RewardSprite = null } },
            { new SimpleRewardSpriteHolder() { RewardType = RewardType.Gems, RewardSprite = null } },
            { new SimpleRewardSpriteHolder() { RewardType = RewardType.Cups, RewardSprite = null } }
        };

        [Space(20)]
        [SerializeField]
        private List<LootboxSriteHolder> _lootboxSrites = new List<LootboxSriteHolder>()
        {
            { new LootboxSriteHolder() { Rarity = Rarity.Common, LootboxSprite = null } },
            { new LootboxSriteHolder() { Rarity = Rarity.Uncommon, LootboxSprite = null } },
            { new LootboxSriteHolder() { Rarity = Rarity.Rare, LootboxSprite = null } },
            { new LootboxSriteHolder() { Rarity = Rarity.Epic, LootboxSprite = null } },
            { new LootboxSriteHolder() { Rarity = Rarity.Legendary, LootboxSprite = null} }
        };

        [Space(20)]
        [SerializeField]
        private List<LevelSpriteHolder> _levelSprites = new List<LevelSpriteHolder>()
        {
            { new LevelSpriteHolder() { LevelName = LevelName.Level_0_test, LevelSprite = null } }
        };

        public Sprite GetSimpleRewardSprite(RewardType rewardType)
        {
            var holder = _simpleRewardSprites.Find(h => h.RewardType == rewardType);
            return holder.RewardSprite;
        }

        public Sprite GetLootboxSprite(Rarity rarity)
        {
            var holder = _lootboxSrites.Find(h => h.Rarity == rarity);
            return holder.LootboxSprite;
        }

        public Sprite GetLevelSprite(LevelName levelName)
        {
            var holder = _levelSprites.Find(h => h.LevelName == levelName);
            return holder.LevelSprite;
        }

        public Sprite GetShopSprite(RewardType rewardType, int rewardsAmount)
        {
            var holder = _shopRewardSprites.Find
                (
                h => 
                h.RewardType == rewardType 
                && rewardsAmount > h.MinAmountThreshold 
                && rewardsAmount <= h.MaxAmountThreshold 
                );

            if (holder == null)
                holder = _shopRewardSprites.First(h => h.RewardType == rewardType);

            return holder.RewardSprite;
        }

        public class LevelSpriteHolder
        {
            public LevelName LevelName;
            public Sprite LevelSprite;
        }

        public class SimpleRewardSpriteHolder
        {
            public RewardType RewardType;
            public Sprite RewardSprite;
        }

        public class LootboxSriteHolder
        { 
            public Rarity Rarity;
            public Sprite LootboxSprite;
        }

        public class ShopRewardSpriteHolder
        {
            public RewardType RewardType;
            public int MinAmountThreshold;
            public int MaxAmountThreshold;
            public Sprite RewardSprite;
        }
    }
}
