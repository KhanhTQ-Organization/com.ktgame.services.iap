using System;
using com.ktgame.core;
using com.ktgame.iap.core;

namespace com.ktgame.services.iap
{
	public interface IPurchaseService : IService, IInitializable
	{
		event Action<PurchaseError> PurchaseFailed;

		event Action<PurchaseComplete> PurchaseCompleted;

		event Action<PurchaseProcess> PurchaseProcessing;

		event Action<PurchaseComplete> ServerPurchaseValid;
        
		void SetUserId(string userId);

		void SetRequestValidateTimeOut(int timeOut);

		IProduct GetProduct(string productId);

		ProductData GetProductData(string productId);

		SubscriptionInfo GetSubscriptionInfo(string productId);

		decimal GetPriceDecimal(string productId);

		string GetCurrencyCode(string productId);

		string GetPriceString(string productId);

		void Purchase(string productId);

		void Restore();
	}
}
