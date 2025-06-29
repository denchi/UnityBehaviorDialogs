using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Behaviours
{
    namespace Dialogs
    {
        [Serializable]
        public class DialogOption
        {
            [SerializeField]
            public List<Condition> conditions = new List<Condition>();
            
            [SerializeField]
            public List<DialogActionBase> actions = new List<DialogActionBase>();

            [HideInInspector]
            public Dialog dialog;
            
            /// <summary>
            /// Helper method to find all non-abstract types derived from T in this assembly.
            /// </summary>
            public static IEnumerable<Type> GetAllDerivedTypes<T>()
            {
                return typeof(T)
                    .Assembly
                    .GetTypes()
                    .Where(x => typeof(T) != x && typeof(T).IsAssignableFrom(x) && !x.IsAbstract);
            }
        }
    }
}