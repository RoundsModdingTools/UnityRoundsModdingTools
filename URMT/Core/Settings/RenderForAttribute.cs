using System;

namespace URMT.Core.Settings {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RenderForAttribute : Attribute {
        public string FieldName { get; private set; }
        public RenderForAttribute(string methodName) {
            FieldName = methodName;
        }
    }
}
