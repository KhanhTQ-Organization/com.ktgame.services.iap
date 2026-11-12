#if FIREBASE_ANALYTICS

using UnityEngine;
using System;
using System.Linq;
using Firebase.Analytics;
using com.ktgame.iap.core;

namespace com.ktgame.iap.extensions.firebase_revenue
{
    public class FirebasePurchaseMeasureRevenue : PurchaseDecorator
    {
        public FirebasePurchaseMeasureRevenue(IPurchase purchase) : base(purchase)
        {
        }

        protected override void PurchaseCompletedHandler(PurchaseComplete purchaseComplete)
        {
            if (!purchaseComplete.IsRestored)
            {
                LogRevenue(purchaseComplete);
            }

            base.PurchaseCompletedHandler(purchaseComplete);
        }

        private void LogRevenue(PurchaseComplete purchaseComplete)
        {
            var product = Purchaser.Products?
                .FirstOrDefault(p => p.Id == purchaseComplete.ProductId);

            if (product != null && product.LocalizedPrice > 0)
            {
                try
                {
                    FirebaseAnalytics.LogEvent(
                        FirebaseAnalytics.EventPurchase,
                        new Parameter("item_id", product.Id),
                        new Parameter("currency", product.IsoCurrencyCode),
                        new Parameter("value", (double)product.LocalizedPrice),
                        new Parameter("transaction_id", purchaseComplete.TransactionId ?? string.Empty)
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
        }
    }
}
#endif
