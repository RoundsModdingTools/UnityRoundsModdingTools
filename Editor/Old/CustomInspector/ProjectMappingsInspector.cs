using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityRoundsModdingTools.Editor.ScriptableObjects;

namespace UnityRoundsModdingTools.Editor.CustomInspector {
    [CustomEditor(typeof(ProjectMappings))]
    public class ProjectMappingsInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if(GUILayout.Button("Recompile")) {
                var editorAssembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
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
