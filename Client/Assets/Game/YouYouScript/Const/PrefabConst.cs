using System.Collections.Generic;



public enum PrefabName : uint
{
    zhujiao_cike_animation,
}
public struct PrefabEntity
{
    public PrefabName PrefabName;

    /// <summary>
    /// 路径
    /// </summary>
    public string AssetPath;

    /// <summary>
    /// 对象池编号
    /// </summary>
    public byte PoolId;

    /// <summary>
    /// 是否开启缓存池自动清理模式
    /// </summary>
    public bool CullDespawned;

    /// <summary>
    /// 缓存池自动清理但是始终保留几个对象不清理
    /// </summary>
    public int CullAbove;

    /// <summary>
    /// 多长时间清理一次单位是秒
    /// </summary>
    public int CullDelay;

    /// <summary>
    /// 每次清理几个
    /// </summary>
    public int CullMaxPerPass;

    public PrefabEntity(PrefabName prefabName, string assetPath, byte poolID = 1, bool cullDespawned = true, byte cullAove = 0, int cullDelay = 30, int cullMaxPerPass = 100)
    {
        PrefabName = prefabName;
        AssetPath = assetPath;
        PoolId = poolID;
        CullDespawned = cullDespawned;
        CullAbove = cullAove;
        CullDelay = cullDelay;
        CullMaxPerPass = cullMaxPerPass;
    }
}

public class PrefabConst
{
    private static Dictionary<PrefabName, PrefabEntity> dic = new Dictionary<PrefabName, PrefabEntity>();

    public PrefabConst()
    {
        void AddDic(PrefabEntity entity)
        {
            dic.Add(entity.PrefabName, entity);
        }

        //配置对象池的预制体
        AddDic(new PrefabEntity(PrefabName.zhujiao_cike_animation, "Role/RoleSources/cike/zhujiao_cike_animation"));
    }

    public static PrefabEntity GetDic(PrefabName name)
    {
        return dic[name];
    }
}