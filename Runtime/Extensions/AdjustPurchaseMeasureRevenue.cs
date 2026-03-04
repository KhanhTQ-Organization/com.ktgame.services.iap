#if ADJUST_ANALYTICS
using System;
using System.Linq;
using System.Collections.Generic;
using AdjustSdk;
using com.ktgame.iap.core;

namespace com.ktgame.iap.extensions.adjust_revenue
{
    public class AdjustPurchaseMeasureRevenue : IPurchase
    {
        private readonly string _adjustEventToken;
        private readonly IPurchase _purchase;

        public AdjustPurchaseMeasureRevenue(string adjustEventToken, IPurchase purchase)
        {
            _adjustEventToken = adjustEventToken;
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
                var adjustEvent = new AdjustEvent(_adjustEventToken);

                adjustEvent.SetRevenue(
                    (double)product.LocalizedPrice,
                    product.IsoCurrencyCode
                );

                adjustEvent.AddCallbackParameter("product_id", product.Id);

                Adjust.TrackEvent(adjustEvent);
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