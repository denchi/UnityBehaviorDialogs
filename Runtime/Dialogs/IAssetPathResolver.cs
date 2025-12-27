using UnityEngine;

namespace Behaviours.Dialogs
{
    /// <summary>
    /// Interface for resolving asset paths to actual Unity object references
    /// </summary>
    public interface IAssetPathResolver
    {
        /// <summary>
        /// Get the asset path for a given Unity object
        /// </summary>
        /// <param name="obj">Unity object to get path for</param>
        /// <returns>Asset path string, or null if object is null or has no path</returns>
        string GetAssetPath(Object obj);

        /// <summary>
        /// Load an asset of type T from the given path
        /// </summary>
        /// <typeparam name="T">Type of asset to load</typeparam>
        /// <param name="path">Asset path to load from</param>
        /// <returns>Loaded asset, or null if not found or invalid type</returns>
        T LoadAssetAtPath<T>(string path) where T : Object;

        /// <summary>
        /// Check if asset loading is supported in the current context
        /// </summary>
        bool IsAssetLoadingSupported { get; }
    }
}