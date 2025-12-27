using UnityEngine;

namespace Behaviours.Dialogs
{
    /// <summary>
    /// Unity Editor implementation of asset path resolver using AssetDatabase
    /// </summary>
    public class UnityEditorAssetPathResolver : IAssetPathResolver
    {
        public string GetAssetPath(Object obj)
        {
            if (obj == null) return null;

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
            return null;
#endif
        }

        public T LoadAssetAtPath<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) return null;

#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }

        public bool IsAssetLoadingSupported
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
    }
}