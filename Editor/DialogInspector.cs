using Behaviours;
using Behaviours.Dialogs;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dialog))]
public class DialogInspector : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        var dialog = target as Dialog;
        ValueGUI.DrawLayerValues("Vars", dialog);

        List<string> values = new List<string>();
        foreach (Value val in dialog.values)
        {
            values.Add(val.name);
        }

        var idxToDelete = -1;
        for (var i = 0; i < dialog.options.Count; i++)
        {
            if (!DrawOption(i, dialog, values))
            {
                idxToDelete = i;
            }
        }

        if (idxToDelete != -1)
        {
            Dialog.Editor.RemoveOption(idxToDelete, dialog);
        }

        DrawAddOption(dialog);
    }

    void DrawAddOption(Dialog dialog)
    {
        if (GUILayout.Button("Add option"))
        {
            Dialog.Editor.CreateOption(dialog);
        }
    }

    void DrawAddOptionAction(Dialog dialog, DialogOption option)
    {
        if (GUILayout.Button("Add action"))
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add Talk Action"), false, () => { Dialog.Editor.CreateAction<DialogActionTalk>(dialog, option); });
            menu.AddItem(new GUIContent("Add Talk Action (Multi)"), false, () => { Dialog.Editor.CreateAction<DialogActionTalkMultiple>(dialog, option); });
            menu.AddItem(new GUIContent("Add Set Action"), false, () => { Dialog.Editor.CreateAction<DialogActionSet>(dialog, option); });
            // Add support for DialogAddInputAction
            menu.AddItem(new GUIContent("Add Input Action"), false, () => { Dialog.Editor.CreateAction<DialogAddInputAction>(dialog, option); });
            menu.ShowAsContext();
        }
    }

    void DrawAddOptionCondition(Dialog dialog, DialogOption option)
    {
        if (GUILayout.Button("Add condition"))
        {
            Dialog.Editor.CreateCondition(dialog, option);
        }
    }

    bool DrawOption(int idx, Dialog dialog, List<string> values)
    {
        var option = dialog.options[idx];

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.PrefixLabel("Conditions");
            var idxToDelete = -1;
            for (var i = 0; i < option.conditions.Count; i++)
            {
                var condition = option.conditions[i];
                if (!DrawCondition(i, condition, dialog, values))
                {
                    idxToDelete = i;
                }
            }
            if (idxToDelete != -1)
            {
                Dialog.Editor.RemoveCondition(dialog, option, idxToDelete);
            }

            DrawAddOptionCondition(dialog, option);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.PrefixLabel("Actions");

            var idxToDelete = -1;
            for (var i = 0; i < option.actions.Count; i++)
            {
                var action = option.actions[i];
                switch (action)
                {
                    case DialogActionTalk talk:
                    {
                        if (!DrawActionTalk(i, talk, dialog))
                        {
                            idxToDelete = i;
                        }

                        break;
                    }
                    case DialogActionTalkMultiple talkMultiple:
                    {
                        if (!DrawActionTalkMultiple(i, talkMultiple, dialog))
                        {
                            idxToDelete = i;
                        }

                        break;
                    }
                    case DialogActionSet set:
                    {
                        if (!DrawActionSet(i, set, dialog, values))
                        {
                            idxToDelete = i;
                        }

                        break;
                    }
                    // Add support for DialogAddInputAction
                    case DialogAddInputAction inputAction:
                    {
                        if (!DrawActionInput(i, inputAction, dialog))
                        {
                            idxToDelete = i;
                        }
                        break;
                    }
                    default:
                        EditorGUILayout.PrefixLabel("Action " + i.ToString());
                        break;
                }
            }

            if (idxToDelete != -1)
            {
                Dialog.Editor.RemoveAction(dialog, option, idxToDelete);
            }

            DrawAddOptionAction(dialog, option);
        }
        EditorGUILayout.EndVertical();

        return true;
    }

    bool DrawCondition(int idx, Condition condition, Dialog dialog, List<string> values)
    {
        bool didNotDelete = true;

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PrefixLabel("Condition " + idx.ToString());
            ValueGUI.BeginBackColor(Color.yellow);
            {
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup(dialog.values.IndexOf(condition.value), values.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    condition.value = dialog.values[newIndex];
                    EditorUtility.SetDirty(condition);
                }
            }
            ValueGUI.EndBackColor();

            EditorGUI.BeginChangeCheck();
            condition.operation = (Operation)EditorGUILayout.EnumPopup(condition.operation, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(condition);
            }

            if (condition.value)
            {
                EditorGUI.BeginChangeCheck();

                const int w = 40;
                switch (condition.value.type)
                {
                    case ValueType.Integer:
                        {
                            condition.iConstant = EditorGUILayout.IntField(condition.iConstant, GUILayout.Width(w));
                        }
                        break;

                    case ValueType.Float:
                        {
                            condition.fConstant = EditorGUILayout.FloatField(condition.fConstant, GUILayout.Width(w));
                        }
                        break;

                    case ValueType.Bool:
                        {
                            condition.bConstant = EditorGUILayout.Toggle(condition.bConstant, GUILayout.Width(w));
                        }
                        break;

                    case ValueType.String:
                        {
                            condition.sConstant = GUILayout.TextField(condition.sConstant, GUILayout.Width(w));
                        }
                        break;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(condition);
                }
            }

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                didNotDelete = false;
            }
        }

        EditorGUILayout.EndHorizontal();

        return didNotDelete;
    }

    bool DrawActionTalk(int idx, DialogActionTalk action, Dialog dialog)
    {
        bool didNotDelete = true;

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Talk", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                float newDuration = EditorGUILayout.FloatField(action.duration);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(action, "Changed action duration");
                    action.duration = newDuration;
                    EditorUtility.SetDirty(action);
                }

                EditorGUI.BeginChangeCheck();
                AudioClip newClip = (AudioClip)EditorGUILayout.ObjectField(action.audioClip, typeof(AudioClip), true);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(action, "Changed action clip");
                    action.audioClip = newClip;
                    EditorUtility.SetDirty(action);
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    didNotDelete = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Text");

                EditorGUI.BeginChangeCheck();
                string newText = EditorGUILayout.TextArea(action.text);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(action, "Changed action text");
                    action.text = newText;
                    EditorUtility.SetDirty(action);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        return didNotDelete;
    }
    
    bool DrawActionTalkMultiple(int idx, DialogActionTalkMultiple action, Dialog dialog)
    {
        bool didNotDelete = true;

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Talk Multiple", EditorStyles.boldLabel);

                // if (!action.audioClip)
                // {
                //     EditorGUI.BeginChangeCheck();
                //     var newDuration = EditorGUILayout.FloatField(action.duration);
                //     if (EditorGUI.EndChangeCheck())
                //     {
                //         Undo.RecordObject(action, "Changed action duration");
                //         action.duration = newDuration;
                //         EditorUtility.SetDirty(action);
                //     }
                // }

                EditorGUI.BeginChangeCheck();
                AudioClip newClip = (AudioClip)EditorGUILayout.ObjectField(action.audioClip, typeof(AudioClip), true);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(action, "Changed action clip");
                    action.audioClip = newClip;
                    EditorUtility.SetDirty(action);
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    didNotDelete = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (action.entries != null)
            {
                DialogActionTalkMultiple.TextRange entryToDelete = null;
                float timeStart = 0; 
                foreach (var entry in action.entries)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.PrefixLabel("Text");

                            EditorGUI.BeginChangeCheck();
                            var newDuration = EditorGUILayout.FloatField(entry.duration);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(action, "Changed action duration");
                                entry.duration = newDuration;
                                EditorUtility.SetDirty(action);
                            }

                            // TODO: Add a way to set the start time of the entry
                            // GUI.enabled = action.audioClip;
                            // if (GUILayout.Button("P", EditorStyles.miniButton))
                            // {
                            //     var startSample = (int)(timeStart * action.audioClip.frequency);
                            //     
                            //     PublicAudioUtil.StopAllClips();
                            //     PublicAudioUtil.PlayClip(action.audioClip, startSample);
                            //     EditorDelayedCall.Schedule(PublicAudioUtil.StopAllClips, entry.duration);
                            // }
                            // GUI.enabled = true;
                            
                            if (GUILayout.Button("-", EditorStyles.miniButton))
                            {
                                entryToDelete = entry;
                            }
                        }
                        GUILayout.EndHorizontal();

                        EditorGUI.BeginChangeCheck();
                        string newText = EditorGUILayout.TextField(entry.text);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(action, "Changed action text");
                            entry.text = newText;
                            EditorUtility.SetDirty(action);
                        }
                    }
                    EditorGUILayout.EndVertical();

                    timeStart += entry.duration;
                }

                if (entryToDelete != null)
                {
                    Undo.RecordObject(action, "Removed entry");
                    action.entries.Remove(entryToDelete);
                    EditorUtility.SetDirty(action);
                }
            }

            if (GUILayout.Button("Add Text Entry"))
            {
                Undo.RecordObject(action, "Changed action text");
                var entry = new DialogActionTalkMultiple.TextRange();
                entry.duration = 1;
                if (action.entries == null)
                {
                    action.entries = new List<DialogActionTalkMultiple.TextRange>();
                }
                action.entries.Add(entry);
                EditorUtility.SetDirty(action);
            }
        }
        EditorGUILayout.EndVertical();

        return didNotDelete;
    }

    bool DrawActionSet(int idx, DialogActionSet action, Dialog dialog, List<string> values)
    {
        bool didNotDelete = true;

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Set", EditorStyles.boldLabel);

                ValueGUI.BeginBackColor(Color.yellow);
                {
                    EditorGUI.BeginChangeCheck();
                    int newIndex = EditorGUILayout.Popup(values.IndexOf(action.varName), values.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        action.varName = values[newIndex];
                        EditorUtility.SetDirty(action);
                    }
                }
                ValueGUI.EndBackColor();

                var value = dialog.findValue(action.varName);
                if (value != null)
                {
                    const int w = 40;

                    EditorGUI.BeginChangeCheck();

                    switch (value.type)
                    {
                        case ValueType.Integer:
                            {
                                action.iConstant = EditorGUILayout.IntField(action.iConstant, GUILayout.Width(w));
                            }
                            break;

                        case ValueType.Float:
                            {
                                action.fConstant = EditorGUILayout.FloatField(action.fConstant, GUILayout.Width(w));
                            }
                            break;

                        case ValueType.Bool:
                            {
                                action.bConstant = EditorGUILayout.Toggle(action.bConstant, GUILayout.Width(w));
                            }
                            break;

                        case ValueType.String:
                            {
                                action.sConstant = GUILayout.TextField(action.sConstant, GUILayout.Width(w));
                            }
                            break;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(action);
                    }
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    didNotDelete = false;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        return didNotDelete;
    }

    // Add inspector UI for DialogAddInputAction
    bool DrawActionInput(int idx, DialogAddInputAction action, Dialog dialog)
    {
        bool didNotDelete = true;

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            EditorGUILayout.BeginHorizontal();
            {
                //EditorGUILayout.PrefixLabel("Input", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                string newPrompt = EditorGUILayout.TextField("Prompt", action.prompt);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(action, "Changed input prompt");
                    action.prompt = newPrompt;
                    EditorUtility.SetDirty(action);
                }

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    didNotDelete = false;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

            if (action.options == null)
                action.options = new List<DialogAddInputAction.InputOption>();

            int optionToDelete = -1;
            for (int i = 0; i < action.options.Count; i++)
            {
                var opt = action.options[i];
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginChangeCheck();
                        string newText = EditorGUILayout.TextField("Text", opt.text);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(action, "Changed input option text");
                            opt.text = newText;
                            EditorUtility.SetDirty(action);
                        }
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            optionToDelete = i;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Actions", EditorStyles.miniBoldLabel);

                    if (opt.actions == null)
                        opt.actions = new List<DialogAddInputAction.InputOptionAction>();

                    int actionToDelete = -1;
                    for (int j = 0; j < opt.actions.Count; j++)
                    {
                        var optAction = opt.actions[j];
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginChangeCheck();
                            var newType = (DialogAddInputAction.InputOptionActionType)EditorGUILayout.EnumPopup(optAction.type, GUILayout.Width(100));
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(action, "Changed input option action type");
                                optAction.type = newType;
                                EditorUtility.SetDirty(action);
                            }

                            switch (optAction.type)
                            {
                                case DialogAddInputAction.InputOptionActionType.ChangeValue:
                                    EditorGUI.BeginChangeCheck();
                                    string newVar = EditorGUILayout.TextField(GUIContent.none, optAction.varName);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(action, "Changed input option action varName");
                                        optAction.varName = newVar;
                                        EditorUtility.SetDirty(action);
                                    }
                                    break;
                                case DialogAddInputAction.InputOptionActionType.TriggerAction:
                                    EditorGUI.BeginChangeCheck();
                                    string newState = EditorGUILayout.TextField(GUIContent.none, optAction.stateName);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(action, "Changed input option action stateName");
                                        optAction.stateName = newState;
                                        EditorUtility.SetDirty(action);
                                    }
                                    break;
                                case DialogAddInputAction.InputOptionActionType.Cutscene:
                                    EditorGUI.BeginChangeCheck();
                                    var newCutscene = (UnityEngine.Timeline.TimelineAsset)EditorGUILayout.ObjectField(optAction.cutscene, typeof(UnityEngine.Timeline.TimelineAsset), false);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(action, "Changed input option action cutscene");
                                        optAction.cutscene = newCutscene;
                                        EditorUtility.SetDirty(action);
                                    }
                                    break;
                            }

                            if (GUILayout.Button("-", GUILayout.Width(20)))
                            {
                                actionToDelete = j;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (actionToDelete != -1)
                    {
                        Undo.RecordObject(action, "Removed input option action");
                        opt.actions.RemoveAt(actionToDelete);
                        EditorUtility.SetDirty(action);
                    }

                    if (GUILayout.Button("Add Action"))
                    {
                        Undo.RecordObject(action, "Added input option action");
                        opt.actions.Add(new DialogAddInputAction.InputOptionAction());
                        EditorUtility.SetDirty(action);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            if (optionToDelete != -1)
            {
                Undo.RecordObject(action, "Removed input option");
                action.options.RemoveAt(optionToDelete);
                EditorUtility.SetDirty(action);
            }

            if (GUILayout.Button("Add Option"))
            {
                Undo.RecordObject(action, "Added input option");
                action.options.Add(new DialogAddInputAction.InputOption());
                EditorUtility.SetDirty(action);
            }
        }
        EditorGUILayout.EndVertical();

        return didNotDelete;
    }
}
