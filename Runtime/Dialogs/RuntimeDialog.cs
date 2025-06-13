using Behaviours.HFSM.Runtime;
using Behaviours.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Behaviours.Dialogs.Runtime
{
    public class RuntimeDialog
    {
        public Dialog dialog;
        public RuntimeValueCollection runtimeValues;
        public List<RuntimeOption> runtimeOptions;

        public static RuntimeDialog CreateRuntimeDialog(Dialog dialog)
        {
            var runtimeDialog = new RuntimeDialog
            {
                // VARS
                dialog = dialog,
                runtimeValues = new RuntimeValueCollection()
            };

            runtimeDialog.runtimeValues.InitWithLayer(dialog);

            // OPTIONS
            runtimeDialog.runtimeOptions = dialog.options
                .Select(CreateRuntimeOption)
                .ToList(); 
            
            return runtimeDialog;

            RuntimeOption CreateRuntimeOption(DialogOption option)
            {
                return new RuntimeOption
                {
                    option = option,

                    // CONDITIONS
                    runtimeConditions = option.conditions
                        .Select(condition => Utils.CreateRuntimeCondition(runtimeDialog.runtimeValues, condition))
                        .ToList(),
                };
            }
        }

        public static bool EvaluateConditions(List<RuntimeConditionData> runtimeConditions)
        {
            var finalValue = true;

            for (var i = 0; i < runtimeConditions.Count; ++i)
            {
                var cond = runtimeConditions[i];
                var value = cond.value.compare(cond);
                if (i > 0)
                {
                    if (cond.nextOperand == Operand.And)
                    {
                        finalValue = finalValue && value;
                        if (finalValue == false)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        finalValue = finalValue || value;
                    }
                }
                else
                {
                    finalValue = value;
                }
            }

            return finalValue;
        }
        
        //
        
        public void SaveRuntimeValues(IValueStorage storage)
        {
            foreach (var runtimeValue in runtimeValues.Values)
            {
                SaveRuntimeValue(runtimeValue, storage);
            }
        }
        
        public void LoadRuntimeValues(IValueStorage storage)
        {
            foreach (var runtimeValue in runtimeValues.Values)
            {
                LoadRuntimeValue(runtimeValue, storage);
            }
        }

        public void SaveRuntimeValue(RuntimeValueData runtimeValue, IValueStorage storage)
        {
            var name = $"{dialog.name}-{runtimeValue.name}";
            switch (runtimeValue.type)
            {
                case ValueType.Bool:
                    storage.SetBool(name, runtimeValue.bValue);
                    break;
                case ValueType.Integer:
                    storage.SetInt(name, runtimeValue.iValue);
                    break;
                case ValueType.Float:
                    storage.SetFloat(name, runtimeValue.fValue);
                    break;
                case ValueType.String:
                    storage.SetString(name, runtimeValue.sValue);
                    break;
            }
        }
        
        public void LoadRuntimeValue(RuntimeValueData runtimeValue, IValueStorage storage)
        {
            var name = $"{dialog.name}-{runtimeValue.name}";
            switch (runtimeValue.type)
            {
                case ValueType.Bool:
                    runtimeValue.bValue = storage.GetBool(name);
                    break;
                case ValueType.Integer:
                    runtimeValue.iValue = storage.GetInt(name);
                    break;
                case ValueType.Float:
                    runtimeValue.fValue = storage.GetFloat(name);
                    break;
                case ValueType.String:
                    runtimeValue.sValue = storage.GetString(name);
                    break;
            }
        }
    }
}
