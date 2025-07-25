using System.IO;
using System.Reflection;
using UnityEngine;
using Logger = Modding.Logger;

namespace PaleCourtCharms
{
    public static class ABManager
    {
        private static readonly Assembly asm = Assembly.GetExecutingAssembly();

        public static AssetBundle Charms { get; private set; }
        public static AssetBundle CharmUnlock { get; private set; }

        public static void LoadAll()
        {
            if (Charms == null)
                Charms = LoadBundle("PaleCourtCharms.assets.pureamulets", "Charms");

            if (CharmUnlock == null)
                CharmUnlock = LoadBundle("PaleCourtCharms.assets.charmunlock", "CharmUnlock");
        }

        public static T LoadFromCharms<T>(string assetName) where T : Object =>
            LoadAsset<T>(Charms, assetName, "Charms");

        public static T LoadFromUnlocks<T>(string assetName) where T : Object =>
            LoadAsset<T>(CharmUnlock, assetName, "CharmUnlock");

        public static void UnloadAll(bool unloadAll = false)
        {
            if (Charms != null)
            {
                Charms.Unload(unloadAll);
                Charms = null;
                Logger.Log("[PaleCourtCharms] Unloaded Charms AssetBundle.");
            }

            if (CharmUnlock != null)
            {
                CharmUnlock.Unload(unloadAll);
                CharmUnlock = null;
                Logger.Log("[PaleCourtCharms] Unloaded CharmUnlock AssetBundle.");
            }
        }
        public static T LoadAsset<T>(AssetBundle bundle, string assetName, string label) where T : Object
        {
            if (bundle == null)
            {
                Logger.LogError($"[PaleCourtCharms] Tried to load asset '{assetName}' from {label}, but bundle is null.");
                return null;
            }

            T asset = bundle.LoadAsset<T>(assetName);
            if (asset == null)
                Logger.LogError($"[PaleCourtCharms] Asset '{assetName}' not found in {label} bundle.");

            return asset;
        }

        private static AssetBundle LoadBundle(string resourceName, string label)
        {
            Stream s = asm.GetManifestResourceStream(resourceName);
            if (s == null)
            {
                Logger.LogError($"[PaleCourtCharms] Could not find embedded resource: {resourceName}");
                return null;
            }

            AssetBundle bundle = AssetBundle.LoadFromStream(s);
            s.Dispose();

            if (bundle == null)
                Logger.LogError($"[PaleCourtCharms] Failed to load AssetBundle: {label}");
            else
                Logger.Log($"[PaleCourtCharms] Loaded AssetBundle: {label}");

            return bundle;
        }
    }
}
