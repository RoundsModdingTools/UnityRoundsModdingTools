using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace URMT.Core {
    public class CSProj {
        public string Path { get; private set; }

        public string RootNamespace { get; private set; }
        public string AssemblyName { get; private set; }
        public string[] References { get; private set; }

        public string Name {
            get {
                if(!string.IsNullOrEmpty(RootNamespace)) {
                    return RootNamespace;
                } else if(!string.IsNullOrEmpty(AssemblyName)) {
                    return AssemblyName;
                } else if(!string.IsNullOrEmpty(Path)) {
                    return System.IO.Path.GetFileNameWithoutExtension(Path);
                } else {
                    return null;
                }
            }
        }

        private string _content;
        public string Content {
            get {
                if(string.IsNullOrEmpty(_content)) {
                    _content = System.IO.File.ReadAllText(Path);
                }
                return _content;
            }
            set {
                RootNamespace = GetRootNamespace(value);
                _content = value;
            }
        }

        public CSProj(string path) {
            Path = path;

            RootNamespace = GetRootNamespace(Content);
            AssemblyName = GetAssemblyName(Content);
            References = GetReferencesFromCsproj(Content).ToArray();
        }



        private string GetRootNamespace(string content) {
            string pattern = @"<RootNamespace>(.*?)<\/RootNamespace>";
            Match match = Regex.Match(content, pattern);

            if(match.Groups[1].Value == "") return null;
            return match.Groups[1].Value;
        }

        private string GetAssemblyName(string content) {
            string pattern = @"<AssemblyName>(.*?)<\/AssemblyName>";
            Match match = Regex.Match(content, pattern);

            if(match.Groups[1].Value == "") return null;
            return match.Groups[1].Value;
        }

        private List<string> GetReferencesFromCsproj(string content) {
            List<string> references = new List<string>();
            string pattern = @"<(PackageReference|Reference|ProjectReference) Include=""([^"",]+).*""\s*\/?>";
            MatchCollection matches = Regex.Matches(content, pattern);

            foreach(Match match in matches) {
                string reference = match.Groups[2].Value.Split('/').Last().Replace(".csproj", "");
                references.Add(reference);
            }

            return references;
        }
    }
}
