using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace YouYou
{
    public static class InstanceHandler
    {
        public delegate GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot);
        public delegate void DestroyDelegate(GameObject instance);

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
        internal static GameObject InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion rot)
        {
            if (InstantiateDelegates != null)
            {
                return InstantiateDelegates(prefab, pos, rot);
            }
            else
            {
                return Object.Instantiate(prefab, pos, rot);
            }
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        internal static void DestroyInstance(GameObject instance)
        {
            if (DestroyDelegates != null)
            {
                DestroyDelegates(instance);
            }
            else
            {
                Object.Destroy(instance);
            }
        }

    }
}
