using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Behaviours
{
    namespace Dialogs
    {
        public class DialogActionSet : DialogActionBase
        {
            [SerializeField]
            public string varName;
            
            [SerializeField]
            public bool bConstant;
        
            [SerializeField]
            public int iConstant;
        
            [SerializeField]
            public float fConstant;
        
            [SerializeField]
            public string sConstant;

            public override IEnumerator Execute(IDialogContext context)
            {
                var value = context.GetRuntimeValues().GetRuntimeValue(varName);
                if (value == null)
                    yield break;
            
                switch (value.type)
                {
                    case ValueType.Integer:
                        context.GetRuntimeValues().SetInt(varName, iConstant);
                        break;

                    case ValueType.Float:
                        context.GetRuntimeValues().SetFloat(varName, fConstant);
                        break;

                    case ValueType.Bool:
                        context.GetRuntimeValues().SetBool(varName, bConstant);
                        break;

                    case ValueType.String:
                        context.GetRuntimeValues().SetString(varName, sConstant);
                        break;
                }
            }
            
            private string[] GetAvailableValues()
            {
                #if UNITY_EDITOR
                var thisPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                var mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(thisPath);
                if (mainAsset is ILayer layer)
                {
                    return layer.values.Select(v => v.name).ToArray();
                }
                #endif

                return Array.Empty<string>();
            }

            public Value GetValueFromParent()
            {
                #if UNITY_EDITOR
                var thisPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                var mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(thisPath);
                if (mainAsset is ILayer layer)
                {
                    return layer.values.Find(v => v.name == varName);
                }
                #endif
                return null;
            }
            
            public bool IsValueBool() => GetValueFromParent() is { type: ValueType.Bool };
            public bool IsValueInt() => GetValueFromParent() is { type: ValueType.Integer };
            public bool IsValueFloat() => GetValueFromParent() is { type: ValueType.Float };
            public bool IsValueString() => GetValueFromParent() is { type: ValueType.String };
        }
    }
}
