using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;

namespace Behaviours.Dialogs
{
    public class DialogAddInputAction : DialogActionBase
    {
        public string prompt;
        public List<InputOption> options = new List<InputOption>();

        private bool _isCancelled;

        private void OnEnable()
        {
            foreach (var option in options)
            {
                option.DialogAction = this;
            }
        }

        [Serializable]
        public class InputOption
        {
            public string text;
            public List<InputOptionAction> actions = new List<InputOptionAction>();
            
            private DialogAddInputAction _dialogAction;

            public DialogAddInputAction DialogAction
            {
                get => _dialogAction;
                set
                {
                    _dialogAction = value;
                    foreach (var action in actions)
                    {
                        action.Option = this;
                    }
                }
            }
        }
        
        [Serializable]
        public class InputOptionAction
        {
            [HorizontalGroup("Value")]
            public InputOptionActionType type;
            
            [ValueDropdown(nameof(Values))]
            [HorizontalGroup("Value")]
            [HideLabel]
            [ShowIf(nameof(type), InputOptionActionType.ChangeValue)]
            public string varName;
            
            //TODO: Add a dropdown to select the action state from the current layer
            //[ActionStateId]
            [HorizontalGroup("Value")]
            [HideLabel]
            [ShowIf(nameof(type), InputOptionActionType.TriggerAction)]
            public string stateName;
            
            [HorizontalGroup("Value")]
            [HideLabel]
            [ShowIf(nameof(type), InputOptionActionType.Cutscene)]
            public TimelineAsset cutscene;

            private InputOption _option;

            public InputOption Option
            {
                get => _option;
                set => _option = value;
            }
            
            private IEnumerable Values
            {
                get
                {
                    #if UNITY_EDITOR
                    var thisPath = UnityEditor.AssetDatabase.GetAssetPath(Option.DialogAction);
                    var mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(thisPath);
                    if (mainAsset is ILayer layer)
                    {
                        return layer.values.Select(v => v.name);
                    }
                    #endif

                    return Array.Empty<string>();
                }
            }
        }
        
        public enum InputOptionActionType
        {
            None = -1,
            TriggerAction,
            ChangeValue,
            Cutscene
        }

        public override IEnumerator Execute(IDialogContext context)
        {
            Debug.Log("[PanelSubtitles] context.ShowInputs");
            
            _isCancelled = false;
            
            var optionsNames = options.Select(option => option.text).ToList();
            var resultOption = default(InputOption);
            var result = false;
            
            context.ShowInputs(prompt, optionsNames, 
                optionName =>
                {
                    resultOption = options.FirstOrDefault(option => option.text == optionName);
                    result = true;
                },
                () =>
                {
                    _isCancelled = true;
                });

            while (!result && !_isCancelled)
            {
                yield return null;
            }

            if (resultOption != null)
            {
                foreach (var action in resultOption.actions)
                {
                    switch (action.type)
                    {
                        case InputOptionActionType.ChangeValue:
                            context.GetRuntimeValues().SetBool(action.varName, true);
                            break;
                        case InputOptionActionType.TriggerAction:
                            context.TriggerAction(action.stateName);
                            break;
                        
                        case InputOptionActionType.Cutscene:
                            context.PlayCutscene(action.cutscene);
                            break;
                    }
                }
            }
        }
            
        public override void Cancel(IDialogContext context)
        {
            _isCancelled = true;

            context.HideSubtitles();
            context.StopVoice();
        }
    }
}