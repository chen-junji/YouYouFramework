using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YouYouFramework
{
    public static class InstanceHandler
    {
        public delegate GameObject InstantiateDelegate(PrefabPool prefabPool);
        public delegate void DestroyDelegate(GameObject instance, PrefabPool prefabPool);

        /// <summary>
        /// 可以用来拦截Instantiate来实现你自己的处理
        /// </summary>
        public static InstantiateDelegate InstantiateDelegates;

        /// <summary>
        /// 可以用来拦截Destroys来实现你自己的处理
        /// </summary>
        public static DestroyDelegate DestroyDelegates;

        /// <summary>
        /// 克隆对象
        /// </summary>
        internal static GameObject InstantiatePrefab(PrefabPool prefabPool)
        {
            if (InstantiateDelegates != null)
            {
                return InstantiateDelegates(prefabPool);
            }
            else
            {
                return Object.Instantiate(prefabPool.prefab);
            }
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        internal static void DestroyInstance(GameObject instance, PrefabPool prefabPool)
        {
            if (DestroyDelegates != null)
            {
                DestroyDelegates(instance, prefabPool);
            }
            else
            {
                Object.Destroy(instance);
            }
        }

    }
}
