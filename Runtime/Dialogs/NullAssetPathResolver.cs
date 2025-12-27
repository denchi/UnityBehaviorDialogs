using UnityEngine;

namespace Behaviours.Dialogs
{
    /// <summary>
    /// Null implementation of asset path resolver that doesn't resolve any assets
    /// Useful for runtime scenarios where asset loading from paths is not supported
    /// </summary>
    public class NullAssetPathResolver : IAssetPathResolver
    {
        public string GetAssetPath(Object obj)
        {
            return null;
        }

        public T LoadAssetAtPath<T>(string path) where T : Object
        {
            return null;
        }

        public bool IsAssetLoadingSupported => false;
    }
}