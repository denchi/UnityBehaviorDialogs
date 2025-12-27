# Dialog JSON Export/Import Feature

This feature allows you to export Dialog assets to JSON format and import them back, enabling:
- Easy backup and version control of dialog content
- Sharing dialogs between projects
- External editing of dialog content
- Automated dialog generation from external tools

## Features

### Supported Dialog Elements

✅ **Dialog Properties**
- Name and enabled state
- Values (variables) with all types (Bool, Integer, Float, String, Other)

✅ **Dialog Options**
- Multiple options per dialog
- Conditions with operations and constants
- Actions of all supported types

✅ **Dialog Actions**
- `DialogActionTalk` - Single text with duration and audio clip
- `DialogActionTalkMultiple` - Multiple text entries with individual durations and audio clip
- `DialogActionSet` - Variable assignment with all constant types
- `DialogAddInputAction` - Input prompts with multiple options and actions (including cutscene references)

✅ **Unity Asset References**
- AudioClip references in talk actions are preserved via asset paths
- TimelineAsset references in input actions are preserved via asset paths
- Asset references are automatically restored during import
- Configurable asset resolution system supports different scenarios

## Asset Path Resolver System

The JSON exporter uses a configurable asset path resolver system that allows different implementations for different scenarios:

### **Available Resolvers:**

#### **UnityEditorAssetPathResolver (Default)**
- Uses Unity's AssetDatabase for asset path resolution
- Full support for all asset types in Unity Editor
- Best for: Editor-only workflows, development

#### **NullAssetPathResolver** 
- Ignores all asset references (no asset loading)
- Asset paths are not saved or restored
- Best for: Runtime scenarios without asset support

#### **MappedAssetPathResolver**
- Uses predefined mappings between paths and assets
- Allows custom asset resolution at runtime
- Best for: Runtime scenarios with known asset mappings

#### **Custom Resolvers**
- Implement `IAssetPathResolver` for custom logic
- Full control over asset path handling
- Best for: Specialized scenarios

### **Configuration Examples:**

```csharp
// Use Unity Editor resolver (default)
AssetPathResolverUtility.UseUnityEditorResolver();

// Use null resolver (ignore assets)
AssetPathResolverUtility.UseNullResolver();

// Use mapped resolver with custom mappings
var mappings = new Dictionary<string, Object> {
    { "Assets/Audio/sound.wav", myAudioClip },
    { "Assets/Cutscenes/intro.playable", myCutscene }
};
AssetPathResolverUtility.UseMappedResolver(mappings);

// Create mapped resolver from existing dialog
var resolver = AssetPathResolverUtility.CreateMappedResolverFromDialog(myDialog);
DialogJsonExporter.AssetPathResolver = resolver;

// Use custom resolver
DialogJsonExporter.AssetPathResolver = new MyCustomResolver();
```

## Usage

### 1. Using the Unity Editor GUI

#### Context Menu (Recommended)
1. Right-click on a Dialog asset in the Project window
2. Select **"Dialog JSON" → "Export to JSON"** to save as JSON file
3. Select **"Dialog JSON" → "Import from JSON"** to create a new Dialog from JSON

#### Inspector Buttons
1. Select a Dialog asset
2. In the Inspector, scroll down to find the "JSON Export/Import" section
3. Click **"Export to JSON"** or **"Import from JSON"**

#### JSON Validation
1. Right-click on a JSON file in the Project window
2. Select **"Dialog JSON" → "Validate JSON"** to check if it's a valid Dialog JSON

### 2. Using Code (Runtime or Editor)

```csharp
using Behaviours.Dialogs;

// Export dialog to JSON string
var jsonString = DialogJsonExporter.ExportToJson(myDialog);

// Import dialog from JSON string
var importedDialog = DialogJsonExporter.ImportFromJson(jsonString);

// Save dialog to file
DialogJsonExporter.SaveToFile(myDialog, "/path/to/dialog.json");

// Load dialog from file
var loadedDialog = DialogJsonExporter.LoadFromFile("/path/to/dialog.json");
```

### 3. Example Scripts

An example script `DialogJsonExample.cs` is included in the `Runtime/Examples/` folder that demonstrates:
- Programmatic export/import
- Creating sample dialogs
- Round-trip testing
- Runtime usage patterns

## JSON Format

The JSON format preserves all dialog structure and data:

```json
{
  "name": "My Dialog",
  "enabled": true,
  "values": [
    {
      "name": "PlayerMet",
      "type": 0,
      "value": false
    }
  ],
  "options": [
    {
      "conditions": [
        {
          "operation": 0,
          "valueName": "PlayerMet",
          "bConstant": false
        }
      ],
      "actions": [
        {
          "type": "DialogActionTalk",
          "text": "Hello! Nice to meet you.",
          "duration": 2.0
        },
        {
          "type": "DialogActionSet",
          "varName": "PlayerMet",
          "bConstant": true
        }
      ]
    }
  ]
}
```

## Value Types Reference

- `0` = Bool
- `1` = Integer  
- `2` = Float
- `3` = String
- `4` = Other

## Operation Types Reference

- `0` = Is (==)
- `1` = IsNot (!=)
- `2` = Greater (>)
- `3` = GreaterOrEqual (>=)
- `4` = Less (<)
- `5` = LessOrEqual (<=)

## Best Practices

### Version Control
- Store JSON files in version control alongside your Unity project
- Use descriptive filenames that include version numbers or dates
- Consider using separate JSON files for different dialog sections

### External Editing
- JSON files can be edited in any text editor
- Use JSON validation tools to ensure proper format
- Test imported dialogs thoroughly after external modifications

### Backup Strategy
- Export dialogs to JSON before major changes
- Keep backup copies of important dialog JSON files
- Use the round-trip test functionality to verify data integrity

### Performance Considerations
- JSON import creates new ScriptableObject instances
- Large dialogs may take time to import
- Consider splitting very large dialogs into smaller, manageable pieces

## Troubleshooting

### Common Issues

**Import fails with "null" result:**
- Check console for error messages
- Validate JSON format using online JSON validators
- Ensure all required fields are present

**Asset references not restored after import:**
- Ensure the referenced assets still exist at their original paths
- Asset path restoration only works in the Unity Editor (not at runtime)
- Check console for warnings about missing assets during import
- Asset paths are relative to the project root and must be valid Unity asset paths

**Values not linking properly:**
- Ensure value names in conditions match existing value names exactly
- Check for case sensitivity and special characters

**Actions not importing correctly:**
- Verify the action "type" field matches exact class names
- Ensure all required fields for each action type are present

### Getting Help

1. Enable detailed logging by checking console output during import/export
2. Use the "Validate JSON" feature to verify file integrity
3. Try the round-trip test with known working dialogs
4. Check the example script for reference implementations

## Technical Details

- Uses Newtonsoft.Json for JSON serialization
- Preserves Unity object relationships through asset database
- Maintains dialog structure integrity during round-trip operations
- Supports all current dialog action types with extensible architecture