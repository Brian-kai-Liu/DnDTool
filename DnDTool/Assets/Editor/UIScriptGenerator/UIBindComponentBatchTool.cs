using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TEngine.Editor.UI;
using UnityEditor;
using UnityEngine;

namespace GameLogic
{
    internal static class UIBindComponentBatchTool
    {
        public static void RegenerateFromCommandLine()
        {
            string prefabPath = GetCommandLineArgument("-prefabPath") ?? GetCommandLineArgument("--prefabPath");
            string outputDir = GetCommandLineArgument("-outputDir") ?? GetCommandLineArgument("--outputDir");
            string reportPath = GetCommandLineArgument("-reportPath") ?? GetCommandLineArgument("--reportPath");
            bool savePrefab = ParseBooleanArgument(GetCommandLineArgument("-savePrefab") ?? GetCommandLineArgument("--savePrefab"));

            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                throw new ArgumentException("缺少参数 -prefabPath");
            }

            if (string.IsNullOrWhiteSpace(outputDir))
            {
                outputDir = "Temp/BatchGeneratorOutput";
            }

            if (string.IsNullOrWhiteSpace(reportPath))
            {
                reportPath = "Temp/BatchGeneratorOutput/ChapterEditorUI_report.txt";
            }

            RegeneratePrefab(prefabPath, outputDir, reportPath, savePrefab);
        }

        public static void RegeneratePrefab(string prefabPath, string outputDir, string reportPath, bool savePrefab = false)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                throw new InvalidOperationException($"无法加载 Prefab: {prefabPath}");
            }

            try
            {
                UIBindComponent[] bindComponents = prefabRoot.GetComponentsInChildren<UIBindComponent>(true);
                if (bindComponents == null || bindComponents.Length == 0)
                {
                    throw new InvalidOperationException($"Prefab 中未找到 UIBindComponent: {prefabPath}");
                }

                List<string> reportLines = new List<string>
                {
                    $"Prefab: {prefabPath}",
                    $"OutputDir: {outputDir}",
                    $"BindComponentCount: {bindComponents.Length}",
                };

                for (int index = 0; index < bindComponents.Length; index++)
                {
                    UIBindComponent bindComponent = bindComponents[index];
                    Rebind(bindComponent);
                    IReadOnlyList<Component> components = GetBoundComponents(bindComponent);
                    reportLines.Add($"[{index}] {GetTransformPath(bindComponent.transform)} -> {components.Count}");
                    for (int componentIndex = 0; componentIndex < components.Count; componentIndex++)
                    {
                        Component component = components[componentIndex];
                        string componentName = component != null ? component.GetType().Name : "null";
                        string gameObjectName = component != null ? component.gameObject.name : "null";
                        reportLines.Add($"  {componentIndex}: {gameObjectName} ({componentName})");
                    }
                }

                UIBindComponent rootBindComponent = prefabRoot.GetComponent<UIBindComponent>();
                if (rootBindComponent == null)
                {
                    throw new InvalidOperationException($"根节点缺少 UIBindComponent: {prefabPath}");
                }

                Selection.activeGameObject = prefabRoot;
                ScriptGenerator.GenerateCSharpScript(
                    true,
                    false,
                    true,
                    outputDir,
                    rootBindComponent.className,
                    rootBindComponent.uiType,
                    false,
                    GetHiddenString(rootBindComponent, "impCodePath"));

                if (savePrefab)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }

                string reportDirectory = Path.GetDirectoryName(reportPath);
                if (!string.IsNullOrWhiteSpace(reportDirectory) && !Directory.Exists(reportDirectory))
                {
                    Directory.CreateDirectory(reportDirectory);
                }

                File.WriteAllLines(reportPath, reportLines);
                AssetDatabase.Refresh();
                Debug.Log($"UIBindComponent batch regenerate finished. Prefab={prefabPath}, OutputDir={outputDir}, Report={reportPath}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void Rebind(UIBindComponent bindComponent)
        {
            Selection.activeGameObject = bindComponent.gameObject;
            bindComponent.Clear();
            ScriptGenerator.GenerateUIComponentScript();
        }

        private static IReadOnlyList<Component> GetBoundComponents(UIBindComponent bindComponent)
        {
            FieldInfo fieldInfo = typeof(UIBindComponent).GetField("m_components", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                throw new MissingFieldException(typeof(UIBindComponent).FullName, "m_components");
            }

            return fieldInfo.GetValue(bindComponent) as List<Component> ?? new List<Component>();
        }

        private static string GetHiddenString(UIBindComponent bindComponent, string fieldName)
        {
            FieldInfo fieldInfo = typeof(UIBindComponent).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return string.Empty;
            }

            return fieldInfo.GetValue(bindComponent) as string ?? string.Empty;
        }

        private static string GetTransformPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private static string GetCommandLineArgument(string argumentName)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (string.Equals(arguments[index], argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }

            return null;
        }

        private static bool ParseBooleanArgument(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.Equals("1", StringComparison.OrdinalIgnoreCase)
                || value.Equals("true", StringComparison.OrdinalIgnoreCase)
                || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }
    }
}