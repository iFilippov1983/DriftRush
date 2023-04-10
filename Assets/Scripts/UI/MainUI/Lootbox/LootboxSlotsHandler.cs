using RaceManager.Progress;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RaceManager.UI
{
    public class LootboxSlotsHandler : AnimatableSubject
    {
        private const int LootboxesListCapacity = 4;

        [Space]
        [Header("Main Fields")]
        [SerializeField] private LootboxProgressPanel _lootboxProgress;
        [SerializeField] private LootboxPopup _lootboxPopup;
        [SerializeField] private List<LootboxSlot> _lootboxSlots = new List<LootboxSlot>(LootboxesListCapacity);

        private PlayerProfile _playerProfile;
        private Profiler _profiler;
        private SpritesContainerRewards _spritesRewards;
        private LootboxSlot _activeTimerSlot;
        private Lootbox _activeTimerLootbox;
        private LootboxImageAnimationHandler _lootboxAnimationHandler;
        private GameEvents _gameEvents;

        private bool _hasActiveTimerSlot;

        private float _hoursRounded;
        private float _hours;
        private float _minutes;

        public Action<bool> OnPopupIsActive;
        public Action<Button> OnButtonPressed;
        public Action OnInstantLootboxOpen;

        private UIAnimator Animator => Singleton<UIAnimator>.Instance;

        [Inject]
        private void Construct(Profiler profiler, SpritesContainerRewards spritesRewards, GameEvents gameEvents)
        {
            _profiler = profiler;
            _spritesRewards = spritesRewards;
            _gameEvents = gameEvents;
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

            //if (counter == _lootboxProgress.Images.Length)
            if(_playerProfile.WillGetLootboxForVictiories)
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
            _lootboxProgress.MoreWinsText.text = text.ToUpper();
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

                    if (secondsPassed >= lootbox.TimeToOpenLeft || lootbox.IsOpen)
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

                slot.SlotButton.onClick.RemoveAllListeners();
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

            LootboxPopup.LootboxPopupInfo info = new LootboxPopup.LootboxPopupInfo()
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
            _lootboxPopup.TimerOpenButton.interactable = !_hasActiveTimerSlot;
            _lootboxPopup.TimerOpenButton.onClick.AddListener(() => SlotStartTimer(slot));
            _lootboxPopup.TimerOpenButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.TimerOpenButton));

            _lootboxPopup.InstantOpenButton.SetActive(true);
            _lootboxPopup.InstantOpenButton.onClick.AddListener(() => SlotInstantOpen(slot));
            _lootboxPopup.InstantOpenButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.InstantOpenButton));

            _lootboxPopup.SpeedupButton.SetActive(timerActive);
            _lootboxPopup.SpeedupButton.onClick.AddListener(() => SlotSpeedupTimer(slot));
            _lootboxPopup.SpeedupButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.SpeedupButton));

            _lootboxPopup.GetFreeLootboxButton.SetActive(false);

            _lootboxPopup.SetActive(true);

            _lootboxPopup.LastAppearTransform = slot.transform;
            Animator.AppearSubject(_lootboxPopup, slot.transform).AddTo(this);

            OnPopupIsActive?.Invoke(true);
        }

        public void OpenFreeLootboxPopup()
        {
            Rarity rarity = Rarity.Common;
            float timeLeft = -1f;

            Lootbox lootbox = new Lootbox(rarity, timeLeft)
            {
                OpenTimerActivated = true
            };

            Sprite sprite = _spritesRewards.GetLootboxSprite(rarity);

            LootboxPopup.LootboxPopupInfo info = new LootboxPopup.LootboxPopupInfo()
            {
                lootboxRarity = lootbox.Rarity,
                lootboxSprite = sprite,
                moneyMin = lootbox.MoneyAmountMin,
                moneyMax = lootbox.MoneyAmountMax,
                cardsMin = lootbox.CardsAmountMin,
                cardsMax = lootbox.CardsAmountMax,
                instantOpenCost = lootbox.GemsToOpen,
                timeToOpen = lootbox.InitialTimeToOpen
            };

            _lootboxPopup.InitiallizeView(info);

            _lootboxPopup.TimerOpenButton.SetActive(false);
            _lootboxPopup.InstantOpenButton.SetActive(false);
            _lootboxPopup.SpeedupButton.SetActive(false);

            _lootboxPopup.GetFreeLootboxButton.SetActive(true);
            _lootboxPopup.GetFreeLootboxButton.onClick.AddListener(() => _profiler.AddOrOpenLootbox(lootbox));
            _lootboxPopup.GetFreeLootboxButton.onClick.AddListener(() => OnButtonPressedMethod(_lootboxPopup.GetFreeLootboxButton));
            _lootboxPopup.GetFreeLootboxButton.onClick.AddListener(CloseLootboxPopup);

            _lootboxPopup.SetActive(true);

            _lootboxPopup.LastAppearTransform = _lootboxProgress.transform;
            Animator.AppearSubject(_lootboxPopup, _lootboxProgress.transform).AddTo(this);

            OnPopupIsActive?.Invoke(true);
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

            if (lootbox != null && _profiler.TryBuyWithGems(lootbox.GemsToOpen))
            {
                _profiler.RemoveLootboxWithId(lootbox.Id);
                lootbox.TimeToOpenLeft = -1;
                CloseLootboxPopup();
                HandleSlotTimer();
                _profiler.AddOrOpenLootbox(lootbox);

                OnInstantLootboxOpen?.Invoke();

                slot.SetStatusEmpty();
            }
        }

        private void SlotSpeedupTimer(LootboxSlot slot)
        {
            Debug.LogWarning("Speedup not implemented");
        }

        private void CloseLootboxPopup()
        {
            _lootboxPopup.TimerOpenButton.onClick.RemoveAllListeners();
            _lootboxPopup.InstantOpenButton.onClick.RemoveAllListeners();
            _lootboxPopup.SpeedupButton.onClick.RemoveAllListeners();
            _lootboxPopup.GetFreeLootboxButton.onClick.RemoveAllListeners();

            _lootboxPopup.SetActive(false);

            OnPopupIsActive?.Invoke(false);
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

                if (_profiler.GotFirstFreeLootbox == false)
                {
                    emptySlot.SetStatusLootboxOpen();
                    lootbox.TimeToOpenLeft = -1;
                    lootbox.OpenTimerActivated = true;

                    _gameEvents.Notification.OnNext(NotificationType.FirstLootbox.ToString());
                }

                if (_profiler.GetVictoriesTotalCount() == PlayerProfile.HowToOpenLootboxTutorThreshold)
                {
                    _gameEvents.Notification.OnNext(NotificationType.SecondLootbox.ToString());
                }

                _profiler.AddOrOpenLootbox(lootbox);
                _profiler.ResetVictoriesCounter();

                emptySlot.SlotButton.onClick.RemoveAllListeners();
                emptySlot.SlotButton.onClick.AddListener(() => OnLootboxSlotClicked(emptySlot));
                emptySlot.SlotButton.onClick.AddListener(() => OnButtonPressedMethod(emptySlot.SlotButton));

                _lootboxAnimationHandler.OnAnimationFinish -= OnAnimationFinish;
            }

            _lootboxAnimationHandler.InitializeAnimationWithTarget(emptySlot.gameObject, emptySlot.ImagePosOffset);
        }

        private void HandleSlotTimer()
        {
            if (_hasActiveTimerSlot)
            {
                if (_activeTimerLootbox.TimeToOpenLeft > 0 && !_activeTimerLootbox.IsOpen)
                {
                    _hours = _activeTimerLootbox.TimeToOpenLeft / 3600f;
                    //_hoursRounded = Mathf.RoundToInt(_hours);
                    _hoursRounded = Mathf.Floor(_hours);

                    _minutes = Mathf.Floor((_hours - _hoursRounded) * 60f);
                    if (_minutes >= 60) _minutes = 59;
                    if (_minutes < 1) _minutes = 1;

                    _activeTimerSlot.TimerText.text = _hoursRounded.ToString("00") + "h." + _minutes.ToString("00") + "m.";
                    _activeTimerSlot.FastOpenCostText.text = _activeTimerLootbox.GemsToOpen.ToString();
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
                _activeTimerLootbox.RecalculateGemsToOpen();
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

        private void OnDestroy()
        {
            _lootboxAnimationHandler.OnAnimationFinish -= _lootboxProgress.OnAnimationFinish;

            _lootboxProgress.OnImagesDisableComplete -= InitializeLootboxProgressPanel;
        }

        #endregion
    }
}

