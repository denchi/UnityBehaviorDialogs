using System.Collections;
using UnityEngine;

namespace Behaviours
{
    namespace Dialogs
    {
        public class DialogActionBase : ScriptableObject
        {
            public virtual IEnumerator Execute(IDialogContext context)
            {
                yield return null;
            }

            public virtual void Cancel(IDialogContext context)
            {
                
            }
        }
    }
}
