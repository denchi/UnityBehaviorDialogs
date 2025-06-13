using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Behaviours
{
    namespace Dialogs
    {
        [Serializable]
        public class DialogOption
        {
            #if UNITY_EDITOR
            [ListDrawerSettings(CustomAddFunction = nameof(AddCondition), CustomRemoveIndexFunction = nameof(RemoveCondition))]
            [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
            #endif
            public List<Condition> conditions = new List<Condition>();
            
            #if UNITY_EDITOR
            [ListDrawerSettings(CustomAddFunction = nameof(AddAction), CustomRemoveIndexFunction = nameof(RemoveAction))]
            [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
            #endif
            public List<DialogActionBase> actions = new List<DialogActionBase>();

            [HideInInspector]
            public Dialog dialog;
            
            
            //
            
#if UNITY_EDITOR
            // This is the custom add method that Odin Inspector will call 
            // whenever you press the "Add" button in the Inspector.
            private void AddCondition()
            {
                Dialog.Editor.CreateCondition(dialog, this);
            }

            private void RemoveCondition(int index)
            {
                Dialog.Editor.RemoveCondition(dialog, this, index);
            }
            
            /// <summary>
            /// Helper method to find all non-abstract types derived from T in this assembly.
            /// </summary>
            private static IEnumerable<Type> GetAllDerivedTypes<T>()
            {
                return typeof(T)
                    .Assembly
                    .GetTypes()
                    .Where(x => typeof(T) != x && typeof(T).IsAssignableFrom(x) && !x.IsAbstract);
            }
            
            // This is the custom add method that Odin Inspector will call 
            // whenever you press the "Add" button in the Inspector.
            private void AddAction()
            {
                var menu = new GenericMenu();

                // Collect all non-abstract derived types of DialogActionBase in your assembly.
                var derivedTypes = GetAllDerivedTypes<DialogActionBase>();

                foreach (var type in derivedTypes)
                {
                    // The label that appears in the popup menu
                    string menuLabel = type.Name; // or type.FullName, or a custom string

                    menu.AddItem(
                        new GUIContent(menuLabel),
                        on: false,
                        func: () => Dialog.Editor.CreateAction(type, dialog, this) // When selected
                    );
                }

                // If you only want certain whitelisted classes, you could filter out derivedTypes here
                // or add them individually to the menu.

                menu.ShowAsContext();
            }

            private void RemoveAction(int index)
            {
                Dialog.Editor.RemoveAction(dialog, this, index);
            }
#endif
        }
    }
}