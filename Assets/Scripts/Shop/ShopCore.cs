using RaceManager.Root;
using RaceManager.Tools;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace RaceManager.Shop
{
    public class ShopCore : MonoBehaviour, IStoreListener //, IInitializable
    {
        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;

        public Action<string> OnPurchaseSuccess;

        /// <summary>
        /// int - cost in gems; Rarity = Lootbox rarity
        /// </summary>
        public Action<int, Rarity> OnTryBuyLootbox;

        /// <summary>
        /// int 1 - gems amount; int 2 - money amount
        /// </summary>
        public Action<int, int> OnTryGemsExchange;

        private bool IsInitialized => 
            m_StoreController != null && m_StoreExtensionProvider != null;

        private void Awake()
        {
            InitializePurchasing();
        }

        //public void Initialize()
        //{
        //    if (m_StoreController == null)
        //    {
        //        InitializePurchasing();
        //    }
        //}

        public async void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(IapIds.NoAds, ProductType.NonConsumable); //or ProductType.Subscription if use kind a "vip" sub
            builder.AddProduct(IapIds.Gems_80, ProductType.Consumable);
            builder.AddProduct(IapIds.Gems_500, ProductType.Consumable);
            builder.AddProduct(IapIds.Gems_1200, ProductType.Consumable);
            builder.AddProduct(IapIds.Gems_2500, ProductType.Consumable);
            builder.AddProduct(IapIds.Gems_6500, ProductType.Consumable);
            builder.AddProduct(IapIds.Gems_14000, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);

            while(!IsInitialized)
                await Task.Yield();
        }

        public void BuyNoAds()
        {
            BuyProductID(IapIds.NoAds);
        }

        public void BuyGems80()
        {
            BuyProductID(IapIds.Gems_80);
        }

        public void BuyGems500()
        {
            BuyProductID(IapIds.Gems_500);
        }

        public void BuyGems1200()
        { 
            BuyProductID(IapIds.Gems_1200);
        }

        public void BuyGems2500()
        { 
            BuyProductID(IapIds.Gems_2500);
        }

        public void BuyGems6500()
        {
            BuyProductID(IapIds.Gems_6500);
        }

        public void BuyGems14000()
        {
            BuyProductID(IapIds.Gems_14000);
        }

        public void BuyProductID(string productId)
        {
            if (IsInitialized)
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("[Shop Core] Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("[Shop Core] BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("[Shop Core] BuyProductID FAIL. Not initialized.");
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            foreach (string id in IapIds.All)
            {
                if (string.Equals(args.purchasedProduct.definition.id, id, StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("[Shop Core] ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                    OnPurchaseSuccess?.Invoke(id);
                }
            }

            return PurchaseProcessingResult.Complete;
        }

        public void RestorePurchases() //Only for apple devices. Project also must have a "restore purchases" button. (Automated in google)
        {
            if (!IsInitialized)
            {
                Debug.Log("[Shop Core] RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer) //if apple device runtime
            {
                Debug.Log("[Shop Core] RestorePurchases started ...");

                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();

                apple.RestoreTransactions((b, s) =>
                {
                    Debug.Log("[Shop Core] RestorePurchases continuing: " + b + ". If no further messages, no purchases available to restore. Message: " + s);
                });
            }
            else
            {
                Debug.Log("[Shop Core] RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[Shop Core] OnInitialized: PASS");
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("[Shop Core] OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log($"[Shop Core] OnInitializeFailed InitializationFailureReason: {error}; Message: {message}");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log($"[Shop Core] OnInitializeFailed InitializationFailureReason: {error}");
        }
    }
}
