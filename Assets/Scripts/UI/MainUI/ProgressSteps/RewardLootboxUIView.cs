using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class RewardLootboxUIView : MonoBehaviour
    {
        [SerializeField] private Image _lootboxImage;

        public Image LootboxImage => _lootboxImage;
    }
}

