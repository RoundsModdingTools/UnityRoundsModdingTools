using System;

namespace URMT.Core.Modules {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class URMTModuleAttribute : Attribute {
        public string Name { get; }
        public string GUID { get; set; }

        public URMTModuleAttribute(string name, string guid) {
            Name = name;
            GUID = guid;
        }
    }
}
