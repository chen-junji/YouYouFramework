using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 行为树节点View
/// </summary>
public class NodeView : Node
{
    private static Color inputPortColor = Color.cyan;
    private static Color outputPortColor = Color.cyan;

    private static Color compositeColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);
    private static Color decoratorColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);
    private static Color actionColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);

    private static Color freeColor = new Color(81/255f, 81/255f, 81/255f, 255f/255);
    private static Color runningColor = new Color(38 / 255f, 130 / 255f, 205 / 255f, 255f / 255);
    private static Color successColor = new Color(36 / 255f, 178 / 255f, 50 / 255f, 255f / 255);
    private static Color failedColor = new Color(203 / 255f, 81 / 255f, 61 / 255f, 255f / 255);
    
    private BehaviourTreeWindow window;
    private Port inputPort;
    private Port outputPort;
    private EnumField stateField;

    public BaseNode RuntimeNode;

    /// <summary>
    /// 获取节点在节点图的名字
    /// </summary>
    public static string GetNodeDisplayName(Type type)
    {
        NodeInfoAttribute nodeInfo = type.GetCustomAttribute<NodeInfoAttribute>();
        string[] parts = nodeInfo.Name.Split('/');
        string name = parts[parts.Length - 1];
        return name;
    }

    /// <summary>
    /// 创建节点
    /// </summary>
    public static NodeView Create(Type type, BehaviourTreeWindow window, Vector2 pos)
    {
        window.RecordObject($"Create {type.Name} Node");
        
        BaseNode runtimeNode = window.GraphView.BTSO.CreateNode(type);
        runtimeNode.Position = pos;

        NodeView nodeView = new NodeView();
        nodeView.Init(window, runtimeNode);
        window.GraphView.AddElement(nodeView);

        return nodeView;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(BehaviourTreeWindow window, BaseNode runtimeNode)
    {
        this.window = window;
        RuntimeNode = runtimeNode;

        if (RuntimeNode is RootNode)
        {
            //根节点不可删除
            capabilities -= Capabilities.Deletable;
        }

        //注册点击事件
        RegisterCallback<MouseDownEvent>((evt =>
        {
            if (evt.button == 0)
            { 
                window.OnNodeClick(this);
            }
        }));

        SetNameAndPos();
        SetVertical();
        AddPort();
        SetNodeColor();
        
        if (window.IsDebugMode)
        {
            //调试模式下 增加节点状态显示
            AddStateField();
            RefreshNodeState();
        }
    }

    /// <summary>
    /// 设置节点名和位置
    /// </summary>
    private void SetNameAndPos()
    {
        Type nodeType = RuntimeNode.GetType();
        title = GetNodeDisplayName(nodeType);
        SetPosition(new Rect(RuntimeNode.Position, GetPosition().size));
    }

    /// <summary>
    /// 将端口方向改成垂直的
    /// </summary>
    private void SetVertical()
    {
        var titleButtonContainer = contentContainer.Q<VisualElement>("title-button-container");
        titleButtonContainer.RemoveAt(0); //删掉收起箭头 否则会有bug

        var titleContainer = this.Q<VisualElement>("title");
        var topContainer = this.Q("input");
        var bottomContainer = this.Q("output");

        var nodeBorder = this.Q<VisualElement>("node-border");
        nodeBorder.RemoveAt(0);
        nodeBorder.RemoveAt(0);

        nodeBorder.Add(topContainer);
        nodeBorder.Add(titleContainer);
        nodeBorder.Add(bottomContainer);
    }

    /// <summary>
    /// 根据节点类型添加端口
    /// </summary>
    private void AddPort()
    {
        if (!(RuntimeNode is RootNode))
        {
            inputPort = PortView.Create(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(PortView));
            inputPort.portName = "父节点";
            inputPort.portColor = inputPortColor;
            inputContainer.Add(inputPort);
        }

        var capacityInfo = RuntimeNode.GetType().GetCustomAttribute<ChildCapacityInfoAttribute>();
        if (capacityInfo == null || capacityInfo.Capacity == ChildCapacity.None)
        {
            return;
        }

        Port.Capacity outputCount;
        if (capacityInfo.Capacity == ChildCapacity.Single)
        {
            outputCount = Port.Capacity.Single;
        }
        else
        {
            outputCount = Port.Capacity.Multi;
        }

        outputPort = PortView.Create(Orientation.Vertical, Direction.Output, outputCount, typeof(PortView));
        outputPort.portName = "子节点";
        outputPort.portColor = outputPortColor;
        outputContainer.Add(outputPort);
    }


    /// <summary>
    /// 设置节点颜色
    /// </summary>
    private void SetNodeColor()
    {
        Color borderColor = default;

        if (RuntimeNode is BaseActionNode)
        {
            borderColor = actionColor;
        }
        else if (RuntimeNode is BaseCompositeNode)
        {
            borderColor = compositeColor;
        }
        else if (RuntimeNode is BaseDecoratorNode)
        {
            borderColor = decoratorColor;
        }
        else
        {
            return;
        }

        var nodeBorder = this.Q<VisualElement>("node-border");
        nodeBorder.style.borderTopColor = borderColor;
        nodeBorder.style.borderBottomColor = borderColor;
        nodeBorder.style.borderLeftColor = borderColor;
        nodeBorder.style.borderRightColor = borderColor;
    }
    
    /// <summary>
    /// 添加状态显示UI
    /// </summary>
    private void AddStateField()
    {
        var nodeBorder = contentContainer.Q<VisualElement>("node-border");
        stateField = new EnumField(RuntimeNode.CurState);
        nodeBorder.Insert(2,stateField);

        IMGUIContainer imguiContainer = new IMGUIContainer();
        imguiContainer.onGUIHandler += RefreshNodeState;
        nodeBorder.Add(imguiContainer);
    }
    
    /// <summary>
    /// 刷新节点状态显示
    /// </summary>
    private void RefreshNodeState()
    {
        stateField.value = RuntimeNode.CurState;
            
        var element = stateField.ElementAt(0);

        Color color = default;
        switch (RuntimeNode.CurState)
        {
            case BaseNode.State.Free:
                color = freeColor;
                break;
            case BaseNode.State.Running:
                color = runningColor;
                break;
            case BaseNode.State.Success:
                color = successColor;
                break;
            case BaseNode.State.Failed:
                color = failedColor;
                break;
        }
            
        element.style.backgroundColor = color;
    }
    
    /// <summary>
    /// 设置位置
    /// </summary>
    public void SetPos(Rect newPos)
    {
        window.RecordObject($"SetPosition {this}");
        
        SetPosition(newPos);
        RuntimeNode.Position = newPos.position;
    }

    /// <summary>
    /// 添加子节点
    /// </summary>
    public void AddChild(NodeView child)
    {
        var info = RuntimeNode.GetType().GetCustomAttribute<ChildCapacityInfoAttribute>();
        if (info == null || info.Capacity == ChildCapacity.None)
        {
            return;
        }
        
        window.RecordObject($"AddChild {this}");

        //如果当前节点的子节点容量为single 就先清空子节点
        if (info.Capacity == ChildCapacity.Single)
        {
            ClearChild();
        }

        //如果要添加的子节点已有父节点了 就将它从旧的父节点那里删掉
        if (child.inputPort.connected)
        {
            var edgeToOldParent = child.inputPort.connections.First();
            var oldParent = (NodeView)edgeToOldParent.output.node;
            oldParent.RemoveChild(child);
        }

        //添加子节点
        RuntimeNode.AddChild(child.RuntimeNode);
        var edge = outputPort.ConnectTo(child.inputPort);
        window.GraphView.AddElement(edge);
        
        window.BuildNodeId();
    }

    /// <summary>
    /// 删除子节点
    /// </summary>
    public void RemoveChild(NodeView child)
    {
        window.RecordObject($"RemoveChild {this}");
        
        RuntimeNode.RemoveChild(child.RuntimeNode);

        var edge = child.inputPort.connections.First();
        outputPort.Disconnect(edge);

        window.GraphView.RemoveElement(edge);
        
        window.BuildNodeId();
    }

    /// <summary>
    /// 清空子节点
    /// </summary>
    public void ClearChild()
    {
        window.RecordObject($"ClearChild {this}");
        
        RuntimeNode.ClearChild();
        if (outputPort != null)
        {
            //遍历output端口的所有线 让线的input端口都断开连接 并删除线
            foreach (Edge edge in outputPort.connections.ToList())
            {
                edge.input.DisconnectAll();
                window.GraphView.RemoveElement(edge);
            }

            //断开output断开的所有连接
            outputPort.DisconnectAll();
        }
        
        window.BuildNodeId();
    }

    /// <summary>
    /// 删除节点
    /// </summary>
    public void RemoveSelf()
    {
        window.RecordObject($"RemoveNode {this}");
        
        window.GraphView.BTSO.RemoveNode(RuntimeNode);
    }

    public override string ToString()
    {
        return RuntimeNode.ToString();
    }

}