using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace RaceManager.Shop
{
    public class IAPCore : MonoBehaviour, IStoreListener
    {
        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;

        public static string noads = "noads";       //nonconsumable
        public static string gems_100 = "gems_100"; //consumable

        private void Awake()
        {
            //if (PlayerPrefs.HasKey("ads") == true)
            //{
            //    panelAds.SetActive(false);
            //    panelAds_Done.SetActive(true);
            //}
            //else
            //{
            //    panelAds.SetActive(true);
            //    panelAds_Done.SetActive(false);
            //}

            //if (PlayerPrefs.HasKey("vip") == true)
            //{
            //    panelVIP.SetActive(false);
            //    panelVIP_Done.SetActive(true);
            //}
            //else
            //{
            //    panelVIP.SetActive(true);
            //    panelVIP_Done.SetActive(false);
            //}
        }

        void Start()
        {
            if (m_StoreController == null)
            {
                InitializePurchasing();
            }
        }

        public void InitializePurchasing()
        {
            if (IsInitialized())
            {
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(noads, ProductType.NonConsumable); //or ProductType.Subscription if kind a "vip" sub
            builder.AddProduct(gems_100, ProductType.Consumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public void Buy_noads()
        {
            BuyProductID(noads);
        }

        public void Buy_gems100()
        {
            BuyProductID(gems_100);
        }

        void BuyProductID(string productId)
        {
            if (IsInitialized())
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
            if (string.Equals(args.purchasedProduct.definition.id, noads, StringComparison.Ordinal))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

                //действия при покупке
                //if (PlayerPrefs.HasKey("ads") == false)
                //{
                //    PlayerPrefs.SetInt("ads", 0);
                //    panelAds.SetActive(false);
                //    panelAds_Done.SetActive(true);

                //    //AdsCore.S.HideBanner();
                //    //AdsCore.S.StopAllCoroutines();
                //}

            }
            //else if (String.Equals(args.purchasedProduct.definition.id, vip, StringComparison.Ordinal))
            //{
            //    Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            //    //действия при покупке
            //    if (PlayerPrefs.HasKey("vip") == false)
            //    {
            //        PlayerPrefs.SetInt("vip", 0);
            //        panelVIP.SetActive(false);
            //        panelVIP_Done.SetActive(true);
            //    }
            //}
            else if (string.Equals(args.purchasedProduct.definition.id, gems_100, StringComparison.Ordinal))
            {
                Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

                //действия при покупке
                //GameLogic.S.IncrementPoint2AfterAds(151);
            }
            else
            {
                Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            }

            return PurchaseProcessingResult.Complete;
        }

        public void RestorePurchases() //Only for apple devices. Progect also must has a "restore purchses" button. (Automated in google)
        {
            if (!IsInitialized())
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

        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
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
