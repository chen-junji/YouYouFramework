using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    public enum GameObjectPoolId
    {
        Audio = 1,
        Common = 2,
    }
    /// <summary>
    /// 对象池实体
    /// </summary>
    [System.Serializable]
    public class GameObjectPoolEntity
    {
        [Header("跨场景不销毁")]
        public bool IsGlobal;

        /// <summary>
        /// 对象池编号
        /// </summary>
        public GameObjectPoolId PoolId;

        /// <summary>
        /// 对象池名字
        /// </summary>
        public string PoolName;

        /// <summary>
        /// 对应的游戏物体对象池
        /// </summary>
        [HideInInspector]
        public SpawnPool Pool;

    }
}