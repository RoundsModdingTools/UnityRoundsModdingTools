using System;
using System.IO;

namespace URMT.Core {
    public class Solution {
        public CSProj[] Projects { get; private set; }

        public Solution(string path) {
            if(path == null) throw new ArgumentNullException(nameof(path));
            if(File.Exists(path)) path = Path.GetDirectoryName(path);
            if(!Directory.Exists(path)) throw new DirectoryNotFoundException($"Directory not found at path: {path}");

            string[] csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            Projects = new CSProj[csprojFiles.Length];

            for(int i = 0; i < csprojFiles.Length; i++) {
                Projects[i] = new CSProj(csprojFiles[i]);
            }
        }
    }
}
