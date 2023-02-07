using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class IapShopPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _contentRect;

        public RectTransform ContentRect => _contentRect;
    }
}

