using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace History
{
    public class HistoryCache
    {
        public static Dictionary<string, (object asset, int staleIndex)> loadedAssets = new Dictionary<string, (object asset, int staleIndex)> ();

        public static T TryLoadObject<T> (string key)
        {
            object resource = null;
            if(loadedAssets.ContainsKey(key))
            {
                resource = (T)loadedAssets[key].asset;
            }
            else
            {
                resource = Resources.Load(key);
                if(resource != null)
                {
                    loadedAssets[key] = (resource, 0);
                }
            }
            if(resource != null)
            {
                if (resource is T)
                    return (T)resource;
                else
                    Debug.LogWarning($"retrieved object '{key}' was not the expected type");
            }
            Debug.LogWarning($"could not load object from cache '{key}'");
            return default(T);
        }

        public static TMP_FontAsset LoadFont(string key) => TryLoadObject<TMP_FontAsset>(key);
        public static Texture2D LoadImage(string key) => TryLoadObject<Texture2D>(key);

    }
}