using System;

namespace URMT.General.Entities {
    [Serializable]
    public struct ModBundleMapping {
        public string ModName;
        public string AssetBundleName;
        public ModBundleMapping(string modName, string assetBundleName) {
            ModName = modName;
            AssetBundleName = assetBundleName;
        }
    }
}
