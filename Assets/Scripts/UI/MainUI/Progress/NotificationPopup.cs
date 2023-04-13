using RaceManager.Cars;
using RaceManager.Progress;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class NotificationPopup : AnimatableSubject
    {
        [Space(20)]
        [SerializeField] private Button _closePopupWindowButton;
        [SerializeField] private Button _okButton;
        [SerializeField] private Button _upgradeCarButton;
        [SerializeField] private Button _openCollectionButton;
        [Space]
        [SerializeField] private Image _carImage;
        [Space]
        [SerializeField] private TMP_Text _carNameText;
        [SerializeField] private TMP_Text _unlockedCarText;
        [SerializeField] private TMP_Text _canUpgradeCarText;

        public Button ClosePopupWindowButton => _closePopupWindowButton;
        public Button OkButton => _okButton;
        public Button UpgradeCarButton => _upgradeCarButton;
        public Button OpenCollectionButton => _openCollectionButton;
        public Image CarImage => _carImage;
        public TMP_Text CarNameText => _carNameText;
        public TMP_Text UnlockedCarText => _unlockedCarText;
        public TMP_Text CanUpgradeCarText => _canUpgradeCarText;
    }
}

