using System;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;

namespace Behaviours
{
    namespace Dialogs
    {
        public class DialogActionSet : DialogActionBase
        {
            [ValueDropdown(nameof(Values))]
            [HorizontalGroup("Value")]
            [LabelText("Set")]
            public string varName;
            
            [HorizontalGroup("Value")]
            [ShowIf(nameof(IsValueBool))]
            [HideLabel]
            public bool bConstant;
        
            [HorizontalGroup("Value")]
            [ShowIf(nameof(IsValueInt))]
            [HideLabel]
            public int iConstant;
        
            [HorizontalGroup("Value")]
            [ShowIf(nameof(IsValueFloat))]
            [HideLabel]
            public float fConstant;
        
            [HorizontalGroup("Value")]
            [ShowIf(nameof(IsValueString))]
            [HideLabel]
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
            
            private IEnumerable Values
            {
                get
                {
                    #if UNITY_EDITOR
                    var thisPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                    var mainAsset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(thisPath);
                    if (mainAsset is ILayer layer)
                    {
                        return layer.values.Select(v => v.name);
                    }
                    #endif

                    return Array.Empty<string>();
                }
            }

            private Value ValueFromParent
            {
                get
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
            }
            private bool IsValueBool => ValueFromParent is { type: ValueType.Bool };
            private bool IsValueInt => ValueFromParent is { type: ValueType.Integer };
            private bool IsValueFloat => ValueFromParent is { type: ValueType.Float };
            private bool IsValueString => ValueFromParent is { type: ValueType.String };
        }
    }
}
