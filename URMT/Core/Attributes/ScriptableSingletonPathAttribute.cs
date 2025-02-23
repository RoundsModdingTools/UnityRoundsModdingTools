using System;

namespace URMT.Core.Attributes {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class ScriptableSingletonPathAttribute : Attribute {
        public string Path { get; private set; }
        public ScriptableSingletonPathAttribute(string path) {
            Path = path;
        }
    }
}
