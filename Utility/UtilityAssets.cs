using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UtilityAssets : UnityEditor.AssetModificationProcessor
{
    public static MonoScript[] AllScript { get; protected set; }

    //--监听“资源即将被创建”事件
    public static void OnWillCreateAsset(string path)
    {
        if (path.Contains(".cs"))
        {
            UpdateAllScript();
        }
    }
    //--监听“资源即将被保存”事件
    public static string[] OnWillSaveAssets(string[] paths)
    {
        if (paths != null)
        {
            foreach (var VARIABLE in paths)
            {
                if (VARIABLE.Contains(".cs"))
                {
                    UpdateAllScript();
                    break;
                }
            }
        }
        return paths;
    }
    //--监听“资源即将被删除”事件
    public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions option)
    {
        if (assetPath.Contains(".cs"))
        {
            UpdateAllScript();
        }
        return AssetDeleteResult.DidNotDelete;
    }

    [InitializeOnLoadMethod]
    public static void UpdateAllScript()
    {
        AllScript = Resources.FindObjectsOfTypeAll<MonoScript>();
    }
}
