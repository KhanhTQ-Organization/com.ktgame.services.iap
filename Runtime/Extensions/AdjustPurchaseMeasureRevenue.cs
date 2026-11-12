#if ADJUST_ANALYTICS
using System.Linq;
using AdjustSdk;
using com.ktgame.iap.core;

namespace com.ktgame.iap.extensions.adjust_revenue
{
    public class AdjustPurchaseMeasureRevenue : PurchaseDecorator
    {
        private readonly string _adjustEventToken;

        public AdjustPurchaseMeasureRevenue(string adjustEventToken, IPurchase purchase) : base(purchase)
        {
            _adjustEventToken = adjustEventToken;
        }

        protected override void PurchaseCompletedHandler(PurchaseComplete purchaseComplete)
        {
            if (!purchaseComplete.IsRestored)
            {
                var product = Purchaser.Products?
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
            }

            base.PurchaseCompletedHandler(purchaseComplete);
        }
    }
}
#endif
