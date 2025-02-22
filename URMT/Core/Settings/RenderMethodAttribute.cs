using System;

namespace URMT.Core.Settings {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RenderMethodAttribute : Attribute {
        public string FieldName { get; private set; }
        public RenderMethodAttribute(string methodName) {
            FieldName = methodName;
        }
    }
}
