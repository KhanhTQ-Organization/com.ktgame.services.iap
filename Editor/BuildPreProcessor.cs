using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace com.ktgame.services.iap.editor
{
	public class BuildPreProcessor : IPreprocessBuildWithReport
	{
		public int callbackOrder => 1;

		private const string PackageName = "com.ktgame.services.iap";

		public void OnPreprocessBuild(BuildReport report)
		{
			var pluginPath = Path.Combine(Application.dataPath, $"Plugins/Ktgame/Settings/{PackageName}");
			if (!Directory.Exists(pluginPath))
			{
				Directory.CreateDirectory(pluginPath);
			}

			if (AssetDatabase.IsValidFolder($"Packages/{PackageName}"))
			{
				AssetDatabase.CopyAsset($"Packages/{PackageName}/Runtime/link.xml", $"Assets/Plugins/Ktgame/Settings/{PackageName}/link.xml");
			}
		}
	}
}
