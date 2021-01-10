using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UtilityAssets : UnityEditor.AssetModificationProcessor
{
    public static MonoScript[] AllScript { get; protected set; }

    //--��������Դ�������������¼�
    public static void OnWillCreateAsset(string path)
    {
        if (path.Contains(".cs"))
        {
            UpdateAllScript();
        }
    }
    //--��������Դ���������桱�¼�
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
    //--��������Դ������ɾ�����¼�
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
