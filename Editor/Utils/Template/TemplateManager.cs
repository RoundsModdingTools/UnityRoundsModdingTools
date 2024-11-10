using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using UnityEditor.Compilation;
using System.Reflection;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace UnityRoundsModdingTools.Editor.Utils.Template {
    public interface ITemplateHandler {
        string HandleTemplate(string content);
        void InitTemplate(params object[] args);
        void AfterTemplateCompile();
    }


    [InitializeOnLoad]
    public static class TemplateManager {
        private static readonly Dictionary<string, ITemplateHandler> templateHandlers = new Dictionary<string, ITemplateHandler>();

        static TemplateManager() {
            RegisterTemplateHandler("mod", new ModTemplateHandler());

            foreach(var handlerEntry in new Dictionary<string, ITemplateHandler>(templateHandlers)) {
                string serializedArgs = EditorPrefs.GetString($"Template_{handlerEntry.Key}_Serialized");
                if(string.IsNullOrEmpty(serializedArgs)) continue;

                var templateHandlerType = handlerEntry.Value.GetType();
                templateHandlers[handlerEntry.Key] = (ITemplateHandler)JsonConvert.DeserializeObject(serializedArgs, templateHandlerType);
                try {
                    templateHandlers[handlerEntry.Key].AfterTemplateCompile();
                } catch (Exception e) {
                    UnityEngine.Debug.LogError($"Error while running AfterTemplateCompile for '{handlerEntry.Key}': {e}");
                }
                EditorPrefs.DeleteKey($"Template_{handlerEntry.Key}_Serialized");
            }
        }

        public static void RegisterTemplateHandler(string key, ITemplateHandler templateHandler) {
            templateHandlers[key] = templateHandler;
        }

        public static void ApplyTemplateToFiles(string sourcePath, string destinationPath, string handlerKey, params object[] args) {
            if(sourcePath != destinationPath) FileSystemManager.CopyDirectory(sourcePath, destinationPath);

            templateHandlers[handlerKey].InitTemplate(args);

            var processedTemplates = ProcessTemplatesInFiles(destinationPath, handlerKey);
            foreach(var fileEntry in processedTemplates) {
                File.WriteAllText(ApplyTemplate(handlerKey, 
                    Path.Combine(Directory.GetParent(Path.GetFullPath(fileEntry.Key)).FullName, 
                    Path.GetFileNameWithoutExtension(fileEntry.Key))
                ), fileEntry.Value);

                File.Delete(fileEntry.Key);
            }

            // Save the arguments to EditorPrefs so they can be retrieved later
            EditorPrefs.SetString($"Template_{handlerKey}_Serialized", JsonConvert.SerializeObject(templateHandlers[handlerKey]));
        }

        private static Dictionary<string, string> ProcessTemplatesInFiles(string path, string handlerKey) {
            string[] templateFiles = Directory.GetFiles(path, "*.template", SearchOption.AllDirectories);
            return templateFiles.ToDictionary(file => file, file => ApplyTemplate(handlerKey, File.ReadAllText(file)));
        }

        private static string ApplyTemplate(string handlerKey, string content) {
            if(!templateHandlers.ContainsKey(handlerKey)) {
                return content;
            }

            string processedContent = templateHandlers[handlerKey].HandleTemplate(content);
            return processedContent;
        }
    }
}
