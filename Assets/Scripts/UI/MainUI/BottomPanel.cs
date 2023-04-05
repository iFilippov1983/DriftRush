using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class BottomPanel : AnimatableSubject
    {
        [Space(20)]
        [Header("Main Fields")]
        [SerializeField] private Button _shopButton;
        [SerializeField] private Image _iapShopPressedImage;
        [Space]
        [SerializeField] private Button _carsCollectionButton;
        [SerializeField] private Image _carsCollectionPressedImage;
        [Space]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Image _mainMenuPressedImage;
        [Space]
        [SerializeField] private Button _tuningButton;
        [SerializeField] private Image _tuningPressedImage;
        [Space]
        [SerializeField] private Button _soonButton;
        [SerializeField] private Image _soonPressedImage;

        public Button ShopButton => _shopButton;
        public Button CarsCollectionButton => _carsCollectionButton;
        public Button MainMenuButton => _mainMenuButton;
        public Button TuneButton => _tuningButton;
        public Button SoonButton => _soonButton;

        public Image IapShopPressedImage => _iapShopPressedImage;
        public Image CarsCollectionPressedImage => _carsCollectionPressedImage;
        public Image MainMenuPressedImage => _mainMenuPressedImage;
        public Image TuningPressedImage => _tuningPressedImage;
        public Image SoonPressedImage => _soonPressedImage;

        //private void Start()
        //{
        //    DeactivateAllPressedImages();

        //    ShopButton.onClick.AddListener(() => TogglePressedImageActivity(_iapShopPressedImage));
        //    CarsCollectionButton.onClick.AddListener(() => TogglePressedImageActivity(_carsCollectionPressedImage));
        //    MainMenuButton.onClick.AddListener(() => TogglePressedImageActivity(_mainMenuPressedImage));
        //    TuneButton.onClick.AddListener(() => TogglePressedImageActivity(_tuningPressedImage));
        //}

        //private void TogglePressedImageActivity(Image pressedImage)
        //{
        //    DeactivateAllPressedImages();

        //    bool active = pressedImage.IsActive() ? false : true;
        //    pressedImage.SetActive(active);
        //}

        //private void DeactivateAllPressedImages()
        //{ 
        //    _iapShopPressedImage.SetActive(false);
        //    _carsCollectionPressedImage.SetActive(false);
        //    _mainMenuPressedImage.SetActive(false);
        //    _tuningPressedImage.SetActive(false);
        //    _soonPressedImage.SetActive(false);
        //}
    }
}

