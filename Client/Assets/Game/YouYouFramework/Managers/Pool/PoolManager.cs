using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace YouYouFramework
{
    /// <summary>
    /// 池管理器
    /// </summary>
    public class PoolManager
    {
        /// <summary>
        /// 游戏物体对象池
        /// </summary>
        public GameObjectPool GameObjectPool { get; private set; }

        /// <summary>
        /// 类对象池
        /// </summary>
        public ClassObjectPool ClassObjectPool { get; private set; }

        /// <summary>
        /// 变量对象池
        /// </summary>
        public VarObjectPool VarObjectPool {  get; private set; }

        internal PoolManager()
        {
            GameObjectPool = new GameObjectPool();
            ClassObjectPool = new ClassObjectPool();
            VarObjectPool = new VarObjectPool();
        }
        internal void OnUpdate()
        {
            ClassObjectPool.OnUpdate();
        }
        internal void Init()
        {
            GameObjectPool.Init();
            ClassObjectPool.Init();
        }
    }
}