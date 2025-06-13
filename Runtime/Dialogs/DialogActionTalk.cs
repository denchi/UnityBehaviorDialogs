using System.Collections;
using UnityEngine;

namespace Behaviours
{
    namespace Dialogs
    {
        public class DialogActionTalk : DialogActionBase
        {
            public float duration;
            public string text;
            public AudioClip audioClip;

            public override IEnumerator Execute(IDialogContext context)
            {
                var finalDuration = duration;
                if (audioClip)
                {
                    finalDuration = audioClip.length;
                    context.PlayVoice(audioClip);
                }

                bool isDone = false;
                context.ShowSubtitle(text, finalDuration, () => isDone = true);
                while (!isDone)
                {
                    yield return null;
                }
                context.HideSubtitles();
            }

            public override void Cancel(IDialogContext context)
            {
                context.StopVoice();
            }
        }
    }
}
