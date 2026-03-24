using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleLoader
{
    // 存储已加载的AssetBundle，键为路径，值为AssetBundle对象
    private static Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();
    
    //参数1是AssetBundle的路径，参数2是资源的名称
	public static GameObject LoadAssetBundle(string path, string name)
    {
        // 检查是否已经加载了该AssetBundle
        if (!loadedAssetBundles.ContainsKey(path))
        {
            // 未加载过，需要加载
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            
            if (ab == null)
            {
                Debug.LogError("Failed to load AssetBundle from path: " + path);
                return null;
            }
            
            // 将加载的AssetBundle存储起来
            loadedAssetBundles.Add(path, ab);
        }
        
        // 从已加载的AssetBundle中获取资源
        return loadedAssetBundles[path].LoadAsset<GameObject>(name);
    }
    
    // 提供一个方法来卸载特定的AssetBundle
    public static void UnloadAssetBundle(string path, bool unloadAllLoadedObjects = false)
    {
        if (loadedAssetBundles.ContainsKey(path))
        {
            loadedAssetBundles[path].Unload(unloadAllLoadedObjects);
            loadedAssetBundles.Remove(path);
        }
    }
    
    // 提供一个方法来卸载所有AssetBundle
    public static void UnloadAllAssetBundles(bool unloadAllLoadedObjects = false)
    {
        foreach (var ab in loadedAssetBundles.Values)
        {
            ab.Unload(unloadAllLoadedObjects);
        }
        loadedAssetBundles.Clear();
    }
}

