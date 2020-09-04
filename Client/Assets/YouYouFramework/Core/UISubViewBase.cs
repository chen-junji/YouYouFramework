using UnityEngine;
using System.Collections;


namespace YouYou
{
    public class UISubViewBase : MonoBehaviour
    {
        void Awake()
        {
            OnAwake();
        }

        void Start()
        {
            OnStart();
        }

        void OnDestroy()
        {
            BeforeOnDestroy();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnStart() { }

        protected virtual void BeforeOnDestroy() { }

    }
}