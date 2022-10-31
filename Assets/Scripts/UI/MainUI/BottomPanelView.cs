using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class BottomPanelView : MonoBehaviour
    {
        [SerializeField] private Button _iapShopButton;
        [SerializeField] private Button _carsCollectionButton;
        [SerializeField] private Button _cupsMapButton;
        [SerializeField] private Button _tuningButton;
        [SerializeField] private Button _soonButton;

        public Button IapShopButton => _iapShopButton;
        public Button CarsCollectionButton => _carsCollectionButton;
        public Button CupsMapButton => _cupsMapButton;
        public Button TuneButton => _tuningButton;
        public Button SoonButton => _soonButton;
    }
}

