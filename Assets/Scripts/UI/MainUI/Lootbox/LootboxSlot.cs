﻿using UnityEngine.UI;
using System;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

namespace RaceManager.UI
{
    public enum SlotStatus { Empty, Closed, ActiveTimer, LootboxOpen }

    [Serializable]
    public class LootboxSlot : MonoBehaviour
    {
        [SerializeField] private Button _slotButton;
        [Space]
        [SerializeField] private Image _lootboxImage;
        [SerializeField] private Image _imageTimerPassive;
        [SerializeField] private Image _imageTimerActive;
        [Space]
        [SerializeField] private TMP_Text _canOpenText;
        [SerializeField] private TMP_Text _closedText;
        [SerializeField] private TMP_Text _midText;
        [SerializeField] private TMP_Text _bottomText;
        [Space]
        [SerializeField] private RectTransform _timerRect;
        [SerializeField] private TMP_Text _timerText;
        [Space]
        [SerializeField] private RectTransform _fastOpenRect;
        [SerializeField] private TMP_Text _costText;

        private SlotStatus _slotStatus = SlotStatus.Empty;

        [ReadOnly]
        public string CurrentLootboxId;

        public Button SlotButton => _slotButton;
        public Image LootboxImage => _lootboxImage;
        public Image ImageTimerPassive => _imageTimerPassive;
        public Image ImageTimerActive => _imageTimerActive;
        public TMP_Text TimerText => _timerText;
        public TMP_Text CanOpenText => _canOpenText;
        public TMP_Text ClosedText => _closedText;
        public TMP_Text MidText => _midText;
        public TMP_Text BottomText => _bottomText;
        public TMP_Text FastOpenCostText => _costText;
        public RectTransform FastOpenRect => _fastOpenRect;
        public RectTransform TimerRect => _timerRect;
        public Vector3 ImagePosOffset => _lootboxImage.transform.localPosition;
        public SlotStatus SlotStatus 
        { 
            get => _slotStatus; 
            private set => _slotStatus = value; 
        }

        public void SetStatusLootboxOpen(Sprite lootboxSprite = null, string id = null)
        { 
            if(id != null)
                CurrentLootboxId = id;
            SlotStatus = SlotStatus.LootboxOpen;

            BottomText.SetActive(false);
            MidText.SetActive(false);
            ClosedText.SetActive(false);

            TimerRect.SetActive(false);
            FastOpenRect.SetActive(false);

            SlotButton.interactable = true;

            if(lootboxSprite != null)
                LootboxImage.sprite = lootboxSprite;
            LootboxImage.SetActive(true);
            
            ImageTimerPassive.SetActive(true);
            ImageTimerPassive.SetActive(false);

            CanOpenText.SetActive(true);
        }

        public void SetStatusActiveTimer(Sprite lootboxSprite = null, int cost = 0, string id = null)
        {
            if(id != null)
                CurrentLootboxId = id;
            SlotStatus = SlotStatus.ActiveTimer;

            CanOpenText.SetActive(false);
            BottomText.SetActive(false);
            MidText.SetActive(false);
            ClosedText.SetActive(false);

            SlotButton.interactable = true;

            TimerRect.SetActive(true);
            FastOpenRect.SetActive(true);

            if (lootboxSprite != null)
                LootboxImage.sprite = lootboxSprite;
            LootboxImage.SetActive(true);

            ImageTimerPassive.SetActive(false);
            ImageTimerPassive.SetActive(true);

            if (cost != 0)
                FastOpenCostText.text = cost.ToString();
        }

        public void SetStatusClosed(Sprite lootboxSprite, int hoursToOpen, int gemsToPen, string id)
        {
            CurrentLootboxId = id;
            SlotStatus = SlotStatus.Closed;

            SlotButton.interactable = true;

            CanOpenText.SetActive(false);
            MidText.SetActive(false);

            TimerRect.SetActive(false);

            FastOpenRect.SetActive(false);
            FastOpenCostText.text = gemsToPen.ToString();

            LootboxImage.SetActive(true);
            LootboxImage.sprite = lootboxSprite;

            BottomText.SetActive(true);
            BottomText.text = string.Concat(hoursToOpen.ToString(), "h");

            ImageTimerPassive.SetActive(true);
            ImageTimerPassive.SetActive(false);

            ClosedText.SetActive(true);
        }

        public void SetStatusEmpty()
        {
            CurrentLootboxId = string.Empty;
            SlotStatus = SlotStatus.Empty;

            SlotButton.interactable = false;

            LootboxImage.SetActive(false);
            
            CanOpenText.SetActive(false);
            ClosedText.SetActive(false);
            BottomText.SetActive(false);

            TimerRect.SetActive(false);
            FastOpenRect.SetActive(false);

            ImageTimerPassive.SetActive(false);
            ImageTimerPassive.SetActive(false);

            MidText.SetActive(true);
        }
    }
}

