using com.ktgame.core.editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.ktgame.services.iap.editor
{
    [InitializeOnLoad]
    public class IapEditorModule : IEditorDirtyHandler, IMenuTreeExtension
    {
        static IapEditorModule()
        {
            var module = new IapEditorModule();
            EditorDirtyRegistry.Register(module);
            MenuTreeExtensionRegistry.Register(module);
        }
		
        public void SetDirty()
        {
            var instance = PurchaseServiceSettings.Instance;
            if (instance != null)
            {
                EditorUtility.SetDirty(instance);
            }
        }
        public void BuildMenu(OdinMenuTree tree)
        {
            tree.Add("Purchase", new IapEditor(KTWindow.Setting), KTEditor.GetIconComponent("purchase"));
        }
    }
}
