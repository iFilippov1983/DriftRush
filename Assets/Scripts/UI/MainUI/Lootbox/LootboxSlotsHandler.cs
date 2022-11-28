using RaceManager.Cars;
using RaceManager.Progress;
using RaceManager.Root;
using System;
using System.Collections.Generic;
using UnityEngine;
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

        public Action<bool> OnPopupIsActive;

        [Inject]
        public void Construct(Profiler currencyHandler, SpritesContainerRewards spritesRewards)
        {
            _profiler = currencyHandler;
            _spritesRewards = spritesRewards;
        }

        public void Initialize(PlayerProfile playerProfile)
        { 
            _playerProfile = playerProfile;

            InitializeLootboxProgressPanel();
            InitializeLootboxSlots();

            AddButtonsListeners();
        }

        private void InitializeLootboxProgressPanel()
        {
            for (int i = 0; i < _lootboxProgress.Images.Length; i++)
                _lootboxProgress.Images[i].SetActive(false);

            int counter = _playerProfile.VictoriesCounter;
            for (int i = 0; i < counter; i++)
                _lootboxProgress.Images[i].SetActive(true);

            string text = string.Empty;
            if (counter == _lootboxProgress.Images.Length)
            {
                if (_playerProfile.CanGetLootbox)
                {
                    Lootbox lootbox = new Lootbox(Rarity.Common);
                    _profiler.AddOrOpenLootbox(lootbox);
                    InitializeLootboxSlots();
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
            float secondsPassed = (float)timePassed.TotalSeconds;
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

                Sprite sprite = _spritesRewards.GetLootboxSprite(lootbox.LootboxModel.Rarity);

                if (lootbox.OpenTimerActivated == false)
                {
                    slot.SetStatusClosed(sprite, lootbox.LootboxModel.TimeToOpen, lootbox.Id);
                }

                if (lootbox.OpenTimerActivated)
                {
                    if (secondsPassed > lootbox.TimeToOpenLeft)
                    {
                        slot.SetStatusLootboxOpen(sprite, lootbox.Id);
                    }
                    else
                    {
                        slot.SetStatusActiveTimer(sprite, lootbox.LootboxModel.GemsToOpen, lootbox.Id);
                    }
                }

                slot.SlotButton.onClick.AddListener(() => OnLootboxSlotClicked(slot));
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
                lootboxRarity = lootbox.LootboxModel.Rarity,
                lootboxSprite = slot.LootboxImage.sprite,
                moneyMin = lootbox.LootboxModel.MoneyAmountMin,
                moneyMax = lootbox.LootboxModel.MoneyAmountMax,
                cardsMin = lootbox.LootboxModel.CardsAmountMin,
                cardsMax = lootbox.LootboxModel.CardsAmountMax,
                instantOpenCost = lootbox.LootboxModel.GemsToOpen,
                timeToOpen = lootbox.LootboxModel.TimeToOpen
            };

            _lootboxPopup.InitiallizeView(info);
            _lootboxPopup.TimerOpenButton.SetActive(!timerActive);
            _lootboxPopup.SpeedupButton.SetActive(timerActive);

            _lootboxPopup.TimerOpenButton.onClick.AddListener(() => SlotStartTimer(slot));
            _lootboxPopup.InstantOpenButton.onClick.AddListener(() => SlotInstantOpen(slot));
            _lootboxPopup.SpeedupButton.onClick.AddListener(() => SlotSpeedupTimer(slot));

            _lootboxPopup.SetActive(true);
            OnPopupIsActive.Invoke(true);
        }

        private void SlotStartTimer(LootboxSlot slot)
        {
            slot.SetStatusActiveTimer();
            Lootbox lootbox = _profiler.GetLootboxWithId(slot.CurrentLootboxId);
            lootbox.OpenTimerActivated = true;

            _lootboxPopup.TimerOpenButton.SetActive(false);
            _lootboxPopup.SpeedupButton.SetActive(true);
        }

        private void SlotInstantOpen(LootboxSlot slot)
        {
            slot.SetStatusEmpty();
            Lootbox lootbox = _profiler.GetLootboxWithId(slot.CurrentLootboxId);

            if (_profiler.TryBuyForGems(lootbox.LootboxModel.GemsToOpen))
            {
                _profiler.RemoveLootboxWithId(lootbox.Id);
                lootbox.TimeToOpenLeft = 0;
                CloseLootboxPopup();
                _profiler.AddOrOpenLootbox(lootbox);
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

        private void AddButtonsListeners()
        {
            _lootboxPopup.ClosePopupButton.onClick.AddListener(CloseLootboxPopup);
        }

        

        private void FixedUpdate()
        {
            UpdateTimers();
        }

        private void OnGUI()
        {
            
        }

        private void UpdateTimers()
        {
            foreach (var lootbox in _profiler.Lootboxes)
            {
                if (lootbox.OpenTimerActivated)
                    lootbox.TimeToOpenLeft -= Time.fixedDeltaTime;
            }
        }

        private void OnEnable()
        {
            _lootboxProgress.SetActive(true);
        }

        private void OnDisable()
        {
            _lootboxProgress.SetActive(false);
        }
    }
}

