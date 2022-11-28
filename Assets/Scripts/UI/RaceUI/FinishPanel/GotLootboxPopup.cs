using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class GotLootboxPopup : MonoBehaviour
    {
        [SerializeField] private Image _looboxImage;
        [SerializeField] private TMP_Text _rarityText;

        private Animation _animation;

        public Image LootboxImage => _looboxImage;
        public TMP_Text RarityText => _rarityText;

        private void Awake()
        {
            _animation = GetComponent<Animation>();
        }

        private void OnEnable()
        {
            _animation.Play();
        }

        private void OnDisable()
        {
            _animation.Stop();
        }
    }
}

