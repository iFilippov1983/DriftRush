using RaceManager.Progress;
using RaceManager.Root;
using RaceManager.Tools;
using RaceManager.UI;
using System;
using UnityEngine;
using Zenject;

namespace RaceManager.Shop
{
    [RequireComponent(typeof(ShopCore))]
    public class ShopHandler : MonoBehaviour, ILateInitializable, IDisposable
    {
        private const int Amount_80 = 80;
        private const int Amount_500 = 500;
        private const int Amount_1200 = 1200;
        private const int Amount_2500 = 2500;
        private const int Amount_6500 = 6500;
        private const int Amount_14000 = 14000;

        private ShopCore _shopCore;
        private MainUI _mainUI;
        private Profiler _profiler;
        private ShopScheme _shopScheme;
        private SpritesContainerRewards _rewardSpritesContainer;
        private SaveManager _saveManager;

        private ShopPanel ShopPanel => _mainUI.ShopPanel;
        private ShopConfirmationPanel ConfirmationPanel => _mainUI.ShopPanel.ConfirmationPanel;
        private UIAnimator Animator => Singleton<UIAnimator>.Instance;

        [Inject]
        private void Construct
            (
            ShopCore shopCore, 
            MainUI mainUI, 
            Profiler profiler, 
            ShopScheme shopScheme,
            SpritesContainerRewards rewardSptitesContainer,
            SaveManager saveManager
            )
        {
            _shopCore = shopCore;
            _mainUI = mainUI;
            _profiler = profiler;
            _shopScheme = shopScheme;
            _rewardSpritesContainer = rewardSptitesContainer;
            _saveManager = saveManager;
        }

        public void LateInitialize()
        {
            InstallPanels();
            SubscribeToCore();
        }

        private void InstallPanels()
        {
            foreach (var installer in _shopScheme.Installers)
            {
                installer.SpritesContainer = _rewardSpritesContainer;
                installer.ShopCore = _shopCore;
                installer.SpecialOfferGranted = _profiler.GotSpecialOffer;
                installer.OnButtonClicked += HandleButtonClick;
            }

            ShopPanel.InstallAllPanels(_shopScheme.Installers);

            ConfirmationPanel.BackButton.onClick.AddListener(() => CloseConfirmationPanel(ConfirmationPanel.BackButton.name));
            ConfirmationPanel.CloseWindowButton.onClick.AddListener(() => CloseConfirmationPanel(ConfirmationPanel.CloseWindowButton.name));
        }

        private void SubscribeToCore()
        {
            _shopCore.OnPurchaseSuccess += GivePuchased;
            _shopCore.OnTryBuyLootbox += ConfirmBuyLootbox;
            _shopCore.OnTryGemsExchange += ConfirmExchangeGemsToMoney;
        }

        private void GivePuchased(string id)
        {
            switch (id)
            {
                case IapIds.NoAds:
                    GrantNoAds();
                    break;
                case IapIds.Gems_80:
                    AddGems(Amount_80);
                    break;
                case IapIds.Gems_500:
                    AddGems(Amount_500);
                    break;
                case IapIds.Gems_1200:
                    AddGems(Amount_1200);
                    break;
                case IapIds.Gems_2500:
                    AddGems(Amount_2500);
                    break;
                case IapIds.Gems_6500:
                    AddGems(Amount_6500);
                    break;
                case IapIds.Gems_14000:
                    AddGems(Amount_14000);
                    break;
                default:
                    $"ShopHandler can't identify product Id!".Log(Logger.ColorYellow);
                    break;
            }

            HandleButtonClick(id);
        }

        private void ConfirmBuyLootbox(int lootboxCost, Rarity lootboxRarity)
        {
            Sprite sprite = _rewardSpritesContainer.GetLootboxSprite(lootboxRarity);
            string text = lootboxRarity.ToString();
            string cost = lootboxCost.ToString();

            ShopPanel.ActivateConfirmationPanel(RewardType.Lootbox, sprite, text, cost);

            ConfirmationPanel.ConfirmButton.onClick.RemoveAllListeners();
            ConfirmationPanel.ConfirmButton.onClick.AddListener(() => TryBuyLootbox(lootboxCost, lootboxRarity));
        }

        private void TryBuyLootbox(int lootboxCost, Rarity lootboxRarity)
        {
            if (_profiler.TryBuyWithGems(lootboxCost))
            {
                CloseConfirmationPanel(ConfirmationPanel.ConfirmButton.name);
                Lootbox lootbox = new Lootbox(lootboxRarity, -1);
                _profiler.AddOrOpenLootbox(lootbox);

                $"[Shop Handler] Got lootbox => {lootboxRarity}".Log(Logger.ColorYellow);
            }
            else 
            {
                HandleButtonClick(ConfirmationPanel.ConfirmButton.name);
                $"[Shop Handler] Can't buy lootbox => {lootboxRarity}".Log(Logger.ColorRed);
            }
        }

        private void ConfirmExchangeGemsToMoney(int gemsAmount, int moneyAmount)
        {
            Sprite sprite = _rewardSpritesContainer.GetShopSprite(RewardType.Money, moneyAmount);
            string text = moneyAmount.ToString();
            string cost = gemsAmount.ToString();

            ShopPanel.ActivateConfirmationPanel(RewardType.Money, sprite, text, cost);

            ConfirmationPanel.ConfirmButton.onClick.RemoveAllListeners();
            ConfirmationPanel.ConfirmButton.onClick.AddListener(() => TryExchangeGemsToMoney(gemsAmount, moneyAmount));
        }

        private void TryExchangeGemsToMoney(int gemsAmount, int moneyAmount)
        {
            if (_profiler.TryBuyWithGems(gemsAmount))
            {
                CloseConfirmationPanel(ConfirmationPanel.ConfirmButton.name);
                AddMoney(moneyAmount);
            }
            else
            {
                HandleButtonClick(ConfirmationPanel.ConfirmButton.name);
                $"[Shop Handler] Not anough gems to exchange!".Log(Logger.ColorRed);
            }
        }

        private void GrantNoAds()
        {
            if (_shopScheme.TryGetInstallerTypeOf(ShopOfferType.NoAds, out OfferPanelInstaller installer))
            {
                //TODO: Switch off Ads

                foreach (var bonus in installer.NoAdsData.BonusContents)
                {
                    switch (bonus.Type)
                    {
                        case RewardType.Money:
                            AddMoney(bonus.Amount);
                            break;
                       
                        case RewardType.Gems:
                            AddGems(bonus.Amount);
                            break;

                        case RewardType.Lootbox:
                            break;

                        case RewardType.CarCard:
                            break;

                        case RewardType.RaceReward:
                        case RewardType.Cups:
                        case RewardType.RaceMap:
                        case RewardType.IncomeBonus:
                        default:
                            break;
                    }
                }

                ShopPanel.TryRemovePanel(ShopOfferType.NoAds);
                _profiler.SetNoAds();
                _saveManager.Save();
            }
            else
            { 
                $"Installer type of [{ShopOfferType.NoAds}] wasn't found!".Log(Logger.ColorYellow);
            }
        }

        private void AddMoney(int moneyAmount)
        { 
            _profiler.AddMoney(moneyAmount);
            $"[Shop Handler] Money added: {moneyAmount}".Log(Logger.ColorYellow);

            if (ShopPanel.TryGetPanelTransform(ShopOfferType.ExchangeGems, out Transform panelTransform))
            {
                Animator.SpawnGroupOnAndMoveTo
                (
                    RewardType.Money, 
                    _mainUI.CurrencyPanel.transform, 
                    panelTransform, 
                    _mainUI.CurrencyPanel.MoneyImage.transform, 
                    () => 
                {
                    _mainUI.UpdateCurrencyAmountPanels(RewardType.Money);
                    
                });
                return;
            }

            _mainUI.UpdateCurrencyAmountPanels(RewardType.Money);
        }

        private void AddGems(int gemsAmount)
        { 
            _profiler.AddGems(gemsAmount);
            $"[Shop Handler] Money added: {gemsAmount}".Log(Logger.ColorYellow);

            if (ShopPanel.TryGetPanelTransform(ShopOfferType.BuyGems, out Transform panelTransform))
            {
                Animator.SpawnGroupOnAndMoveTo
                (
                    RewardType.Gems, 
                    _mainUI.CurrencyPanel.transform, 
                    panelTransform, 
                    _mainUI.CurrencyPanel.GemsImage.transform, 
                    () =>
                {
                    _mainUI.UpdateCurrencyAmountPanels(RewardType.Gems);
                    
                });
                return;
            }

            _mainUI.UpdateCurrencyAmountPanels(RewardType.Gems);
        }

        private void CloseConfirmationPanel(string buttonName)
        {
            ShopPanel.DeactivateConfirmationPanel();
           HandleButtonClick(buttonName);
        }

        private void HandleButtonClick(string buttonName) => _mainUI.OnButtonPressed?.Invoke(buttonName);

        public void Dispose()
        {
            _shopCore.OnPurchaseSuccess -= GivePuchased;
            _shopCore.OnTryBuyLootbox -= ConfirmBuyLootbox;
            _shopCore.OnTryGemsExchange -= ConfirmExchangeGemsToMoney;

            foreach (var installer in _shopScheme.Installers)
            {
                installer.OnButtonClicked -= HandleButtonClick;
            }
        }
    }
}
