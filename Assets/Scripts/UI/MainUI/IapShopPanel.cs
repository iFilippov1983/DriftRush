using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class IapShopPanel : MonoBehaviour
    {
        [SerializeField] private Button _closePanelButton;

        public Button ClosePanalButton => _closePanelButton;
    }
}

