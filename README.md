# Behaviour Dialog

Unity package (`com.deathbygravitystudio.behaviour.dialog` 0.1.0) of ScriptableObject dialogs: each option runs talk, variable, choice, and Timeline actions when its shared-variable conditions pass.

Author: Death By Gravity Studio.

## Features

- `Dialog` assets (`Behaviours.Dialogs.Dialog`) created from **Assets > Create > Dialog**. A dialog is an `ILayer` from Behaviour Vars, so it stores an `enabled` flag and a list of named values.
- Ordered options. Each option has a list of `Condition` assets and a list of `DialogActionBase` actions.
- Runtime evaluation uses `Operation` (`Is`, `IsNot`, `IsGreater`, `IsGreaterOrEqual`, `IsLess`, `IsLessOrEqual`) and combines the checks in one option with each condition's `nextOperand` (`And` or `Or`). `And` returns as soon as one check fails. An option with no conditions passes. The inspector edits the condition's value, operation, and constant, and leaves `nextOperand` at its default, `And`. JSON export writes `operation`, `valueName`, and the matching constant.
- Built-in actions:
  - **Talk** (`DialogActionTalk`) shows one subtitle through `IDialogContext.ShowSubtitle`. An assigned `AudioClip` is played with `PlayVoice`, and the wait uses the clip length. With no clip, the wait uses `duration`. `Cancel` stops the voice.
  - **Talk (multi)** (`DialogActionTalkMultiple`) plays one optional clip, then shows each `TextRange` (text and duration) in order. `Cancel` hides subtitles and stops the voice.
  - **Set** (`DialogActionSet`) writes the matching constant into a runtime variable named `varName`. Handled types are bool, int, float, and string. A missing name is skipped.
  - **Input** (`DialogAddInputAction`) shows a prompt and choice labels through `ShowInputs`. The selected choice can set a bool variable to true (`ChangeValue`), call `TriggerAction` (`TriggerAction`), call `ChangeState` with a state name and bool (`ChangeState`), or call `PlayCutscene` with a `TimelineAsset` (`Cutscene`).
- Further actions: any non-abstract, non-generic `DialogActionBase` subclass is listed in the inspector **Add action** menu. Implement `Execute` and, when needed, `Cancel`.
- Runtime helper `RuntimeDialog.CreateRuntimeDialog` copies the dialog's values into a `RuntimeValueCollection` and builds one `RuntimeOption` per option. Condition data is created with `Behaviours.HFSM.Runtime.Utils.CreateRuntimeCondition`. `RuntimeDialog.EvaluateConditions` returns whether that option should run.
- `SaveRuntimeValues` / `LoadRuntimeValues` read and write bool, int, float, and string values through an `IValueStorage`. Keys are `{dialog.name}-{variable name}`.
- Presentation is an `IDialogContext` implemented by the game: voice, subtitles, the runtime value collection, input choices, `TriggerAction`, `ChangeState`, and `PlayCutscene`.
- Custom inspector for variables, options, conditions, and actions. Adding, duplicating, and removing options, conditions, and actions records undo.
- JSON export and import through `DialogJsonExporter` (`ExportToJson`, `ImportFromJson`, `SaveToFile`, `LoadFromFile`), using Newtonsoft.Json. The JSON stores the dialog name, enabled flag, values, options, conditions, and the four built-in action types. `AudioClip` and `TimelineAsset` references are stored as asset paths.
- Asset path resolution is pluggable (`IAssetPathResolver`): `UnityEditorAssetPathResolver` (default; AssetDatabase, editor only), `NullAssetPathResolver` (paths are dropped), `MappedAssetPathResolver` (an explicit path-to-object map), or a custom resolver. `AssetPathResolverUtility` switches the active resolver.
- Editor entry points: the Dialog inspector **JSON Export/Import** buttons, and **Assets > Dialog JSON** (**Export to JSON**, **Import from JSON**, **Validate JSON**). Export writes a file. The Project-window import creates a new Dialog asset under `Assets/`. Inspector import replaces the open dialog's content after a confirmation dialog. Validate parses a selected `.json` asset and reports the name, option count, and value count.

Field-level JSON notes, value type numbers, and resolver setup are in [`Runtime/DialogJsonExporter_README.md`](Runtime/DialogJsonExporter_README.md). `Runtime/Examples/DialogJsonExample.cs` exports, imports, and round-trips a dialog from context menus on a MonoBehaviour.

## Requirements

| Package | Package id | Role |
| --- | --- | --- |
| [UnityBehaviorVars](https://github.com/denchi/UnityBehaviorVars) | `com.deathbygravitystudio.behaviour.vars` 0.1.0 | `ILayer`, `Value`, `Condition`, `Operation`, `Operand`, `RuntimeValueCollection`, `IValueStorage` |
| [UnityBehaviorHFSM](https://github.com/denchi/UnityBehaviorHFSM) | `com.deathbygravitystudio.behaviour.hfsm` 0.1.0 | `Utils.CreateRuntimeCondition` for option conditions |
| Timeline | `com.unity.timeline` 1.8.7 | `TimelineAsset` cutscenes on input choices |

`DialogJsonExporter` imports `Newtonsoft.Json` and `Newtonsoft.Json.Linq`. That assembly is not listed in this `package.json`. Add `com.unity.nuget.newtonsoft-json` when the project does not already provide Newtonsoft.Json.

### Unity version

`package.json` has no `unity` or `unityRelease` field, and the assets in this repository do not record an editor version. The only Unity package version pinned here is Timeline 1.8.7. That Timeline release's own manifest sets `"unity": "2019.3"`. Unity's [Timeline 1.8 install page](https://docs.unity3d.com/Packages/com.unity.timeline@1.8/manual/install.html) describes the 1.8 documentation line as compatible with Unity 2021.3 and later.

## Installation

These Behaviour packages are git repositories, and `package.json` names Vars and HFSM by id rather than by git URL. Add all three git URLs. Unity Package Manager resolves Timeline from the Unity registry.

**Window > Package Manager > + > Add package from git URL**, once per repository:

```
https://github.com/denchi/UnityBehaviorVars.git
https://github.com/denchi/UnityBehaviorHFSM.git
https://github.com/denchi/UnityBehaviorDialogs.git
```

Or merge this into `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.deathbygravitystudio.behaviour.dialog": "https://github.com/denchi/UnityBehaviorDialogs.git",
    "com.deathbygravitystudio.behaviour.hfsm": "https://github.com/denchi/UnityBehaviorHFSM.git",
    "com.deathbygravitystudio.behaviour.vars": "https://github.com/denchi/UnityBehaviorVars.git",
    "com.unity.timeline": "1.8.7"
  }
}
```

Assemblies: `Behaviour.Dialog` (runtime) references `Behaviour.Vars`, `Behaviour.HFSM`, and `Unity.Timeline`. `Behaviour.Dialog.Editor` is editor-only and also references `Behaviour.Vars.Editor`.

## How this fits the other Behaviour packages

[UnityBehaviorVars](https://github.com/denchi/UnityBehaviorVars) is the shared variable and condition core (`com.deathbygravitystudio.behaviour.vars`). Dialog, HFSM, Graph, and Tree all sit on it.

[UnityBehaviorHFSM](https://github.com/denchi/UnityBehaviorHFSM) (`com.deathbygravitystudio.behaviour.hfsm`) is a hierarchical state machine. This dialog package calls that package's `Utils.CreateRuntimeCondition`, the same helper HFSM uses when it builds transition conditions. A `Dialog` asset is an `ILayer`: a list of values plus dialog options.

[UnityBehaviorGraph](https://github.com/denchi/UnityBehaviorGraph) (`com.deathbygravitystudio.behaviour.graph`, "Graph execution system") and [UnityBehaviorTree](https://github.com/denchi/UnityBehaviorTree) (`com.deathbygravitystudio.behaviour.tree`) are sibling packages. Each depends on UnityBehaviorVars. Dialog's assembly references are Vars, HFSM, and Timeline.

## Quick start

1. Create a dialog with **Assets > Create > Dialog**.
2. In the inspector, under **Vars**, click **Add Variable**. That creates a bool named `value`; rename it to `PlayerMet`. Click **Add option**, then **Add action**, and add a **Talk Action** (`text` and `duration`) and a **Set Action** (choose `PlayerMet`, bool constant on). Leave the option without conditions so it always runs. A condition's value reference has to be assigned before `CreateRuntimeDialog`, which reads `condition.value.name`.
3. Add the component below to a GameObject and assign the dialog. Enter Play Mode. The component runs every option whose conditions pass, in order, and feeds each action an `IDialogContext`.

`ShowSubtitle` must call `onDone`, or a Talk action waits forever. `ShowInputs` must call `onSelect` with the chosen label, or `onCancel`. The stub below logs the line and auto-picks the first choice so the coroutine can finish. Replace those methods with the game's subtitle and choice UI.

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using Behaviours.Dialogs;
using Behaviours.Dialogs.Runtime;
using Behaviours.Runtime;
using UnityEngine;
using UnityEngine.Timeline;

public class DialogQuickStart : MonoBehaviour, IDialogContext
{
    [SerializeField] Dialog dialog;

    RuntimeValueCollection _values;

    IEnumerator Start()
    {
        var runtimeDialog = RuntimeDialog.CreateRuntimeDialog(dialog);
        _values = runtimeDialog.runtimeValues;

        foreach (var runtimeOption in runtimeDialog.runtimeOptions)
        {
            if (!RuntimeDialog.EvaluateConditions(runtimeOption.runtimeConditions))
                continue;

            foreach (var action in runtimeOption.option.actions)
            {
                if (action == null)
                    continue;

                yield return action.Execute(this);
            }
        }
    }

    public RuntimeValueCollection GetRuntimeValues() => _values;

    public void ShowSubtitle(string text, float duration, Action onDone)
    {
        Debug.Log(text);
        StartCoroutine(CompleteAfter(duration, onDone));
    }

    public void HideSubtitles() { }

    public void PlayVoice(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    public void StopVoice() { }

    public void ShowInputs(string prompt, List<string> optionsNames, Action<string> onSelect, Action onCancel)
    {
        Debug.Log(prompt);
        if (optionsNames != null && optionsNames.Count > 0)
            onSelect?.Invoke(optionsNames[0]);
        else
            onCancel?.Invoke();
    }

    public void TriggerAction(string actionStateName) => Debug.Log(actionStateName);

    public void ChangeState(string actionStateName, bool value) => Debug.Log($"{actionStateName} = {value}");

    public void PlayCutscene(TimelineAsset cutscene) => Debug.Log(cutscene != null ? cutscene.name : "null");

    static IEnumerator CompleteAfter(float duration, Action onDone)
    {
        yield return new WaitForSeconds(duration);
        onDone?.Invoke();
    }
}
```

To persist the values after the dialog runs, pass any `IValueStorage` to `runtimeDialog.SaveRuntimeValues` and later `LoadRuntimeValues`.

Export the same asset from code:

```csharp
using Behaviours.Dialogs;

string json = DialogJsonExporter.ExportToJson(dialog);
DialogJsonExporter.SaveToFile(dialog, "MyDialog.json");
Dialog imported = DialogJsonExporter.ImportFromJson(json);
```

Outside the editor, assign a mapped or custom `IAssetPathResolver` before import if AudioClip and Timeline paths should resolve. `AssetPathResolverUtility.UseNullResolver()` drops those references.

## Licence

Released under the [MIT License](LICENSE). Copyright (c) 2026 Denis Chiriac / Death By Gravity Studio.
