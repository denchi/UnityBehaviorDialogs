using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Behaviours.Dialogs
{
    /// <summary>
    /// Utility class for exporting/importing Dialog objects to/from JSON format
    /// </summary>
    public static class DialogJsonExporter
    {
        private static IAssetPathResolver _assetPathResolver;

        /// <summary>
        /// Gets or sets the asset path resolver used for handling Unity asset references
        /// Defaults to UnityEditorAssetPathResolver
        /// </summary>
        public static IAssetPathResolver AssetPathResolver
        {
            get
            {
                if (_assetPathResolver == null)
                    _assetPathResolver = new UnityEditorAssetPathResolver();
                return _assetPathResolver;
            }
            set => _assetPathResolver = value;
        }

        #region Export to JSON

        /// <summary>
        /// Exports a Dialog to JSON string
        /// </summary>
        /// <param name="dialog">The Dialog to export</param>
        /// <returns>JSON string representation of the dialog</returns>
        public static string ExportToJson(Dialog dialog)
        {
            if (dialog == null)
                return null;

            var dialogData = new JObject();
            
            // Export basic dialog properties
            dialogData["name"] = dialog.name;
            dialogData["enabled"] = dialog.enabled;

            // Export values (from ILayer)
            var valuesArray = new JArray();
            if (dialog.values != null)
            {
                foreach (var value in dialog.values)
                {
                    valuesArray.Add(ExportValue(value));
                }
            }
            dialogData["values"] = valuesArray;

            // Export dialog options
            var optionsArray = new JArray();
            foreach (var option in dialog.options)
            {
                optionsArray.Add(ExportDialogOption(option));
            }
            dialogData["options"] = optionsArray;

            return dialogData.ToString(Formatting.Indented);
        }

        private static JObject ExportValue(Value value)
        {
            var valueObj = new JObject();
            valueObj["name"] = value.name;
            valueObj["type"] = (int)value.type;

            switch (value.type)
            {
                case ValueType.Bool:
                    valueObj["value"] = value.bValue;
                    break;
                case ValueType.Integer:
                    valueObj["value"] = value.iValue;
                    break;
                case ValueType.Float:
                    valueObj["value"] = value.fValue;
                    break;
                case ValueType.String:
                case ValueType.Other:
                    valueObj["value"] = value.sValue;
                    break;
            }

            return valueObj;
        }

        private static JObject ExportDialogOption(DialogOption option)
        {
            var optionObj = new JObject();

            // Export conditions
            var conditionsArray = new JArray();
            foreach (var condition in option.conditions)
            {
                conditionsArray.Add(ExportCondition(condition));
            }
            optionObj["conditions"] = conditionsArray;

            // Export actions
            var actionsArray = new JArray();
            foreach (var action in option.actions)
            {
                actionsArray.Add(ExportAction(action));
            }
            optionObj["actions"] = actionsArray;

            return optionObj;
        }

        private static JObject ExportCondition(Condition condition)
        {
            var conditionObj = new JObject();
            conditionObj["operation"] = (int)condition.operation;
            conditionObj["valueName"] = condition.value?.name;

            // Export constants based on value type
            if (condition.value != null)
            {
                switch (condition.value.type)
                {
                    case ValueType.Bool:
                        conditionObj["bConstant"] = condition.bConstant;
                        break;
                    case ValueType.Integer:
                        conditionObj["iConstant"] = condition.iConstant;
                        break;
                    case ValueType.Float:
                        conditionObj["fConstant"] = condition.fConstant;
                        break;
                    case ValueType.String:
                    case ValueType.Other:
                        conditionObj["sConstant"] = condition.sConstant;
                        break;
                }
            }

            return conditionObj;
        }

        private static JObject ExportAction(DialogActionBase action)
        {
            var actionObj = new JObject();
            actionObj["type"] = action.GetType().Name;

            switch (action)
            {
                case DialogActionTalk talk:
                    actionObj["text"] = talk.text;
                    actionObj["duration"] = talk.duration;
                    if (talk.audioClip != null)
                    {
                        var audioClipPath = AssetPathResolver.GetAssetPath(talk.audioClip);
                        if (!string.IsNullOrEmpty(audioClipPath))
                            actionObj["audioClipPath"] = audioClipPath;
                    }
                    break;
                    
                case DialogActionTalkMultiple talkMultiple:
                    var entriesArray = new JArray();
                    if (talkMultiple.entries != null)
                    {
                        foreach (var entry in talkMultiple.entries)
                        {
                            var entryObj = new JObject();
                            entryObj["text"] = entry.text;
                            entryObj["duration"] = entry.duration;
                            entriesArray.Add(entryObj);
                        }
                    }
                    actionObj["entries"] = entriesArray;
                    if (talkMultiple.audioClip != null)
                    {
                        var audioClipPath = AssetPathResolver.GetAssetPath(talkMultiple.audioClip);
                        if (!string.IsNullOrEmpty(audioClipPath))
                            actionObj["audioClipPath"] = audioClipPath;
                    }
                    break;
                    
                case DialogActionSet set:
                    actionObj["varName"] = set.varName;
                    actionObj["bConstant"] = set.bConstant;
                    actionObj["iConstant"] = set.iConstant;
                    actionObj["fConstant"] = set.fConstant;
                    actionObj["sConstant"] = set.sConstant;
                    break;
                    
                case DialogAddInputAction input:
                    actionObj["prompt"] = input.prompt;
                    var inputOptionsArray = new JArray();
                    foreach (var inputOption in input.options)
                    {
                        var inputOptionObj = new JObject();
                        inputOptionObj["text"] = inputOption.text;
                        
                        var inputActionsArray = new JArray();
                        foreach (var inputOptionAction in inputOption.actions)
                        {
                            var inputActionObj = new JObject();
                            inputActionObj["type"] = (int)inputOptionAction.type;
                            inputActionObj["varName"] = inputOptionAction.varName;
                            inputActionObj["stateName"] = inputOptionAction.stateName;
                            if (inputOptionAction.cutscene != null)
                            {
                                var cutscenePath = AssetPathResolver.GetAssetPath(inputOptionAction.cutscene);
                                if (!string.IsNullOrEmpty(cutscenePath))
                                    inputActionObj["cutscenePath"] = cutscenePath;
                            }
                            inputActionsArray.Add(inputActionObj);
                        }
                        inputOptionObj["actions"] = inputActionsArray;
                        inputOptionsArray.Add(inputOptionObj);
                    }
                    actionObj["options"] = inputOptionsArray;
                    break;
            }

            return actionObj;
        }

        #endregion

        #region Import from JSON

        /// <summary>
        /// Creates a Dialog from JSON string
        /// </summary>
        /// <param name="json">JSON string representation of a dialog</param>
        /// <returns>New Dialog instance created from JSON</returns>
        public static Dialog ImportFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                var dialogData = JObject.Parse(json);
                var dialog = ScriptableObject.CreateInstance<Dialog>();

                // Import basic properties
                dialog.name = dialogData["name"]?.ToString() ?? "Imported Dialog";
                dialog.enabled = dialogData["enabled"]?.ToObject<bool>() ?? true;

                // Import values
                dialog.values = new List<Value>();
                var valuesArray = dialogData["values"] as JArray;
                if (valuesArray != null)
                {
                    foreach (var valueToken in valuesArray)
                    {
                        var value = ImportValue(valueToken as JObject, dialog);
                        if (value != null)
                        {
                            dialog.values.Add(value);
                        }
                    }
                }

                // Import options
                dialog.options = new List<DialogOption>();
                var optionsArray = dialogData["options"] as JArray;
                if (optionsArray != null)
                {
                    foreach (var optionToken in optionsArray)
                    {
                        var option = ImportDialogOption(optionToken as JObject, dialog);
                        if (option != null)
                        {
                            dialog.options.Add(option);
                        }
                    }
                }

                return dialog;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to import dialog from JSON: {ex.Message}");
                return null;
            }
        }

        private static Value ImportValue(JObject valueObj, Dialog parentDialog)
        {
            if (valueObj == null) return null;

            var value = ScriptableObject.CreateInstance<Value>();
            value.name = valueObj["name"]?.ToString() ?? "Unnamed Value";
            value.type = (ValueType)(valueObj["type"]?.ToObject<int>() ?? 0);

#if UNITY_EDITOR
            TryAddSubAsset(value, parentDialog);
#endif

            switch (value.type)
            {
                case ValueType.Bool:
                    value.bValue = valueObj["value"]?.ToObject<bool>() ?? false;
                    break;
                case ValueType.Integer:
                    value.iValue = valueObj["value"]?.ToObject<int>() ?? 0;
                    break;
                case ValueType.Float:
                    value.fValue = valueObj["value"]?.ToObject<float>() ?? 0f;
                    break;
                case ValueType.String:
                case ValueType.Other:
                    value.sValue = valueObj["value"]?.ToString() ?? "";
                    break;
            }

            return value;
        }

        private static DialogOption ImportDialogOption(JObject optionObj, Dialog parentDialog)
        {
            if (optionObj == null) return null;

            var option = new DialogOption { dialog = parentDialog };

            // Import conditions
            option.conditions = new List<Condition>();
            var conditionsArray = optionObj["conditions"] as JArray;
            if (conditionsArray != null)
            {
                foreach (var conditionToken in conditionsArray)
                {
                    var condition = ImportCondition(conditionToken as JObject, parentDialog);
                    if (condition != null)
                    {
                        option.conditions.Add(condition);
                    }
                }
            }

            // Import actions
            option.actions = new List<DialogActionBase>();
            var actionsArray = optionObj["actions"] as JArray;
            if (actionsArray != null)
            {
                foreach (var actionToken in actionsArray)
                {
                    var action = ImportAction(actionToken as JObject, parentDialog);
                    if (action != null)
                    {
                        option.actions.Add(action);
                    }
                }
            }

            return option;
        }

        private static Condition ImportCondition(JObject conditionObj, Dialog parentDialog)
        {
            if (conditionObj == null) return null;

            var condition = ScriptableObject.CreateInstance<Condition>();
            condition.operation = (Operation)(conditionObj["operation"]?.ToObject<int>() ?? 0);

            // Find the referenced value by name
            var valueName = conditionObj["valueName"]?.ToString();
            if (!string.IsNullOrEmpty(valueName) && parentDialog.values != null)
            {
                condition.value = parentDialog.values.FirstOrDefault(v => v.name == valueName);
            }

            // Import constants
            if (conditionObj["bConstant"] != null)
                condition.bConstant = conditionObj["bConstant"].ToObject<bool>();
            if (conditionObj["iConstant"] != null)
                condition.iConstant = conditionObj["iConstant"].ToObject<int>();
            if (conditionObj["fConstant"] != null)
                condition.fConstant = conditionObj["fConstant"].ToObject<float>();
            if (conditionObj["sConstant"] != null)
                condition.sConstant = conditionObj["sConstant"].ToString();

#if UNITY_EDITOR
            TryAddSubAsset(condition, parentDialog);
#endif

            return condition;
        }

        private static DialogActionBase ImportAction(JObject actionObj, Dialog parentDialog)
        {
            if (actionObj == null) return null;

            var actionType = actionObj["type"]?.ToString();
            DialogActionBase action = null;

            switch (actionType)
            {
                case nameof(DialogActionTalk):
                    var talk = ScriptableObject.CreateInstance<DialogActionTalk>();
                    talk.text = actionObj["text"]?.ToString() ?? "";
                    if (actionObj["duration"] != null) talk.duration = actionObj["duration"].ToObject<float>();
                    var audioClipPath = actionObj["audioClipPath"]?.ToString();
                    if (!string.IsNullOrEmpty(audioClipPath))
                        talk.audioClip = AssetPathResolver.LoadAssetAtPath<AudioClip>(audioClipPath);
                    action = talk;
                    break;

                case nameof(DialogActionTalkMultiple):
                    var talkMultiple = ScriptableObject.CreateInstance<DialogActionTalkMultiple>();
                    var entriesArray = actionObj["entries"] as JArray;
                    talkMultiple.entries = new List<DialogActionTalkMultiple.TextRange>();
                    if (entriesArray != null)
                    {
                        foreach (var entryToken in entriesArray)
                        {
                            var entryData = entryToken as JObject;
                            var textRange = new DialogActionTalkMultiple.TextRange();
                            textRange.text = entryData?["text"]?.ToString() ?? "";
                            textRange.duration = entryData?["duration"]?.ToObject<float>() ?? 1f;
                            talkMultiple.entries.Add(textRange);
                        }
                    }
                    var audioClipPathMultiple = actionObj["audioClipPath"]?.ToString();
                    if (!string.IsNullOrEmpty(audioClipPathMultiple))
                        talkMultiple.audioClip = AssetPathResolver.LoadAssetAtPath<AudioClip>(audioClipPathMultiple);
                    action = talkMultiple;
                    break;

                case nameof(DialogActionSet):
                    var set = ScriptableObject.CreateInstance<DialogActionSet>();
                    set.varName = actionObj["varName"]?.ToString() ?? "";
                    if (actionObj["bConstant"] != null) set.bConstant = actionObj["bConstant"].ToObject<bool>();
                    if (actionObj["iConstant"] != null) set.iConstant = actionObj["iConstant"].ToObject<int>();
                    if (actionObj["fConstant"] != null) set.fConstant = actionObj["fConstant"].ToObject<float>();
                    if (actionObj["sConstant"] != null) set.sConstant = actionObj["sConstant"].ToString();
                    action = set;
                    break;

                case nameof(DialogAddInputAction):
                    var input = ScriptableObject.CreateInstance<DialogAddInputAction>();
                    input.prompt = actionObj["prompt"]?.ToString() ?? "";
                    input.options = new List<DialogAddInputAction.InputOption>();

                    var optionsArray = actionObj["options"] as JArray;
                    if (optionsArray != null)
                    {
                        foreach (var optionToken in optionsArray)
                        {
                            var optionData = optionToken as JObject;
                            var inputOption = new DialogAddInputAction.InputOption();
                            inputOption.text = optionData?["text"]?.ToString() ?? "";
                            inputOption.actions = new List<DialogAddInputAction.InputOptionAction>();

                            var inputActionsArray = optionData?["actions"] as JArray;
                            if (inputActionsArray != null)
                            {
                                foreach (var inputActionToken in inputActionsArray)
                                {
                                    var inputActionData = inputActionToken as JObject;
                                    var inputOptionAction = new DialogAddInputAction.InputOptionAction();
                                    inputOptionAction.type = (DialogAddInputAction.InputOptionActionType)(inputActionData?["type"]?.ToObject<int>() ?? 0);
                                    inputOptionAction.varName = inputActionData?["varName"]?.ToString() ?? "";
                                    inputOptionAction.stateName = inputActionData?["stateName"]?.ToString() ?? "";
                                    var cutscenePath = inputActionData?["cutscenePath"]?.ToString();
                                    if (!string.IsNullOrEmpty(cutscenePath))
                                        inputOptionAction.cutscene = AssetPathResolver.LoadAssetAtPath<UnityEngine.Timeline.TimelineAsset>(cutscenePath);
                                    inputOption.actions.Add(inputOptionAction);
                                }
                            }

                            input.options.Add(inputOption);
                        }
                    }
                    action = input;
                    break;

                default:
                    Debug.LogWarning($"Unknown action type: {actionType}");
                    return null;
            }

            if (action != null)
            {
#if UNITY_EDITOR
                TryAddSubAsset(action, parentDialog);
#endif
            }

            return action;
        }

#if UNITY_EDITOR
        private static void TryAddSubAsset(UnityEngine.Object child, Dialog parentDialog)
        {
            if (child == null || parentDialog == null) return;
            if (!UnityEditor.EditorUtility.IsPersistent(parentDialog)) return;

            UnityEditor.AssetDatabase.AddObjectToAsset(child, parentDialog);
        }
#endif

        #endregion

        #region Utility Methods

        /// <summary>
        /// Saves a Dialog as JSON file to the specified path
        /// </summary>
        /// <param name="dialog">Dialog to save</param>
        /// <param name="filePath">File path to save to</param>
        public static void SaveToFile(Dialog dialog, string filePath)
        {
            try
            {
                var json = ExportToJson(dialog);
                System.IO.File.WriteAllText(filePath, json);
                Debug.Log($"Dialog exported to: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save dialog to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a Dialog from JSON file at the specified path
        /// </summary>
        /// <param name="filePath">File path to load from</param>
        /// <returns>Dialog loaded from file or null if failed</returns>
        public static Dialog LoadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    Debug.LogError($"File not found: {filePath}");
                    return null;
                }

                var json = System.IO.File.ReadAllText(filePath);
                var dialog = ImportFromJson(json);
                
                if (dialog != null)
                {
                    Debug.Log($"Dialog imported from: {filePath}");
                }
                
                return dialog;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load dialog from file: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
