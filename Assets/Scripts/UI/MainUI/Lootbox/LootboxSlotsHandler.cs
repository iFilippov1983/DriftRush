﻿using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class LootboxSlotsHandler : MonoBehaviour
    {
        private const int LootboxesListCapacity = 4;

        [SerializeField] private LootboxProgressPanel _lootboxProgress;
        [SerializeField] private LootboxPopup _lootboxPopup;
        [SerializeField] private List<LootboxSlot> _lootboxSlots = new List<LootboxSlot>(LootboxesListCapacity);

        private PlayerProfile _playerProfile;
        private Profiler _profiler;
        private SpritesContainerRewards _spritesRewards;
        private LootboxSlot _activeTimerSlot;
        private Lootbox _activeTimerLootbox;
        private LootboxImageAnimationHandler _lootboxAnimationHandler;

        private bool _hasActiveTimerSlot;

        private float _hoursRounded;
        private float _hours;
        private float _minutes;

        public Action<bool> OnPopupIsActive;
        public Action<Button> OnButtonPressed;

        [Inject]
        private void Construct(Profiler profiler, SpritesContainerRewards spritesRewards)
        {
            _profiler = profiler;
            _spritesRewards = spritesRewards;
        }

        public void Initialize(PlayerProfile playerProfile)
        { 
            _playerProfile = playerProfile;
            _lootboxAnimationHandler = new LootboxImageAnimationHandler(_lootboxProgress.LootboxImage);

            InitializeLootboxSlots();
            InitializeLootboxProgressPanel();

            AddButtonsListeners();

            _lootboxAnimationHandler.OnAnimationFinish += _lootboxProgress.OnAnimationFinish;

            _lootboxProgress.OnImagesDisableComplete += InitializeLootboxProgressPanel;
        }

        public void InitializeLootboxProgressPanel()
        {
            for (int i = 0; i < _lootboxProgress.Images.Length; i++)
                _lootboxProgress.Images[i].SetActive(false);

            int counter = _playerProfile.VictoriesCycleCounter;
            for (int i = 0; i < counter; i++)
                _lootboxProgress.Images[i].SetActive(true);

            string text = string.Empty;
            if (counter == _lootboxProgress.Images.Length)
            {
                if (_playerProfile.CanGetLootbox)
                {
                    GrantCommonLootbox();
                }
            }
            else
            {
                text = $"{_lootboxProgress.Images.Length - counter} more win";
                if (counter != 2)
                    text += "s";
            }
            _lootboxProgress.MoreWinsText.text = text;
        }

        private void InitializeLootboxSlots()
        {
            DateTime lastSaveTime = _playerProfile.LastSaveTime;
            TimeSpan timePassed = DateTime.UtcNow - lastSaveTime;
            float secondsPassed = timePassed.TotalSeconds > float.MaxValue
                ? float.MaxValue
                : (float) timePassed.TotalSeconds;
            float secondsInWeek = 7f * 24f * 60f * 60f;
            secondsPassed = Mathf.Clamp(secondsPassed, 0f, secondsInWeek);

            foreach (var slot in _lootboxSlots)
            {
                int slotIndex = _lootboxSlots.IndexOf(slot);
                Lootbox lootbox = _profiler.GetLootboxIndexOf(slotIndex);

                if (lootbox == null)
                {
                    slot.SetStatusEmpty();
                    continue;
                }

                Sprite sprite = _spritesRewards.GetLootboxSprite(lootbox.Rarity);

                if (lootbox.OpenTimerActivated)
                {
                    //$"UtcNow: {DateTime.UtcNow}; Last save time: {lastSaveTime}; Seconds passed: {secondsPassed}; LB time left: {lootbox.TimeToOpenLeft}".Log();

                    if (secondsPassed >= lootbox.TimeToOpenLeft)
                    {
                        slot.SetStatusLootboxOpen(sprite, lootbox.Id);
                    }
                    else
                    {
                        lootbox.TimeToOpenLeft -= secondsPassed;
                        slot.SetStatusActiveTimer(sprite, lootbox.GemsToOpen, lootbox.Id);

                        _activeTimerLootbox = lootbox;
                        _activeTimerSlot = slot;
                        _hasActiveTimerSlot = true;
                    }
                }
                else
                {
                    slot.SetStatusClosed(sprite, lootbox.InitialTimeToOpen, lootbox.GemsToOpen, lootbox.Id);
                }

                slot.SlotButton.onClick.AddListener(() => OnLootboxSlotClicked(slot));
                slot.SlotButton.onClick.AddListener(() => OnButtonPressedMethod(slot.SlotButton));
            }
        }

        private void OnLootboxSlotClicked(LootboxSlot slot)
        {
            switch (slot.SlotStatus)
            {
                case SlotStatus.Closed:
                    OpenLootboxPopup(slot, false);
                    break;
                case SlotStatus.ActiveTimer:
                    OpenLootboxPopup(slot, true);
                    break;
                case SlotStatus.LootboxOpen:
                    OpenLootboxFromSlot(slot);
                    break;
                case SlotStatus.Empty:
                    break;
            }
        }

        private void OpenLootboxFromSlot(LootboxSlot slot)
        {
            _profiler.OpenLootboxWithId(slot.CurrentLootboxId);
            slot.SetStatusEmpty();
        }

        private void OpenLootboxPopup(LootboxSlot slot, bool timerActive)
        {
            Lootbox lootbox = _profiler.GetLootboxWithId(slot.CurrentLootboxId);

            LootboxPopup.PopupInfo info = new LootboxPopup.PopupInfo()
            {
                lootboxRarity = lootbox.Rarity,
                lootboxSprite = slot.LootboxImage.sprite,
                moneyMin = lootbox.MoneyAmountMin,
                moneyMax = lootbox.MoneyAmountMax,
                cardsMin = lootbox.CardsAmountMin,
                cardsMax = lootbox.CardsAmountMax,
                instantOpenCost = lootbox.GemsToOpen,
                timeToOpen = lootbox.InitialTimeToOpen
            };

            _lootboxPopup.InitiallizeView(info);
            _lootboxPopup.TimerOpenButton.SetActive(!timerActive);
            _lootboxPopup.SpeedupButton.SetActive(timerActive);

            _lootboxPopup.TimerOpenButton.onClick.AddListener(() => SlotStartTimer(slot));
            _lootboxPopup.TimerOpenButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.TimerOpenButton));

            _lootboxPopup.InstantOpenButton.onClick.AddListener(() => SlotInstantOpen(slot));
            _lootboxPopup.InstantOpenButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.InstantOpenButton));

            _lootboxPopup.SpeedupButton.onClick.AddListener(() => SlotSpeedupTimer(slot));
            _lootboxPopup.SpeedupButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.SpeedupButton));

            _lootboxPopup.TimerOpenButton.interactable = !_hasActiveTimerSlot;

            _lootboxPopup.SetActive(true);
            OnPopupIsActive.Invoke(true);
        }

        private void SlotStartTimer(LootboxSlot slot)
        {
            if (_hasActiveTimerSlot)
                return;

            slot.SetStatusActiveTimer();
            Lootbox lootbox = _profiler.GetLootboxWithId(slot.CurrentLootboxId);
            lootbox.OpenTimerActivated = true;
            _activeTimerLootbox = lootbox;

            _activeTimerSlot = slot;
            _hasActiveTimerSlot = true;

            _lootboxPopup.TimerOpenButton.SetActive(false);
            _lootboxPopup.SpeedupButton.SetActive(true);
        }

        private void SlotInstantOpen(LootboxSlot slot)
        {
            Lootbox lootbox = _profiler.GetLootboxWithId(slot.CurrentLootboxId);

            if (_profiler.TryBuyWithGems(lootbox.GemsToOpen))
            {
                _profiler.RemoveLootboxWithId(lootbox.Id);
                lootbox.TimeToOpenLeft = 0;
                CloseLootboxPopup();
                HandleSlotTimer();
                _profiler.AddOrOpenLootbox(lootbox);

                slot.SetStatusEmpty();
            }
        }

        private void SlotSpeedupTimer(LootboxSlot slot)
        {
            "Speedup not implemented".Log(Logger.ColorYellow);
        }

        private void CloseLootboxPopup()
        {
            _lootboxPopup.TimerOpenButton.onClick.RemoveAllListeners();
            _lootboxPopup.InstantOpenButton.onClick.RemoveAllListeners();
            _lootboxPopup.SpeedupButton.onClick.RemoveAllListeners();
 
            _lootboxPopup.SetActive(false);

            OnPopupIsActive.Invoke(false);
        }

        private void GrantCommonLootbox()
        {
            Lootbox lootbox = new Lootbox(Rarity.Common);
            Sprite sprite = _spritesRewards.GetLootboxSprite(Rarity.Common);

            var emptySlot = _lootboxSlots.Find(s => s.SlotStatus == SlotStatus.Empty);

            _lootboxAnimationHandler.OnAnimationFinish += OnAnimationFinish;

            void OnAnimationFinish()
            {
                emptySlot.SetStatusClosed(sprite, lootbox.InitialTimeToOpen, lootbox.GemsToOpen, lootbox.Id);
                emptySlot.SlotButton.onClick.AddListener(() => OnLootboxSlotClicked(emptySlot));
                emptySlot.SlotButton.onClick.AddListener(() => OnButtonPressedMethod(emptySlot.SlotButton));
                _lootboxAnimationHandler.OnAnimationFinish -= OnAnimationFinish;
            }

            _lootboxAnimationHandler.InitializeAnimationWithTarget(emptySlot.gameObject, emptySlot.ImagePosOffset);

            _profiler.AddOrOpenLootbox(lootbox);
            _profiler.ResetVictoriesCounter();

        }

        private void HandleSlotTimer()
        {
            if (_hasActiveTimerSlot)
            {
                if (_activeTimerLootbox.TimeToOpenLeft > 0)
                {
                    _hours = _activeTimerLootbox.TimeToOpenLeft / 3600f;
                    _hoursRounded = Mathf.Floor(_hours);
                    _minutes = Mathf.Floor((_hours - _hoursRounded) * 60f);
                    if (_minutes >= 60) _minutes = 59;

                    _activeTimerSlot.TimerText.text = _hoursRounded.ToString("00") + "h. " + _minutes.ToString("00") + "m.";
                }
                else
                {
                    _activeTimerSlot.SetStatusLootboxOpen();

                    _hasActiveTimerSlot = false;
                    _activeTimerLootbox = null;
                    _activeTimerSlot = null;
                }
            }
        }

        private void UpdateTimer()
        {
            if (_hasActiveTimerSlot)
            {
                _activeTimerLootbox.TimeToOpenLeft -= Time.deltaTime;
            }
        }

        private void OnButtonPressedMethod(Button button) => OnButtonPressed?.Invoke(button);

        private void AddButtonsListeners()
        {
            _lootboxPopup.ClosePopupButton.onClick.AddListener(CloseLootboxPopup);
            _lootboxPopup.ClosePopupButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.ClosePopupButton));

            _lootboxPopup.ClosePopupWindowButton.onClick.AddListener(CloseLootboxPopup);
            _lootboxPopup.ClosePopupWindowButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.ClosePopupWindowButton));
        }

        #region Unity Functions
        private void FixedUpdate()
        {
            UpdateTimer();
        }

        private void OnGUI()
        {
            HandleSlotTimer();
        }

        private void OnEnable()
        {
            _lootboxProgress.SetActive(true);
        }

        private void OnDisable()
        {
            _lootboxProgress.SetActive(false);
        }

        private void OnDestroy()
        {
            _lootboxAnimationHandler.OnAnimationFinish -= _lootboxProgress.OnAnimationFinish;

            _lootboxProgress.OnImagesDisableComplete -= InitializeLootboxProgressPanel;
        }
        #endregion
    }
}

