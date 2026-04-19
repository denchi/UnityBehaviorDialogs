using System;
using System.Collections.Generic;
using System.Reflection;
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
            /// Helper method to find all non-abstract types derived from T in loaded assemblies.
            /// </summary>
            public static IEnumerable<Type> GetAllDerivedTypes<T>()
            {
                var parentType = typeof(T);
                return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(GetTypesSafe)
                    .Where(type => type != null && type != parentType && parentType.IsAssignableFrom(type) && !type.IsAbstract);
            }

            static IEnumerable<Type> GetTypesSafe(Assembly assembly)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    return exception.Types.Where(type => type != null);
                }
            }
        }
    }
}
