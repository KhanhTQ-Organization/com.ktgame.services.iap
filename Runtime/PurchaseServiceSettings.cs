using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using com.ktgame.core;
using com.ktgame.iap.core;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.ktgame.services.iap
{
    public class PurchaseServiceSettings : ServiceSettingsSingleton<PurchaseServiceSettings>
    {
        public override string PackageName => GetType().Namespace;

        [SerializeField] private string serverAppId;

        [SerializeField] private string adjustEventName;

        [SerializeField] private string androidBase64;

        [SerializeField, ReadOnly] private byte[] googleTangleObfuscate;

        [SerializeField, ReadOnly] private byte[] appleTangleObfuscate;

        [SerializeField] private List<ProductData> productData;

        public string ServerAppId => serverAppId;

        public string AdjustEventName => adjustEventName;

        public byte[] GoogleTangleObfuscate => googleTangleObfuscate;

        public byte[] AppleTangleObfuscate => appleTangleObfuscate;

        public List<ProductData> ProductData => productData ?? new List<ProductData>();

#if UNITY_EDITOR
        private const string AppleCertPath = "Packages/com.ktgame.services.iap/Editor/AppleIncRootCertificate.cer";

        [Button("Obfuscate Generate")]
        private void ObfuscateGenerate()
        {
            WriteObfuscatedAppleClassAsAsset();
            WriteObfuscatedGooglePlayClassAsAsset(androidBase64);
            AssetDatabase.Refresh();
        }

        [Button("Product Key Generate")]
        private void ProductKeysGenerate()
        {
            if (productData.Count <= 0) return;
            var builder = new StringBuilder();
            builder.AppendFormat("namespace {0}", PackageName).Append("\n").Append("{").Append("\n");
            builder.Append("\t").Append("public class PurchaseProductKey").Append("\n");
            builder.Append("\t").Append("{").Append("\n");
            foreach (var product in productData)
            {
                var productId = product.Id;
                if (productId.Contains("."))
                {
                    productId = productId.Substring(productId.LastIndexOf('.') + 1);
                }

                var productKey = FirstCharToUpperRegex(productId);
                builder.Append("\t\t").AppendFormat("public const string {0}", productKey).Append(" = ").Append("\"").Append(product.Id).Append("\"")
                    .Append(";").Append("\n");
            }

            builder.Append("\t").Append("}").Append("\n");
            builder.Append("}").Append("\n");
            var fileText = builder.ToString();

            var saveFolderPath = Path.Combine(Application.dataPath, "Scripts/Generated");
            var saveFilePath = Path.Combine(saveFolderPath, "PurchaseProductGenerate.cs");

            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }

            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            if (File.Exists(saveFilePath + ".meta"))
            {
                File.Delete(saveFilePath + ".meta");
            }

            File.WriteAllText(saveFilePath, fileText, Encoding.UTF8);
            AssetDatabase.ImportAsset(saveFilePath);
            AssetDatabase.Refresh();
        }

        private void WriteObfuscatedAppleClassAsAsset()
        {
            var key = 0;
            var order = new int[0];
            var tangled = new byte[0];
            try
            {
                var bytes = File.ReadAllBytes(AppleCertPath);
                order = new int[bytes.Length / 20 + 1];
                tangled = Obfuscator.Obfuscate(bytes, order, out key);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Invalid Apple Root Certificate. Generating incomplete credentials file. " + e);
            }

            appleTangleObfuscate = null;
            appleTangleObfuscate = Obfuscator.DeObfuscate(tangled, order, key);
        }

        private void WriteObfuscatedGooglePlayClassAsAsset(string googlePlayPublicKey)
        {
            var key = 0;
            var order = new int[0];
            var tangled = new byte[0];
            try
            {
                var bytes = Convert.FromBase64String(googlePlayPublicKey);
                order = new int[bytes.Length / 20 + 1];
                tangled = Obfuscator.Obfuscate(bytes, order, out key);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Invalid Google Play Public Key. Generating incomplete credentials file. " + e);
            }

            googleTangleObfuscate = null;
            googleTangleObfuscate = Obfuscator.DeObfuscate(tangled, order, key);
        }

        private string FirstCharToUpperRegex(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return System.Text.RegularExpressions.Regex.Replace(input, "^[a-z]", c => c.Value.ToUpper());
        }
#endif
    }
}