using UnityEngine;
using System.Collections.Generic;

namespace Behaviours.Dialogs
{
    /// <summary>
    /// Utility class for configuring and managing asset path resolvers for DialogJsonExporter
    /// </summary>
    public static class AssetPathResolverUtility
    {
        /// <summary>
        /// Configure DialogJsonExporter to use the Unity Editor asset resolver (default)
        /// Best for Editor-only export/import scenarios
        /// </summary>
        public static void UseUnityEditorResolver()
        {
            DialogJsonExporter.AssetPathResolver = new UnityEditorAssetPathResolver();
        }

        /// <summary>
        /// Configure DialogJsonExporter to use a null resolver (no asset resolution)
        /// Best for scenarios where asset references should be ignored
        /// </summary>
        public static void UseNullResolver()
        {
            DialogJsonExporter.AssetPathResolver = new NullAssetPathResolver();
        }

        /// <summary>
        /// Configure DialogJsonExporter to use a mapped resolver with the provided mappings
        /// Best for runtime scenarios with predefined asset mappings
        /// </summary>
        /// <param name="pathToAssetMappings">Dictionary mapping asset paths to Unity objects</param>
        public static void UseMappedResolver(Dictionary<string, Object> pathToAssetMappings = null)
        {
            var resolver = new MappedAssetPathResolver();
            
            if (pathToAssetMappings != null)
            {
                foreach (var mapping in pathToAssetMappings)
                {
                    resolver.AddMapping(mapping.Key, mapping.Value);
                }
            }
            
            DialogJsonExporter.AssetPathResolver = resolver;
        }

        /// <summary>
        /// Configure DialogJsonExporter to use a custom resolver
        /// </summary>
        /// <param name="resolver">Custom asset path resolver implementation</param>
        public static void UseCustomResolver(IAssetPathResolver resolver)
        {
            DialogJsonExporter.AssetPathResolver = resolver;
        }

        /// <summary>
        /// Get the currently configured resolver
        /// </summary>
        /// <returns>Current IAssetPathResolver instance</returns>
        public static IAssetPathResolver GetCurrentResolver()
        {
            return DialogJsonExporter.AssetPathResolver;
        }

        /// <summary>
        /// Check if asset loading is supported by the current resolver
        /// </summary>
        /// <returns>True if current resolver supports asset loading</returns>
        public static bool IsAssetLoadingSupported()
        {
            return DialogJsonExporter.AssetPathResolver.IsAssetLoadingSupported;
        }

        /// <summary>
        /// Create a mapped resolver from a dialog's existing asset references
        /// Useful for creating runtime asset mappings from editor dialogs
        /// </summary>
        /// <param name="dialog">Dialog to extract asset references from</param>
        /// <returns>Configured MappedAssetPathResolver</returns>
        public static MappedAssetPathResolver CreateMappedResolverFromDialog(Dialog dialog)
        {
            var resolver = new MappedAssetPathResolver();
            var editorResolver = new UnityEditorAssetPathResolver();

            if (!editorResolver.IsAssetLoadingSupported || dialog == null)
                return resolver;

            // Extract asset references from dialog options
            foreach (var option in dialog.options)
            {
                foreach (var action in option.actions)
                {
                    switch (action)
                    {
                        case DialogActionTalk talk when talk.audioClip != null:
                            var talkPath = editorResolver.GetAssetPath(talk.audioClip);
                            if (!string.IsNullOrEmpty(talkPath))
                                resolver.AddMapping(talkPath, talk.audioClip);
                            break;

                        case DialogActionTalkMultiple talkMultiple when talkMultiple.audioClip != null:
                            var talkMultiplePath = editorResolver.GetAssetPath(talkMultiple.audioClip);
                            if (!string.IsNullOrEmpty(talkMultiplePath))
                                resolver.AddMapping(talkMultiplePath, talkMultiple.audioClip);
                            break;

                        case DialogAddInputAction inputAction:
                            foreach (var inputOption in inputAction.options)
                            {
                                foreach (var inputOptionAction in inputOption.actions)
                                {
                                    if (inputOptionAction.cutscene != null)
                                    {
                                        var cutscenePath = editorResolver.GetAssetPath(inputOptionAction.cutscene);
                                        if (!string.IsNullOrEmpty(cutscenePath))
                                            resolver.AddMapping(cutscenePath, inputOptionAction.cutscene);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            return resolver;
        }

        /// <summary>
        /// Helper method to log information about the current resolver
        /// </summary>
        public static void LogResolverInfo()
        {
            var resolver = GetCurrentResolver();
            var resolverType = resolver.GetType().Name;
            var supportsLoading = resolver.IsAssetLoadingSupported;
            
            Debug.Log($"DialogJsonExporter is using {resolverType} (Asset Loading Supported: {supportsLoading})");

            if (resolver is MappedAssetPathResolver mappedResolver)
            {
                var mappings = mappedResolver.GetMappedPaths();
                Debug.Log($"Mapped resolver has {System.Linq.Enumerable.Count(mappings)} asset mappings");
            }
        }
    }
}