using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 节点基类
/// </summary>
[Serializable]
public abstract class BaseNode
{
    /// <summary>
    /// 节点状态
    /// </summary>
    public enum State
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Free,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 运行成功
        /// </summary>
        Success,

        /// <summary>
        /// 运行失败
        /// </summary>
        Failed,
    }

    /// <summary>
    /// 节点Id，从1开始
    /// </summary>
    public int Id;

    /// <summary>
    /// 节点位置
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// 当前节点状态
    /// </summary>
    [NonSerialized] public State CurState = State.Free;

    /// <summary>
    /// 持有此节点的行为树
    /// </summary>
    [NonSerialized] public BehaviourTree Owner;

    /// <summary>
    /// 黑板
    /// </summary>
    public BlackBoard BlackBoard => Owner.BlackBoard;

    /// <summary>
    /// 父节点
    /// </summary>
    [NonSerialized] public BaseNode ParentNode;

    /// <summary>
    /// 字段列表，重载此属性才会自动绘制节点用到的的黑板参数
    /// </summary>
    public virtual FieldInfo[] FieldInfos => Array.Empty<FieldInfo>();

    /// <summary>
    /// 添加子节点
    /// </summary>
    public abstract void AddChild(BaseNode child);

    /// <summary>
    /// 删除子节点
    /// </summary>
    public abstract void RemoveChild(BaseNode child);

    /// <summary>
    /// 清空子节点
    /// </summary>
    public abstract void ClearChild();

    /// <summary>
    /// 遍历子节点
    /// </summary>
    public abstract void ForeachChild(Action<BaseNode> action);

    /// <summary>
    /// 排序子节点
    /// </summary>
    public virtual void SortChild()
    {

    }

    /// <summary>
    /// 清空对自身Id和子节点Id的记录
    /// </summary>
    public virtual void ClearId()
    {
        Id = 0;
    }

    /// <summary>
    /// 重建对子节点的Id记录
    /// </summary>
    public abstract void RebuildId();

    /// <summary>
    /// 重建对子节点的引用
    /// </summary>
    public abstract void RebuildNodeReference(List<BaseNode> allNodes);

    /// <summary>
    /// 重建对黑板参数的引用
    /// </summary>
    public void RebuildBBParamReference()
    {
        var bbParamType = typeof(BBParam);
        foreach (var fieldInfo in FieldInfos)
        {
            if (bbParamType.IsAssignableFrom(fieldInfo.FieldType))
            {
                var bbParam = (BBParam)fieldInfo.GetValue(this);

                if (string.IsNullOrEmpty(bbParam.Key))
                {
                    //节点里的黑板参数key为空 跳过
                    continue;
                }

                BBParam newBBParam = BlackBoard.GetParam(bbParam.Key);
                if (newBBParam == null)
                {
                    Debug.LogError($"{GetType().Name}的黑板参数key:{bbParam.Key}不存于黑板中，请检查");
                    continue;
                }

                //替换节点中对黑板参数的引用为黑板中的黑板参数
                fieldInfo.SetValue(this, newBBParam);
            }
        }
    }

    /// <summary>
    /// 开始运行节点
    /// </summary>
    public void Start()
    {
        if (CurState == State.Running)
        {
            return;
        }

        CurState = State.Running;
        OnStart();
    }

    /// <summary>
    /// 取消运行节点
    /// </summary>
    public void Cancel()
    {
        if (CurState != State.Running)
        {
            return;
        }

        CurState = State.Free;
        OnCancel();
    }

    /// <summary>
    /// 结束运行节点
    /// </summary>
    protected virtual void Finish(bool success)
    {
        CurState = success ? State.Success : State.Failed;

        ParentNode?.ChildFinished(this, success);
    }

    /// <summary>
    /// 子节点运行结束
    /// </summary>
    protected void ChildFinished(BaseNode child, bool success)
    {
        OnChildFinished(child, success);
    }

    /// <summary>
    /// 开始运行行为树时调用
    /// </summary>
    public virtual void OnBehaviourTreeStart()
    {

    }

    /// <summary>
    /// 开始运行节点时调用
    /// </summary>
    protected abstract void OnStart();

    /// <summary>
    /// 取消运行节点时调用
    /// </summary>
    protected abstract void OnCancel();

    /// <summary>
    /// 子节点运行结束时
    /// </summary>
    protected abstract void OnChildFinished(BaseNode child, bool success);

    public override string ToString()
    {
        return GetType().Name;
    }
}
