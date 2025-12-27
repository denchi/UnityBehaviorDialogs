using UnityEngine;
using UnityEditor;
using Behaviours.Dialogs;
using System.IO;

namespace Behaviours.Dialogs.Editor
{
    /// <summary>
    /// Editor utility for exporting and importing Dialog assets to/from JSON
    /// </summary>
    public class DialogJsonEditorUtility
    {
        [MenuItem("Assets/Dialog JSON/Export to JSON", validate = true)]
        public static bool ValidateExportDialog()
        {
            return Selection.activeObject is Dialog;
        }

        [MenuItem("Assets/Dialog JSON/Export to JSON")]
        public static void ExportDialog()
        {
            var dialog = Selection.activeObject as Dialog;
            if (dialog == null) return;

            var path = EditorUtility.SaveFilePanel(
                "Export Dialog to JSON",
                Application.dataPath,
                dialog.name + ".json",
                "json"
            );

            if (!string.IsNullOrEmpty(path))
            {
                DialogJsonExporter.SaveToFile(dialog, path);
                EditorUtility.DisplayDialog("Export Complete", 
                    $"Dialog '{dialog.name}' has been exported to:\n{path}", "OK");
            }
        }

        [MenuItem("Assets/Dialog JSON/Import from JSON")]
        public static void ImportDialog()
        {
            var path = EditorUtility.OpenFilePanel(
                "Import Dialog from JSON",
                Application.dataPath,
                "json"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var dialog = DialogJsonExporter.LoadFromFile(path);
                if (dialog != null)
                {
                    // Create the asset in the project
                    var assetPath = "Assets/" + Path.GetFileNameWithoutExtension(path) + ".asset";
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    
                    AssetDatabase.CreateAsset(dialog, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    // Select the newly created dialog
                    Selection.activeObject = dialog;
                    EditorGUIUtility.PingObject(dialog);

                    EditorUtility.DisplayDialog("Import Complete", 
                        $"Dialog has been imported as:\n{assetPath}", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Import Failed", 
                        "Failed to import dialog from JSON file. Please check the console for errors.", "OK");
                }
            }
        }

        [MenuItem("Assets/Dialog JSON/Validate JSON", validate = true)]
        public static bool ValidateJsonFile_MenuItem()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && Path.GetExtension(path).ToLower() == ".json";
        }

        [MenuItem("Assets/Dialog JSON/Validate JSON")]
        public static void ValidateJsonFile()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return;

            var path = AssetDatabase.GetAssetPath(selectedObject);
            if (Path.GetExtension(path).ToLower() != ".json") return;

            var fullPath = Path.Combine(Application.dataPath, path.Substring("Assets/".Length));
            
            try
            {
                var json = File.ReadAllText(fullPath);
                var dialog = DialogJsonExporter.ImportFromJson(json);
                
                if (dialog != null)
                {
                    EditorUtility.DisplayDialog("Validation Success", 
                        $"JSON file is valid and contains:\n" +
                        $"• Name: {dialog.name}\n" +
                        $"• Options: {dialog.options.Count}\n" +
                        $"• Values: {dialog.values?.Count ?? 0}", "OK");
                    
                    // Clean up the temporary dialog
                    Object.DestroyImmediate(dialog);
                }
                else
                {
                    EditorUtility.DisplayDialog("Validation Failed", 
                        "JSON file could not be parsed as a valid Dialog. Please check the console for errors.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Validation Error", 
                    $"Error reading or parsing JSON file:\n{ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Custom inspector extension for Dialog to add JSON export/import buttons
    /// </summary>
    [CustomEditor(typeof(Dialog))]
    public class DialogInspectorExtension : DialogInspector
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(10);
            
            // Add JSON export/import section
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("JSON Export/Import", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Export to JSON"))
                    {
                        ExportDialogToJson();
                    }
                    
                    if (GUILayout.Button("Import from JSON"))
                    {
                        ImportDialogFromJson();
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.HelpBox(
                    "Export: Save this dialog as a JSON file.\n" +
                    "Import: Replace this dialog's content with data from a JSON file.", 
                    MessageType.Info);
            }
            EditorGUILayout.EndVertical();
        }

        private void ExportDialogToJson()
        {
            var dialog = target as Dialog;
            if (dialog == null) return;

            var path = EditorUtility.SaveFilePanel(
                "Export Dialog to JSON",
                Application.dataPath,
                dialog.name + ".json",
                "json"
            );

            if (!string.IsNullOrEmpty(path))
            {
                DialogJsonExporter.SaveToFile(dialog, path);
                EditorUtility.DisplayDialog("Export Complete", 
                    $"Dialog '{dialog.name}' has been exported to:\n{path}", "OK");
            }
        }

        private void ImportDialogFromJson()
        {
            var dialog = target as Dialog;
            if (dialog == null) return;

            var confirmed = EditorUtility.DisplayDialog(
                "Import from JSON",
                "This will replace all current dialog content with data from the JSON file. This action cannot be undone.\n\nAre you sure you want to continue?",
                "Import", "Cancel"
            );

            if (!confirmed) return;

            var path = EditorUtility.OpenFilePanel(
                "Import Dialog from JSON",
                Application.dataPath,
                "json"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var importedDialog = DialogJsonExporter.LoadFromFile(path);
                if (importedDialog != null)
                {
                    // Copy data from imported dialog to current dialog
                    Undo.RecordObject(dialog, "Import Dialog from JSON");
                    
                    dialog.name = importedDialog.name;
                    dialog.enabled = importedDialog.enabled;
                    
                    // Clear existing data
                    ClearDialogContent(dialog);
                    
                    // Copy values
                    if (importedDialog.values != null)
                    {
                        dialog.values = new System.Collections.Generic.List<Value>();
                        foreach (var value in importedDialog.values)
                        {
                            var newValue = Object.Instantiate(value);
                            AssetDatabase.AddObjectToAsset(newValue, dialog);
                            dialog.values.Add(newValue);
                        }
                    }
                    
                    // Copy options
                    if (importedDialog.options != null)
                    {
                        dialog.options = new System.Collections.Generic.List<DialogOption>();
                        foreach (var option in importedDialog.options)
                        {
                            var newOption = new DialogOption { dialog = dialog };
                            
                            // Copy conditions
                            newOption.conditions = new System.Collections.Generic.List<Condition>();
                            foreach (var condition in option.conditions)
                            {
                                var newCondition = Object.Instantiate(condition);
                                AssetDatabase.AddObjectToAsset(newCondition, dialog);
                                newOption.conditions.Add(newCondition);
                            }
                            
                            // Copy actions
                            newOption.actions = new System.Collections.Generic.List<DialogActionBase>();
                            foreach (var action in option.actions)
                            {
                                var newAction = Object.Instantiate(action);
                                AssetDatabase.AddObjectToAsset(newAction, dialog);
                                newOption.actions.Add(newAction);
                            }
                            
                            dialog.options.Add(newOption);
                        }
                    }

                    EditorUtility.SetDirty(dialog);
                    AssetDatabase.SaveAssets();
                    
                    // Clean up temporary imported dialog
                    Object.DestroyImmediate(importedDialog);
                    
                    EditorUtility.DisplayDialog("Import Complete", 
                        "Dialog content has been successfully imported from JSON.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Import Failed", 
                        "Failed to import dialog from JSON file. Please check the console for errors.", "OK");
                }
            }
        }

        private void ClearDialogContent(Dialog dialog)
        {
            // Clear and destroy existing sub-assets
            if (dialog.values != null)
            {
                foreach (var value in dialog.values)
                {
                    if (value != null)
                        Object.DestroyImmediate(value, true);
                }
                dialog.values.Clear();
            }

            if (dialog.options != null)
            {
                foreach (var option in dialog.options)
                {
                    if (option?.conditions != null)
                    {
                        foreach (var condition in option.conditions)
                        {
                            if (condition != null)
                                Object.DestroyImmediate(condition, true);
                        }
                    }

                    if (option?.actions != null)
                    {
                        foreach (var action in option.actions)
                        {
                            if (action != null)
                                Object.DestroyImmediate(action, true);
                        }
                    }
                }
                dialog.options.Clear();
            }
        }
    }
}