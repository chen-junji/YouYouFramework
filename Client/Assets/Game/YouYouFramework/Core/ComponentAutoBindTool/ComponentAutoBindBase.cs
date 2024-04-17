using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ComponentAutoBindTool))]//脚本依赖
public class ComponentAutoBindBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        GetBindComponents(gameObject);
    }
    protected virtual void GetBindComponents(GameObject go)
    {
    }
}
