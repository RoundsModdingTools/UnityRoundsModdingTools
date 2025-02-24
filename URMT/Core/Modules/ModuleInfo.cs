using System.Linq;
using URMT.Core.Modules;

namespace URMT.Core {
    public class ModuleInfo {
        public IModuleEntry ModuleEntry { get; }
        public string Name { get; }
        public string ID { get; }
        public string[] Dependencies { get; }

        public ModuleInfo(IModuleEntry moduleEntry) {
            ModuleEntry = moduleEntry;
            Name = moduleEntry.GetType().GetCustomAttributes(typeof(URMTModuleAttribute), false).Cast<URMTModuleAttribute>().First().Name;
            ID = moduleEntry.GetType().GetCustomAttributes(typeof(URMTModuleAttribute), false).Cast<URMTModuleAttribute>().First().GUID;
            Dependencies = moduleEntry.GetType().GetCustomAttributes(typeof(URMTModuleDependencyAttribute), false).Cast<URMTModuleDependencyAttribute>().Select(x => x.GUID).ToArray();
        }
    }
}
