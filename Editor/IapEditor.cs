using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using com.ktgame.core.editor;
using com.ktgame.iap.core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace com.ktgame.services.iap.editor
{
	public class IapEditor
	{
		private static bool _isInstalled = false;
		private KTSettingSO _setting;
		private PurchaseServiceSettings _parametersIap;
		private bool IsInstalledFirebase => DefineSymbolsEditor.HasDefineSymbol(DefineSymbolName.DS_FIREBASE_INSTALLED);

		public IapEditor(KTSettingSO setting)
		{
			_setting = setting;
			_parametersIap = PurchaseServiceSettings.Instance;
		}

		[Title("In-App Purchase SDK", "Manage your monetization and product configurations.", TitleAlignments.Centered)]
		[PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
		[InfoBox("Configure your IAP settings, security obfuscation, and define the products available for purchase in your game.", InfoMessageType.Info)]
		
		[BoxGroup("Analytics Integration", CenterLabel = true)]
		[LabelText("Server App Id")]
		[Tooltip("Your application's ID on the server.")]
		[ShowInInspector]
		[OnValueChanged("MarkDirty")]
		public string ServerAppId
		{
			get => _parametersIap.ServerAppId;
			set => _parametersIap.ServerAppId = value;
		}
		
		[BoxGroup("Analytics Integration")]
		[LabelText("Adjust Event Name")]
		[Tooltip("The event name mapped to successful purchases in Adjust.")]
		[ShowInInspector]
		[OnValueChanged("MarkDirty")]
		public string AdjustEventName
		{
			get => _parametersIap.AdjustEventName;
			set => _parametersIap.AdjustEventName = value;
		}

		[BoxGroup("Security & Obfuscation", CenterLabel = true)]
		[PropertySpace(SpaceBefore = 10)]
		[LabelText("Google Tangle Key")]
		[ShowInInspector, ReadOnly]
		public byte[] GoogleTangleObfuscate => _parametersIap.GoogleTangleObfuscate;
		
		[BoxGroup("Security & Obfuscation")]
		[LabelText("Apple Tangle Key")]
		[ShowInInspector, ReadOnly]
		public byte[] AppleTangleObfuscate => _parametersIap.AppleTangleObfuscate;

		[BoxGroup("Security & Obfuscation")]
		[PropertySpace(SpaceAfter = 10)]
		[Button("Generate Obfuscation Keys", ButtonSizes.Medium)]
		private void GenerateConfig()
		{
			_parametersIap.ObfuscateGenerate();
			MarkDirty();
		}

		[PropertyOrder(-1)]
		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			// RenderIfNotInstalled();
		}

		[PropertySpace(SpaceBefore = 10)]
		[ListDrawerSettings(CustomAddFunction = "CreateNewParameter")]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		[ShowInInspector]
		[LabelText("Product Catalog")]
		[OnValueChanged("MarkDirty")]
		public List<ProductData> Parameters
		{
			get => _parametersIap.ProductData ?? new List<ProductData>();
			set => _parametersIap.ProductData = value;
		}

		private ProductData CreateNewParameter()
		{
			return new ProductData
			{
				Id = "",
				Type = PurchaseType.Consumable,
				Price = "",
			};
		}

		[PropertySpace(SpaceBefore = 15)]
		[Button("Generate Product Keys (Code)", ButtonSizes.Large, Icon = SdfIconType.CodeSlash), GUIColor(0.2f, 0.8f, 0.2f)]
		private void ProductKeysGenerate()
		{
			_parametersIap.ProductKeysGenerate();
			AssetDatabase.Refresh();
		}

		private void MarkDirty()
		{
			if (_parametersIap != null)
			{
				EditorUtility.SetDirty(_parametersIap);
			}
		}
	}
}
