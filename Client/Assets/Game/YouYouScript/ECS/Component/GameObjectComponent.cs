using Entitas;
using UnityEngine;

namespace Assets.Scripts.ECS.Component
{
    [Game]
    public class GameObjectComponent : IComponent
    {
        /// <summary>
        /// 父物体容器
        /// </summary>
        public GameObject Container;
        /// <summary>
        /// 演员模型
        /// </summary>
        public GameObject Model;
    }
}
