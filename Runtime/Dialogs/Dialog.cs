using System.Collections.Generic;
using UnityEngine;

namespace Behaviours
{
    namespace Dialogs
    {
        [CreateAssetMenu]
        public class Dialog : ILayer
        {
            public List<DialogOption> options = new List<DialogOption>();

            public override void OnEnable()
            {
                base.OnEnable();

                foreach (var option in options)
                {
                    option.dialog = this;
                }
            }

            public static class Editor
            {
                public static DialogOption CreateOption(Dialog dialog)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(dialog, "Add dialog option");
#endif

                    var option = new DialogOption { dialog = dialog };
                    dialog.options.Add(option);

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(dialog);
#endif
                    return option;
                }

                public static void RemoveOption(int idxToDelete, Dialog dialog)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(dialog, "Remove dialog option");
#endif

                    var option = dialog.options[idxToDelete];

                    foreach (var condition in option.conditions)
                    {
                        DestroyImmediate(condition, true);
                    }

                    foreach (var action in option.actions)
                    {
                        DestroyImmediate(action, true);
                    }

                    dialog.options.RemoveAt(idxToDelete);

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(dialog);
#endif
                }

                public static Condition CreateCondition(Dialog dialog, DialogOption option)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(dialog, "Create dialog condition");
#endif

                    var action = ScriptableObject.CreateInstance<Condition>();
#if UNITY_EDITOR
                    UnityEditor.AssetDatabase.AddObjectToAsset(action, dialog);
#endif
                    option.conditions.Add(action);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(dialog);
#endif
                    return action;
                }

                public static T CreateAction<T>(Dialog dialog, DialogOption option) where T : DialogActionBase
                {
                    return CreateAction(typeof(T), dialog, option) as T;
                }
                
                public static DialogActionBase CreateAction(System.Type type, Dialog dialog, DialogOption option)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(dialog, "Create dialog action");
#endif

                    var action = ScriptableObject.CreateInstance(type) as DialogActionBase;
#if UNITY_EDITOR
                    action.name = true.GetType().Name;
                    UnityEditor.AssetDatabase.AddObjectToAsset(action, dialog);
#endif
                    option.actions.Add(action);
#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(dialog);
#endif
                    return action;
                }

                public static void RemoveCondition(Dialog dialog, DialogOption option, int idxToDelete)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(dialog, "Remove option condition");
#endif

                    var condition = option.conditions[idxToDelete];
                    DestroyImmediate(condition, true);
                    option.conditions.RemoveAt(idxToDelete);

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(dialog);
#endif
                }

                public static void RemoveAction(Dialog dialog, DialogOption option, int idxToDelete)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(dialog, "Remove option action");
#endif

                    var condition = option.actions[idxToDelete];
                    DestroyImmediate(condition, true);
                    option.actions.RemoveAt(idxToDelete);

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(dialog);
#endif
                }
            }
        }
    }
}
