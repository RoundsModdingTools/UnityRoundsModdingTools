using System;

namespace URMT.Core.Settings {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class IgnoreInSettingAttribute : Attribute { }
}
