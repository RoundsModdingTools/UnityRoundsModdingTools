using System.IO;
using System.Text;
using UnityEditor;

namespace UnityRoundsModdingTools.AssetPostprocessors {
    [InitializeOnLoad]
    internal static class CsprojPostprocessorDisabler {
        static CsprojPostprocessorDisabler() {
            if(!EditorPrefs.GetBool("Original_CsprojPostprocessor_Detected_RunOnce", false) && File.Exists("Assets/Editor/CsprojPostprocessor.cs")) {
                StringBuilder messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("Do you want to disable the original CsprojPostprocessor?");
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("Disabling it will allow the CsprojPostprocessor from 'UnityRoundsModdingTools' to take over.");
                messageBuilder.AppendLine("Note: This prompt will only be shown once.");

                bool shouldDisableCsprojPostprocessor = EditorUtility.DisplayDialog(
                    "Disable Original CsprojPostprocessor?",
                    messageBuilder.ToString(),
                    "Yes", "No"
                );

                EditorPrefs.SetBool("Original_CsprojPostprocessor_Detected_RunOnce", true);

                if(shouldDisableCsprojPostprocessor) {
                    File.Move("Assets/Editor/CsprojPostprocessor.cs", "Assets/Editor/CsprojPostprocessor.cs.disable");
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
