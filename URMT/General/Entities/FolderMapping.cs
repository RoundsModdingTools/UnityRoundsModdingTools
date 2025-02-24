using System;

namespace URMT.General.Entities {
    [Serializable]
    public struct FolderMapping {
        public string AssemblyName;
        public string FolderName;

        public FolderMapping(string assemblyName, string folderName) {
            AssemblyName = assemblyName;
            FolderName = folderName;
        }
    }
}
