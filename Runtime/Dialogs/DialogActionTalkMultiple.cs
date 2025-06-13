using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Behaviours.Dialogs
{
    public class DialogActionTalkMultiple : DialogActionBase
    {
        public AudioClip audioClip;
        public List<TextRange> entries;
        
        private bool _isCancelled;

        [Serializable]
        public class TextRange
        {
            public float duration;
            public string text;
        }

        public override IEnumerator Execute(IDialogContext context)
        {
            _isCancelled = false;
            
            if (audioClip)
            {
                context.PlayVoice(audioClip);
            }

            foreach (var entry in entries)
            {
                if (_isCancelled)
                    yield break;
                
                Debug.Log($"ShowSubtitle({entry.text}, {entry.duration})");

                bool isDone = false;
                context.ShowSubtitle(entry.text, entry.duration, ()=> isDone = true );
                
                while (!isDone)
                {
                    if (_isCancelled)
                        yield break; // Exit during the wait.
                    
                    yield return null;
                }
            }
            
            context.HideSubtitles();
        }

        public override void Cancel(IDialogContext context)
        {
            _isCancelled = true;

            context.HideSubtitles();
            context.StopVoice();
        }
    }
}