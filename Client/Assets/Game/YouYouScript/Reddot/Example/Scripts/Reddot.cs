using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// 主要用于初始化红点树
/// </summary>
public class Reddot : MonoBehaviour
{
    public bool IsRoot;

    [Header("可选：服务端ID可以映射路径")]
    public int serverId;

    void Awake()
    {
        //获取自身路径，动态添加节点
        string Path = GetPath(this);
        if (serverId > 0)
        {
            ReddotManager.Instance.SetServerIdOfPath(serverId, Path);
        }
        else
        {
            ReddotManager.Instance.GetTreeNode(Path);
        }
        Debug.Log("初始化红点==" + Path);
    }

    private static string GetPath(Reddot reddot)
    {
        if (reddot.transform.parent == null)
        {
            return reddot.name;
        }

        Reddot parentReddot = reddot.transform.parent.GetComponent<Reddot>();
        if (parentReddot == null)
        {
            return reddot.name;
        }

        if (parentReddot.IsRoot)
        {
            return parentReddot.name + "/" + reddot.name;
        }

        return GetPath(parentReddot) + "/" + reddot.name;
    }
}
