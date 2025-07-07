using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace com.ktgame.services.iap.editor
{
    internal static class PackageInstaller
    {
        private const string PackageName = "com.ktgame.services.iap";

        [MenuItem("Ktgame/Services/Settings/Purchase")]
        private static void SelectionSettings()
        {
            Selection.activeObject = PurchaseServiceSettings.Instance;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            Events.registeredPackages += RegisteredPackagesEventHandler;
            Events.registeringPackages += RegisteringPackagesEventHandler;
        }

        private static void RegisteredPackagesEventHandler(PackageRegistrationEventArgs args)
        {
            var addedPackage = args.added.FirstOrDefault(package => package.displayName.Equals(PackageName));
            if (addedPackage != null)
            {
                InstallPackage();
            }
        }

        private static void RegisteringPackagesEventHandler(PackageRegistrationEventArgs args)
        {
            var removedPackage = args.removed.FirstOrDefault(package => package.displayName.Equals(PackageName));
            if (removedPackage != null)
            {
                UninstallPackage();
            }
        }

        private static void InstallPackage()
        {
            // CreateGoogleTangleObfuscateFile();
            // CreateAppleTangleObfuscateFile();
            // RemovePackageRuntimeFolder();
        }

        private static void UninstallPackage()
        {
            // RemoveProjectFolder();
        }

        private static void CreateGoogleTangleObfuscateFile()
        {
            var destFolder = Path.Combine(Application.dataPath, "Plugins", "Ktgame", "Settings", PackageName);
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            var srcGoogleTangleObfuscateFile = Path.Combine(GetPackagePath(PackageName), "Runtime", "GoogleTangleObfuscate.cs");
            var destGoogleTangleObfuscateFile = Path.Combine(destFolder, "GoogleTangleObfuscate.cs");

            if (!File.Exists(destGoogleTangleObfuscateFile))
            {
                File.Copy(srcGoogleTangleObfuscateFile, destGoogleTangleObfuscateFile);
            }
        }

        private static void CreateAppleTangleObfuscateFile()
        {
            var destFolder = Path.Combine(Application.dataPath, "Plugins", "Ktgame", "Settings", PackageName);
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            var srcAppleTangleObfuscateFile = Path.Combine(GetPackagePath(PackageName), "Runtime", "AppleTangleObfuscate.cs");
            var destAppleTangleObfuscateFile = Path.Combine(destFolder, "AppleTangleObfuscate.cs");

            if (!File.Exists(destAppleTangleObfuscateFile))
            {
                File.Copy(srcAppleTangleObfuscateFile, destAppleTangleObfuscateFile);
            }
        }

        private static void RemovePackageRuntimeFolder()
        {
            var packageRuntimeFolder = Path.Combine(GetPackagePath(PackageName), "Runtime");
            if (!Directory.Exists(packageRuntimeFolder)) return;

            var googleTangleObfuscateFile = Path.Combine(packageRuntimeFolder, "GoogleTangleObfuscate.cs");
            var googleTangleObfuscateMetaFile = Path.Combine(packageRuntimeFolder, "GoogleTangleObfuscate.cs.meta");
            if (File.Exists(googleTangleObfuscateFile))
            {
                File.Delete(googleTangleObfuscateFile);
                File.Delete(googleTangleObfuscateMetaFile);
            }

            var appleTangleObfuscateFile = Path.Combine(packageRuntimeFolder, "AppleTangleObfuscate.cs");
            var appleTangleObfuscateMetaFile = Path.Combine(packageRuntimeFolder, "AppleTangleObfuscate.cs.meta");
            if (File.Exists(appleTangleObfuscateFile))
            {
                File.Delete(appleTangleObfuscateFile);
                File.Delete(appleTangleObfuscateMetaFile);
            }
        }

        private static void RemoveProjectFolder()
        {
            var projFolder = Path.Combine(Application.dataPath, "Plugins", "Ktgame", "Settings", PackageName);
            if (!Directory.Exists(projFolder)) return;

            var googleTangleObfuscateFile = Path.Combine(projFolder, "GoogleTangleObfuscate.cs");
            var googleTangleObfuscateMetaFile = Path.Combine(projFolder, "GoogleTangleObfuscate.cs.meta");
            if (File.Exists(googleTangleObfuscateFile))
            {
                File.Delete(googleTangleObfuscateFile);
                File.Delete(googleTangleObfuscateMetaFile);
            }

            var appleTangleObfuscateFile = Path.Combine(projFolder, "AppleTangleObfuscate.cs");
            var appleTangleObfuscateMetaFile = Path.Combine(projFolder, "AppleTangleObfuscate.cs.meta");
            if (File.Exists(appleTangleObfuscateFile))
            {
                File.Delete(appleTangleObfuscateFile);
                File.Delete(appleTangleObfuscateMetaFile);
            }
        }

        private static string GetPackagePath(string packageId)
        {
            var projectRootPath = GetProjectRootPath();
            var packageRootPath = Path.Combine(projectRootPath, "Library/PackageCache");
            var packageRootDir = new DirectoryInfo(packageRootPath);
            if (packageRootDir == null)
            {
                throw new DirectoryNotFoundException($"ERROR::: Path does not exist: {packageRootPath}.");
            }

            foreach (var dirInfo in packageRootDir.GetDirectories())
            {
                if (!dirInfo.Name.Contains(packageId)) continue;
                return dirInfo.FullName;
            }

            throw new DirectoryNotFoundException($"ERROR::: Package {packageId} not found at {packageRootPath}");
        }

        private static string GetProjectRootPath()
        {
            var projectFolderDir = new DirectoryInfo(Application.dataPath).Parent;
            if (projectFolderDir == null)
            {
                throw new DirectoryNotFoundException($"ERROR::: Path does not exist: {Application.dataPath}.");
            }

            return projectFolderDir.FullName;
        }
    }
}