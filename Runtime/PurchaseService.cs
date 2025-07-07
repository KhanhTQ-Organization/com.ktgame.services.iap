using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using com.ktgame.core;
using com.ktgame.iap.core;
using com.ktgame.services.iap;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if UNITY_PURCHASE
using com.ktgame.iap.unity;
#endif

#if SERVER_PURCHASE
using com.ktgame.iap.server;
#endif

#if ADJUST_ANALYTICS
using com.ktgame.iap.extensions.adjust_revenue;
#endif

#if FACEBOOK_ANALYTICS
using com.ktgame.iap.extensions.facebook_revenue;
#endif

namespace com.ktgame.services.iap
{
    [Service(typeof(IPurchaseService))]
    public class PurchaseService : MonoBehaviour, IPurchaseService
    {
        public int Priority => 0;
        public bool Initialized { get; set; }
        public event Action<PurchaseError> PurchaseFailed;
        public event Action<PurchaseComplete> PurchaseCompleted;
        public event Action<PurchaseProcess> PurchaseProcessing;
        public event Action<PurchaseComplete> ServerPurchaseValid;

        private IPurchase _purchase;
#if SERVER_PURCHASE
        private ServerPurchase _serverPurchase;
#endif
        private PurchaseServiceSettings _settings;
        private IDictionary<string, string> _currencySymbols;

        public UniTask OnInitialize(IArchitecture architecture)
        {
            _currencySymbols = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(ri => ri != null)
                .GroupBy(ri => ri.ISOCurrencySymbol)
                .ToDictionary(x => x.Key, x => x.First().CurrencySymbol);

            _settings = PurchaseServiceSettings.Instance;

#if UNITY_PURCHASE && !UNITY_EDITOR
#if UNITY_ANDROID
            if (_settings.GoogleTangleObfuscate == null || _settings.GoogleTangleObfuscate.Length <= 0)
            {
                Debug.LogError($"[{GetType().Name}] GoogleTangleObfuscate Required!");
                return UniTask.CompletedTask;
            }
#elif UNITY_IOS
            if (_settings.AppleTangleObfuscate == null || _settings.AppleTangleObfuscate.Length <= 0)
            {
                Debug.LogError($"[{GetType().Name}] AppleTangleObfuscate Required!");
                return UniTask.CompletedTask;
            }
#endif

            IPurchaseValidator serverValidator = null;
#if SERVER_PURCHASE
            serverValidator = new ServerPurchaseValidator();    
#endif
            
            var localValidator = new UnityPurchaseValidator(_settings.GoogleTangleObfuscate, _settings.AppleTangleObfuscate);
            _purchase = new UnityPurchase(localValidator, serverValidator);
            
#if SERVER_PURCHASE
            if (!string.IsNullOrEmpty(_settings.ServerAppId))
            {
                _serverPurchase = new ServerPurchase(_settings.ServerAppId, _purchase);
                _purchase = _serverPurchase;
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] ServerAppId Required!");
                return UniTask.CompletedTask;
            }
#endif
#else
            _purchase = new MockupPurchase();
#endif

#if ADJUST_ANALYTICS
            if (!string.IsNullOrEmpty(_settings.AdjustEventName))
            {
                _purchase = new AdjustPurchaseMeasureRevenue(_settings.AdjustEventName, _purchase);
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] AdjustEventName Required!");
                return UniTask.CompletedTask;
            }
#endif

#if FACEBOOK_ANALYTICS
            _purchase = new FacebookPurchaseMeasureRevenue(_purchase);
#endif

            _purchase.PurchaseInitialized += OnPurchaseInitialized;
            _purchase.PurchaseFailed += OnPurchaseFailed;
            _purchase.PurchaseProcessing += OnPurchaseProcessing;
            _purchase.PurchaseCompleted += OnPurchaseCompleted;
            _purchase.ServerPurchaseValid += OnServerPurchaseValid;
            _purchase.InitializePurchasing(_settings.ProductData);
            return UniTask.CompletedTask;
        }

        private void OnPurchaseInitialized(PurchaseInitialize purchaseInitialize)
        {
            Initialized = true;
            Debug.Log($"[{GetType().Name}] Initialized status {purchaseInitialize.Status}");
        }

        private void OnPurchaseFailed(PurchaseError purchaseError)
        {
            Debug.Log($"[{GetType().Name}] OnPurchaseFailed {purchaseError.ProductId}: FAILED. PurchaseFailureReason: {purchaseError.ErrorType}");
            PurchaseFailed?.Invoke(purchaseError);
        }

        private void OnPurchaseProcessing(PurchaseProcess purchaseProcess)
        {
            PurchaseProcessing?.Invoke(purchaseProcess);
        }

        private void OnPurchaseCompleted(PurchaseComplete purchaseComplete)
        {
            PurchaseCompleted?.Invoke(purchaseComplete);
        }

        private void OnServerPurchaseValid(PurchaseComplete purchaseComplete)
        {
            ServerPurchaseValid?.Invoke(purchaseComplete);
        }

        public void SetUserId(string userId)
        {
#if SERVER_PURCHASE
            _serverPurchase?.SetUserId(userId);
#endif
        }

        public void SetRequestValidateTimeOut(int timeOut)
        {
#if SERVER_PURCHASE
            _serverPurchase?.SetRequestValidateTimeOut(timeOut);
#endif
        }

        public IProduct GetProduct(string productId)
        {
            if (_purchase.IsInitialized)
            {
                return _purchase.Products.FirstOrDefault(p => p.Id == productId);
            }

            return null;
        }

        public ProductData GetProductData(string productId)
        {
            return _settings.ProductData.FirstOrDefault(p => p.Id == productId);
        }

        public SubscriptionInfo GetSubscriptionInfo(string productId)
        {
            if (_purchase.IsInitialized)
            {
                return _purchase.GetSubscriptionInfo(productId);
            }

            return null;
        }

        public decimal GetPriceDecimal(string productId)
        {
            var product = GetProduct(productId);
            if (product != null && product.LocalizedPrice > 0)
            {
                return product.LocalizedPrice;
            }

            decimal result = 1;
            decimal.TryParse(_settings.ProductData.First(p => string.Equals(p.Id, productId)).Price.Replace("$", ""), out result);
            return result;
        }

        public string GetCurrencyCode(string productId)
        {
            var product = GetProduct(productId);
            if (product != null && !string.IsNullOrEmpty(product.IsoCurrencyCode))
            {
                return _currencySymbols.TryGetValue(product.IsoCurrencyCode, out var isoCurrencyCodeSymbol) ? isoCurrencyCodeSymbol : product.IsoCurrencyCode;
            }

            return "$";
        }

        public string GetPriceString(string productId)
        {
            var product = GetProduct(productId);
            if (product != null && !string.IsNullOrEmpty(product.LocalizedPriceString))
            {
                return product.LocalizedPriceString;
            }

            return _settings.ProductData.First(p => string.Equals(p.Id, productId)).Price;
        }

        public void Purchase(string productId)
        {
            if (_purchase.IsInitialized)
            {
                _purchase.Purchase(productId);
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] Not Initialized!");
            }
        }

        public void Restore()
        {
            if (_purchase.IsInitialized)
            {
                _purchase.RestorePurchases();
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] NOt Initialized!");
            }
        }
    }
}