using System;
using System.Collections.Generic;
using Behaviours.Runtime;
using UnityEngine;
using UnityEngine.Timeline;

namespace Behaviours.Dialogs
{
    public interface IDialogContext
    {
        void PlayVoice(AudioClip clip);
        void ShowSubtitle(string text, float duration, Action onDone);
        RuntimeValueCollection GetRuntimeValues();
        void StopVoice();
        void HideSubtitles();
        void ShowInputs(string prompt, List<string> optionsNames, Action<string> onSelect, Action onCancel);
        void TriggerAction(string actionStateName);
        void PlayCutscene(TimelineAsset actionCutscene);
    }
}