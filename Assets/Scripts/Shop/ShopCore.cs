using RaceManager.Root;
using RaceManager.Tools;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace RaceManager.Shop
{
    public class ShopCore : MonoBehaviour, IInitializable, IStoreListener
    {
        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;

        public Action<string> OnPurchaseSuccess;

        private bool IsInitialized => 
            m_StoreController != null && m_StoreExtensionProvider != null;

        public void Initialize()
        {
            if (m_StoreController == null)
            {
                InitializePurchasing();
            }
        }

        public async void InitializePurchasing()
        {
            //if (IsInitialized)
            //    return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(IapIds.NoAds, ProductType.NonConsumable); //or ProductType.Subscription if use kind a "vip" sub
            builder.AddProduct(IapIds.Gems_100, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);

            while(!IsInitialized)
                await Task.Yield();
        }

        public void BuyNoAds()
        {
            BuyProductID(IapIds.NoAds);
        }

        public void BuyGems100()
        {
            BuyProductID(IapIds.Gems_100);
        }

        public void BuyProductID(string productId)
        {
            if (IsInitialized)
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            foreach (string id in IapIds.All)
            {
                if (string.Equals(args.purchasedProduct.definition.id, id, StringComparison.Ordinal))
                {
                    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
                    OnPurchaseSuccess?.Invoke(id);
                }
            }

            return PurchaseProcessingResult.Complete;
        }

        public void RestorePurchases() //Only for apple devices. Project also must have a "restore purchases" button. (Automated in google)
        {
            if (!IsInitialized)
            {
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer) //if apple device runtime
            {
                Debug.Log("RestorePurchases started ...");

                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();

                apple.RestoreTransactions((result) =>
                {
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("OnInitialized: PASS");
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }
    }
}
