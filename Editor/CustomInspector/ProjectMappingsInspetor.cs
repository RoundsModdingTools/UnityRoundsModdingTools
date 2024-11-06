using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityRoundsModdingTools.ScriptableObjects;

namespace UnityRoundsModdingTools.CustomInspector {
    [CustomEditor(typeof(ProjectMappings))]
    public class ProjectMappingsInspetor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(GUILayout.Button("Recompile")) {
                var editorAssembly = Assembly.GetAssembly(typeof(Editor));
                var editorCompilationInterfaceType = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
                var dirtyAllScriptsMethod = editorCompilationInterfaceType.GetMethod("DirtyAllScripts", BindingFlags.Static | BindingFlags.Public);
                dirtyAllScriptsMethod.Invoke(editorCompilationInterfaceType, null);

                var SyncVSType = editorAssembly.GetType("UnityEditor.SyncVS");
                var SyncSolutionMethod = SyncVSType.GetMethod("SyncIfFirstFileOpenSinceDomainLoad", BindingFlags.Static | BindingFlags.Public);
                SyncSolutionMethod.Invoke(editorCompilationInterfaceType, null);
            }
        }
    }
}
