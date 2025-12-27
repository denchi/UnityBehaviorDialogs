using UnityEngine;
using System.Collections.Generic;

namespace Behaviours.Dialogs
{
    /// <summary>
    /// Asset path resolver that uses a predefined mapping dictionary
    /// Useful for runtime scenarios where you want to provide asset mappings
    /// </summary>
    public class MappedAssetPathResolver : IAssetPathResolver
    {
        private readonly Dictionary<string, Object> _pathToAssetMap;
        private readonly Dictionary<Object, string> _assetToPathMap;

        public MappedAssetPathResolver()
        {
            _pathToAssetMap = new Dictionary<string, Object>();
            _assetToPathMap = new Dictionary<Object, string>();
        }

        /// <summary>
        /// Add a mapping between an asset path and a Unity object
        /// </summary>
        /// <param name="path">Asset path</param>
        /// <param name="asset">Unity object</param>
        public void AddMapping(string path, Object asset)
        {
            if (string.IsNullOrEmpty(path) || asset == null) return;

            _pathToAssetMap[path] = asset;
            _assetToPathMap[asset] = path;
        }

        /// <summary>
        /// Remove a mapping for the given path
        /// </summary>
        /// <param name="path">Asset path to remove</param>
        public void RemoveMapping(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (_pathToAssetMap.TryGetValue(path, out var asset))
            {
                _pathToAssetMap.Remove(path);
                _assetToPathMap.Remove(asset);
            }
        }

        /// <summary>
        /// Remove a mapping for the given asset
        /// </summary>
        /// <param name="asset">Asset to remove mapping for</param>
        public void RemoveMapping(Object asset)
        {
            if (asset == null) return;

            if (_assetToPathMap.TryGetValue(asset, out var path))
            {
                _assetToPathMap.Remove(asset);
                _pathToAssetMap.Remove(path);
            }
        }

        /// <summary>
        /// Clear all mappings
        /// </summary>
        public void ClearMappings()
        {
            _pathToAssetMap.Clear();
            _assetToPathMap.Clear();
        }

        /// <summary>
        /// Get all mapped paths
        /// </summary>
        /// <returns>Collection of mapped asset paths</returns>
        public IEnumerable<string> GetMappedPaths()
        {
            return _pathToAssetMap.Keys;
        }

        /// <summary>
        /// Check if a path has a mapping
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if path is mapped</returns>
        public bool HasMapping(string path)
        {
            return !string.IsNullOrEmpty(path) && _pathToAssetMap.ContainsKey(path);
        }

        /// <summary>
        /// Check if an asset has a mapping
        /// </summary>
        /// <param name="asset">Asset to check</param>
        /// <returns>True if asset is mapped</returns>
        public bool HasMapping(Object asset)
        {
            return asset != null && _assetToPathMap.ContainsKey(asset);
        }

        public string GetAssetPath(Object obj)
        {
            if (obj == null) return null;
            
            _assetToPathMap.TryGetValue(obj, out var path);
            return path;
        }

        public T LoadAssetAtPath<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) return null;

            if (_pathToAssetMap.TryGetValue(path, out var asset))
            {
                return asset as T;
            }

            return null;
        }

        public bool IsAssetLoadingSupported => true;
    }
}