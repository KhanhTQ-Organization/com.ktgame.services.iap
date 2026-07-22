#if FIREBASE_ANALYTICS

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Firebase.Analytics;
using com.ktgame.iap.core;

namespace com.ktgame.iap.extensions.firebase_revenue
{
    public class FirebasePurchaseMeasureRevenue : IPurchase
    {
        private readonly IPurchase _purchase;

        public FirebasePurchaseMeasureRevenue(IPurchase purchase)
        {
            _purchase = purchase;
            _purchase.PurchaseCompleted += OnPurchaseCompleted;
        }

        #region Forward Properties

        public bool IsInitialized => _purchase.IsInitialized;

        public IEnumerable<IProduct> Products => _purchase.Products;

        public IPurchaseValidator LocalValidator => _purchase.LocalValidator;

        public IPurchaseValidator ServerValidator => _purchase.ServerValidator;

        #endregion

        #region Forward Events

        public event Action<PurchaseInitialize> PurchaseInitialized
        {
            add => _purchase.PurchaseInitialized += value;
            remove => _purchase.PurchaseInitialized -= value;
        }

        public event Action<PurchaseError> PurchaseFailed
        {
            add => _purchase.PurchaseFailed += value;
            remove => _purchase.PurchaseFailed -= value;
        }

        public event Action<PurchaseProcess> PurchaseProcessing
        {
            add => _purchase.PurchaseProcessing += value;
            remove => _purchase.PurchaseProcessing -= value;
        }

        public event Action<PurchaseComplete> ServerPurchaseValid
        {
            add => _purchase.ServerPurchaseValid += value;
            remove => _purchase.ServerPurchaseValid -= value;
        }

        public event Action<PurchaseComplete> PurchaseCompleted;

        #endregion

        private void OnPurchaseCompleted(PurchaseComplete purchaseComplete)
        {
            var product = _purchase.Products?
                .FirstOrDefault(p => p.Id == purchaseComplete.ProductId);

            if (product != null && product.LocalizedPrice > 0)
            {
                try
                {
                    FirebaseAnalytics.LogEvent(
                        FirebaseAnalytics.EventPurchase,
                        new Parameter(FirebaseAnalytics.ParameterItemID, product.Id),
                        new Parameter(FirebaseAnalytics.ParameterCurrency, product.IsoCurrencyCode),
                        new Parameter(FirebaseAnalytics.ParameterValue, (double)product.LocalizedPrice)
                    );

                    Debug.Log(
                        $"[FirebaseRevenue] Purchase sent | " +
                        $"Product={product.Id} | " +
                        $"Price={product.LocalizedPrice} | " +
                        $"Currency={product.IsoCurrencyCode}"
                    );
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FirebaseRevenue] {e}");
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[FirebaseRevenue] Product not found: {purchaseComplete.ProductId}"
                );
            }

            PurchaseCompleted?.Invoke(purchaseComplete);
        }

        #region Forward Methods

        public void InitializePurchasing(IEnumerable<ProductData> productData)
        {
            _purchase.InitializePurchasing(productData);
        }

        public void Purchase(string productId)
        {
            _purchase.Purchase(productId);
        }

        public void RestorePurchases()
        {
            _purchase.RestorePurchases();
        }

        public SubscriptionInfo GetSubscriptionInfo(string productId)
        {
            return _purchase.GetSubscriptionInfo(productId);
        }

        #endregion
    }
}
#endif