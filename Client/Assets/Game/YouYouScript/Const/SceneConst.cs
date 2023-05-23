using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct SceneEntity
{
    public SceneGroupName SceneGroupName;

    /// <summary>
    /// 多场景路径
    /// </summary>
    public List<string> AssetPathList;

    /// <summary>
    /// 背景音乐
    /// </summary>
    public BGMName BGMName;

    public SceneEntity(SceneGroupName sceneGroupName, List<string> assetPathList, BGMName bgmName = BGMName.None)
    {
        SceneGroupName = sceneGroupName;
        AssetPathList = assetPathList;
        BGMName = bgmName;
    }
}

public class SceneConst
{
    private static Dictionary<SceneGroupName, SceneEntity> dic = new Dictionary<SceneGroupName, SceneEntity>();

    public SceneConst()
    {
        void AddDic(SceneEntity entity)
        {
            dic.Add(entity.SceneGroupName, entity);
        }

        //配置对象池的预制体
        AddDic(new SceneEntity(SceneGroupName.Main, new List<string> { "Assets/Game/Download/Scenes/Main_1.unity" }, BGMName.maintheme1));
    }

    public static SceneEntity GetDic(SceneGroupName name)
    {
        return dic[name];
    }
}
public enum SceneGroupName : uint
{
    None,
    Main,
}