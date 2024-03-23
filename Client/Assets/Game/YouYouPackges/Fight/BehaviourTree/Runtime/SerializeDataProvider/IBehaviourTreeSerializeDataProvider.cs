/// <summary>
/// 行为树序列化数据提供者接口
/// </summary>
public interface IBehaviourTreeSerializeDataProvider
{
    /// <summary>
    /// 序列化行为树
    /// </summary>
    void Serialize(BehaviourTree behaviourTree);
        
    /// <summary>
    /// 反序列化为行为树
    /// </summary>
    BehaviourTree Deserialize();
}