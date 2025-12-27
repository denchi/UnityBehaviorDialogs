using UnityEngine;
using Behaviours.Dialogs;
using System.IO;

namespace Behaviours.Dialogs.Examples
{
    /// <summary>
    /// Example script demonstrating how to use DialogJsonExporter programmatically
    /// </summary>
    public class DialogJsonExample : MonoBehaviour
    {
        [Header("Example Dialog Export/Import")]
        [SerializeField] private Dialog sourceDialog;
        [SerializeField] private string exportFileName = "ExportedDialog.json";

        [Space]
        [Header("Runtime Testing")]
        [SerializeField] private bool logJsonOnStart = false;

        [Header("Asset Resolver Configuration")]
        [SerializeField] private bool useNullResolver = false;
        [SerializeField] private bool useMappedResolver = false;

        private void Start()
        {
            // Configure asset resolver based on settings
            if (useNullResolver)
            {
                AssetPathResolverUtility.UseNullResolver();
                Debug.Log("Using Null Asset Resolver - asset references will be ignored");
            }
            else if (useMappedResolver)
            {
                AssetPathResolverUtility.UseMappedResolver();
                Debug.Log("Using Mapped Asset Resolver - add mappings as needed");
            }
            else
            {
                AssetPathResolverUtility.UseUnityEditorResolver();
                Debug.Log("Using Unity Editor Asset Resolver - full asset support in editor");
            }

            AssetPathResolverUtility.LogResolverInfo();

            if (logJsonOnStart && sourceDialog != null)
            {
                // Example: Export dialog to JSON string and log it
                var json = DialogJsonExporter.ExportToJson(sourceDialog);
                Debug.Log($"Exported Dialog JSON:\n{json}");
            }
        }

        [ContextMenu("Export Dialog to JSON")]
        public void ExportDialogToJson()
        {
            if (sourceDialog == null)
            {
                Debug.LogError("No source dialog assigned!");
                return;
            }

            var filePath = Path.Combine(Application.persistentDataPath, exportFileName);
            DialogJsonExporter.SaveToFile(sourceDialog, filePath);
            Debug.Log($"Dialog exported to: {filePath}");
        }

        [ContextMenu("Import Dialog from JSON")]
        public void ImportDialogFromJson()
        {
            var filePath = Path.Combine(Application.persistentDataPath, exportFileName);
            var importedDialog = DialogJsonExporter.LoadFromFile(filePath);
            
            if (importedDialog != null)
            {
                Debug.Log($"Successfully imported dialog: {importedDialog.name}");
                Debug.Log($"Dialog has {importedDialog.options.Count} options and {importedDialog.values?.Count ?? 0} values");

                // You can now use the imported dialog
                // For example, assign it to a field or use it in your dialog system
            }
            else
            {
                Debug.LogError($"Failed to import dialog from: {filePath}");
            }
        }

        [ContextMenu("Create Sample Dialog and Export")]
        public void CreateSampleDialogAndExport()
        {
            // Create a sample dialog programmatically
            var dialog = ScriptableObject.CreateInstance<Dialog>();
            dialog.name = "Sample Dialog";
            dialog.enabled = true;

            // Add a sample value
            dialog.values = new System.Collections.Generic.List<Value>();
            var sampleValue = ScriptableObject.CreateInstance<Value>();
            sampleValue.name = "PlayerMet";
            sampleValue.type = ValueType.Bool;
            sampleValue.bValue = false;
            dialog.values.Add(sampleValue);

            // Add a sample option
            dialog.options = new System.Collections.Generic.List<DialogOption>();
            var option = new DialogOption { dialog = dialog };
            
            // Add a condition
            option.conditions = new System.Collections.Generic.List<Condition>();
            var condition = ScriptableObject.CreateInstance<Condition>();
            condition.value = sampleValue;
            condition.operation = Operation.Is;
            condition.bConstant = false;
            option.conditions.Add(condition);

            // Add a talk action with audio clip (if available)
            option.actions = new System.Collections.Generic.List<DialogActionBase>();
            var talkAction = ScriptableObject.CreateInstance<DialogActionTalk>();
            talkAction.text = "Hello! Nice to meet you.";
            talkAction.duration = 2.0f;
            // Note: You can assign an AudioClip here and it will be preserved in JSON via asset path
            // talkAction.audioClip = your_audio_clip;
            option.actions.Add(talkAction);

            // Add a set action
            var setAction = ScriptableObject.CreateInstance<DialogActionSet>();
            setAction.varName = "PlayerMet";
            setAction.bConstant = true;
            option.actions.Add(setAction);

            dialog.options.Add(option);

            // Export the sample dialog
            var json = DialogJsonExporter.ExportToJson(dialog);
            Debug.Log($"Sample Dialog JSON:\n{json}");

            // Save to file
            var filePath = Path.Combine(Application.persistentDataPath, "SampleDialog.json");
            DialogJsonExporter.SaveToFile(dialog, filePath);
            Debug.Log($"Sample dialog exported to: {filePath}");
        }

        [ContextMenu("Create Mapped Resolver from Dialog")]
        public void CreateMappedResolverFromDialog()
        {
            if (sourceDialog == null)
            {
                Debug.LogError("No source dialog assigned!");
                return;
            }

            // Create a mapped resolver from the dialog's asset references
            var mappedResolver = AssetPathResolverUtility.CreateMappedResolverFromDialog(sourceDialog);
            
            // Apply it to the DialogJsonExporter
            DialogJsonExporter.AssetPathResolver = mappedResolver;
            
            Debug.Log($"Created mapped resolver with {System.Linq.Enumerable.Count(mappedResolver.GetMappedPaths())} asset mappings");
            
            // Test export/import with the mapped resolver
            var json = DialogJsonExporter.ExportToJson(sourceDialog);
            var importedDialog = DialogJsonExporter.ImportFromJson(json);
            
            if (importedDialog != null)
            {
                Debug.Log("Successfully tested export/import with mapped resolver");
                Object.DestroyImmediate(importedDialog);
            }
        }
        public void TestJsonRoundTrip()
        {
            if (sourceDialog == null)
            {
                Debug.LogError("No source dialog assigned!");
                return;
            }

            // Export to JSON
            var originalJson = DialogJsonExporter.ExportToJson(sourceDialog);
            Debug.Log("Original dialog exported to JSON");

            // Import from JSON
            var importedDialog = DialogJsonExporter.ImportFromJson(originalJson);
            
            if (importedDialog != null)
            {
                Debug.Log("Dialog successfully imported from JSON");

                // Export the imported dialog back to JSON
                var roundTripJson = DialogJsonExporter.ExportToJson(importedDialog);
                
                // Compare the JSONs (basic validation)
                bool isIdentical = originalJson.Equals(roundTripJson);
                Debug.Log($"Round-trip test result: {(isIdentical ? "PASSED" : "DIFFERENCES DETECTED")}");
                
                if (!isIdentical)
                {
                    Debug.Log($"Original JSON length: {originalJson.Length}");
                    Debug.Log($"Round-trip JSON length: {roundTripJson.Length}");
                    // You could add more detailed comparison logic here
                }
            }
            else
            {
                Debug.LogError("Failed to import dialog from JSON during round-trip test");
            }
        }
    }
}