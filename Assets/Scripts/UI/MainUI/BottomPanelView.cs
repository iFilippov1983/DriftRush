using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class BottomPanelView : MonoBehaviour
    {
        [SerializeField] private Button _iapShopButton;
        [SerializeField] private Image _iapShopPressedImage;
        [Space]
        [SerializeField] private Button _carsCollectionButton;
        [SerializeField] private Image _carsCollectionPressedImage;
        [Space]
        [SerializeField] private Button _cupsMapButton;
        [SerializeField] private Image _cupsMapPressedImage;
        [Space]
        [SerializeField] private Button _tuningButton;
        [SerializeField] private Image _tuningPressedImage;
        [Space]
        [SerializeField] private Button _soonButton;
        [SerializeField] private Image _soonPressedImage;

        public Button IapShopButton => _iapShopButton;
        public Button CarsCollectionButton => _carsCollectionButton;
        public Button CupsMapButton => _cupsMapButton;
        public Button TuneButton => _tuningButton;
        public Button SoonButton => _soonButton;

        public Image IapSopPressedImage => _iapShopPressedImage;
        public Image CarsCollectionPressedImage => _carsCollectionPressedImage;
        public Image CupsMapPressedImage => _cupsMapPressedImage;
        public Image TuningPressedImage => _tuningPressedImage;
        public Image SoonPressedImage => _soonPressedImage;

        private void Start()
        {
            DeactivateAllPressedImages();

            IapShopButton.onClick.AddListener(() => TogglePressedImageActivity(_iapShopPressedImage));
            CarsCollectionButton.onClick.AddListener(() => TogglePressedImageActivity(_carsCollectionPressedImage));
            CupsMapButton.onClick.AddListener(() => TogglePressedImageActivity(_cupsMapPressedImage));
            TuneButton.onClick.AddListener(() => TogglePressedImageActivity(_tuningPressedImage));
        }

        private void TogglePressedImageActivity(Image pressedImage)
        {
            if (pressedImage.IsActive())
            {
                pressedImage.SetActive(false);
            }
            else
            {
                DeactivateAllPressedImages();
                pressedImage.SetActive(true);
            }
        }

        private void DeactivateAllPressedImages()
        { 
            _iapShopPressedImage.SetActive(false);
            _carsCollectionPressedImage.SetActive(false);
            _cupsMapPressedImage.SetActive(false);
            _tuningPressedImage.SetActive(false);
            _soonPressedImage.SetActive(false);
        }
    }
}

