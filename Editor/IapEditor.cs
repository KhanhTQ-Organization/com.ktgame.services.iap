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

		[ShowInInspector]
		[LabelText("Server App Id")]
		public string ServerAppId
		{
			get => _parametersIap.ServerAppId;
			set
			{
				_parametersIap.ServerAppId = value;
				AssetDatabase.SaveAssets();
			}
		}
		
		[ShowInInspector]
		[LabelText("Adjust Event Name")]
		public string AdjustEventName
		{
			get => _parametersIap.AdjustEventName;
			set
			{
				_parametersIap.AdjustEventName = value;
				AssetDatabase.SaveAssets();
			}
		}
		
		[ShowInInspector]
		[LabelText("Google Tangle Obfuscate Key")]
		public byte[] GoogleTangleObfuscate => _parametersIap.GoogleTangleObfuscate;
		
		[ShowInInspector]
		[LabelText("Apple Tangle Obfuscate")]
		public byte[] AppleTangleObfuscate => _parametersIap.AppleTangleObfuscate;

		[PropertyOrder(-1)]
		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			// if (!IsInstalledFirebase)
			// 	RenderIfNotInstalled();
			//
			// if (GUI.changed)
			// {
			// 	EditorUtility.SetDirty(_parametersRc);
			// 	AssetDatabase.SaveAssets();
			// }
		}

		[ListDrawerSettings(CustomAddFunction = "CreateNewParameter")]
		//[HideReferenceObjectPicker]
		//[InlineProperty]
		[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
		[ShowInInspector]
		[LabelText("Purchase Config Parameters")]
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

		[Button("Obfuscate Generate")]
		private void GenerateConfig()
		{
			_parametersIap.ObfuscateGenerate();
		}

		[Button("Product Key Generate")]
		private void ProductKeysGenerate()
		{
			_parametersIap.ProductKeysGenerate();
		}
	}
}
