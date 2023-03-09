using RaceManager.Progress;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RaceManager.UI
{
    public class ShopConfirmationPanel : AnimatableSubject
    {
        [Serializable]
        public class ConfirmPopup
        {
            public RewardType Type;
            public RectTransform popupRect;
            public Image popupImage;
            public TMP_Text rewardText;
            public TMP_Text rewardCost;
        }

        [Space(20)]
        [Title("Main Fields")]
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _closeWindowButton;

        [SerializeField] private List<ConfirmPopup> _popups = new List<ConfirmPopup>();

        public Button ConfirmButton => _confirmButton;
        public Button BackButton => _backButton;
        public Button CloseWindowButton => _closeWindowButton;
        public List<ConfirmPopup> PopupsList => _popups;
    }
}

