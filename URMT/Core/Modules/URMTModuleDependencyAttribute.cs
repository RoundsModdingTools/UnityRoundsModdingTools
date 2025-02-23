using System;

namespace URMT.Core.Modules {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class URMTModuleDependencyAttribute : Attribute {
        public string GUID { get; }
        public URMTModuleDependencyAttribute(string guid) {
            GUID = guid;
        }
    }
}
