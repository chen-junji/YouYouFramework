using UnityEngine;
using System.Collections;
using System;

namespace Hotfix
{
    public class Singleton<T> where T : new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }

        public virtual void Dispose()
        {

        }
    }
}