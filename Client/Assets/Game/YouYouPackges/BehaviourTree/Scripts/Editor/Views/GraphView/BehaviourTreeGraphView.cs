using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 行为树节点图
/// </summary>
public class BehaviourTreeGraphView : GraphView
{
    public new class UxmlFactory : UxmlFactory<BehaviourTreeGraphView, UxmlTraits>
    {
    }

    public BehaviourTreeSO BTSO;
    private BehaviourTreeWindow window;
    public BlackBoardView BlackboardView;

    public List<CommentBlockView> CommentBlockViews = new List<CommentBlockView>();

    /// <summary>
    /// 黑板变化事件
    /// </summary>
    public event Action OnBlackBoardChanged;

    public BehaviourTreeGraphView()
    {
        graphViewChanged = GraphViewChangedCallback;

        //这三个回调实现节点的复制粘贴功能
        serializeGraphElements = SerializeGraphElementsCallback;
        canPasteSerializedData = CanPasteSerializedDataCallback;
        unserializeAndPaste = UnserializeAndPasteCallback;

        Insert(0, new GridBackground()); //格子背景

        //添加背景网格样式
        var styleSheet = Resources.Load<StyleSheet>("USS/BehaviourTreeWindow");
        styleSheets.Add(styleSheet);

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); //可缩放
        this.AddManipulator(new SelectionDragger()); //节点可拖动
        this.AddManipulator(new ContentDragger()); //节点图可移动
        this.AddManipulator(new RectangleSelector()); //可框选多个节点
    }

    private GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
    {
        //移动
        if (changes.movedElements != null)
        {
            foreach (GraphElement element in changes.movedElements)
            {
                if (element is NodeView nodeView)
                {
                    nodeView.SetPos(nodeView.GetPosition());
                }
                else if (element is CommentBlockView commentBlockView)
                {
                    //同步注释块内的节点位置 否则因为移动注释块导致的节点位置变化是不被记录的
                    foreach (var innerNodeView in commentBlockView.NodeViews)
                    {
                        innerNodeView.SetPos(innerNodeView.GetPosition());
                    }
                }
            }
        }

        //删除
        if (changes.elementsToRemove != null)
        {
            foreach (GraphElement element in changes.elementsToRemove.ToList())
            {
                //删除注释块时 不能删除注释块内的节点和线 除非这个节点也被选中了
                if (element is CommentBlockView commentBlockView)
                {
                    foreach (NodeView innerNodeView in commentBlockView.NodeViews)
                    {
                        if (!selection.Contains(innerNodeView))
                        {
                            changes.elementsToRemove.Remove(innerNodeView);

                            if (innerNodeView.outputContainer.childCount > 0)
                            {
                                var outputPort = innerNodeView.outputContainer[0] as PortView;
                                foreach (Edge connection in outputPort.connections)
                                {
                                    changes.elementsToRemove.Remove(connection);
                                }
                            }

                            if (innerNodeView.inputContainer.childCount > 0)
                            {
                                var inputPort = innerNodeView.inputContainer[0] as PortView;
                                if (inputPort.connected)
                                {
                                    var connection = inputPort.connections.First();
                                    changes.elementsToRemove.Remove(connection);
                                }
                            }
                        }
                    }
                }
            }

            foreach (GraphElement element in changes.elementsToRemove)
            {
                if (element is NodeView nodeView)
                {
                    nodeView.RemoveSelf();
                    if (window.NodeInspector.CurSelectNodeView == nodeView)
                    {
                        window.NodeInspector.CurSelectNodeView = null;
                        window.NodeInspector.Clear();
                    }
                }
                else if (element is Edge edge)
                {
                    var parentNode = (NodeView)edge.output.node;
                    var childNode = (NodeView)edge.input.node;
                    parentNode.RemoveChild(childNode);
                }
                else if (element is CommentBlockView commentBlockView)
                {
                    commentBlockView.RemoveSelf();
                }
            }
        }


        return changes;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);

        Vector2 position =
            (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
        evt.menu.InsertAction(1, "创建注释块", (e) =>
        {
            CommentBlockView commentBlockView = CommentBlockView.Create(window, position);

            //将当前选中的节点放入注释块内
            foreach (var selectable in selection)
            {
                if (selectable is NodeView nodeView)
                {
                    if (CommentBlockViews.Exists((x => x.ContainsElement(nodeView))))
                    {
                        //已被其他注释块包含的节点 需要跳过
                        continue;
                    }

                    commentBlockView.AddNode(nodeView);
                }
            }

        }, DropdownMenuAction.AlwaysEnabled);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(BehaviourTreeWindow window)
    {
        this.window = window;

        //节点创建时的搜索窗口
        var searchWindowProvider = ScriptableObject.CreateInstance<NodeSearchWindowProvider>();
        searchWindowProvider.Init(window);

        nodeCreationRequest += context =>
        {
            //打开搜索窗口
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindowProvider);
        };

        //黑板
        BlackboardView = BlackBoardView.Create(window);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public void Refresh(BehaviourTreeSO btSO)
    {
        BTSO = btSO;

        //还原位置与大小
        if (btSO.ViewportRect != default)
        {
            viewTransform.position = btSO.ViewportRect.position;
            viewTransform.scale = new Vector3(btSO.ViewportRect.size.x, btSO.ViewportRect.size.y, 1);
        }

        BlackboardView.Refresh();
        BuildGraphView();
    }

    /// <summary>
    /// 构建行为树节点图
    /// </summary>
    private void BuildGraphView()
    {
        Dictionary<BaseNode, NodeView> nodeDict = new Dictionary<BaseNode, NodeView>();

        //先删掉旧的节点 线 和注释块
        foreach (Node node in nodes)
        {
            RemoveElement(node);
        }

        foreach (Edge edge in edges)
        {
            RemoveElement(edge);
        }

        foreach (var commentBlockView in CommentBlockViews)
        {
            RemoveElement(commentBlockView);
        }

        CommentBlockViews.Clear();

        //创建节点
        CreateGraphNode(nodeDict, BTSO.AllNodes);

        //根据父子关系连线
        BuildConnect(nodeDict, BTSO.AllNodes);

        //创建注释块
        CreateCommentBlock(nodeDict, BTSO.CommentBlocks);
    }

    /// <summary>
    /// 创建节点图节点
    /// </summary >
    private void CreateGraphNode(Dictionary<BaseNode, NodeView> nodeDict, List<BaseNode> allNodes)
    {
        foreach (BaseNode node in allNodes)
        {
            NodeView nodeView = new NodeView();
            nodeView.Init(window, node);
            AddElement(nodeView);
            nodeDict.Add(node, nodeView);
        }
    }

    /// <summary>
    /// 构建节点连接
    /// </summary>
    private void BuildConnect(Dictionary<BaseNode, NodeView> nodeDict, List<BaseNode> allNodes)
    {
        foreach (BaseNode node in allNodes)
        {
            NodeView nodeView = nodeDict[node];
            if (nodeView.outputContainer.childCount == 0)
            {
                //不需要子节点 跳过
                continue;
            }

            Port selfOutput = (Port)nodeView.outputContainer[0];

            //与当前节点的子节点进行连线
            node.ForeachChild((child =>
            {
                if (child == null)
                {
                    return;
                }

                NodeView childNodeView = nodeDict[child];

                Port childInput = (Port)childNodeView.inputContainer[0];
                Edge edge = selfOutput.ConnectTo(childInput);
                AddElement(edge);
            }));
        }
    }

    /// <summary>
    /// 创建注释块
    /// </summary>
    private void CreateCommentBlock(Dictionary<BaseNode, NodeView> nodeDict, List<CommentBlock> commentBlocks)
    {
        foreach (CommentBlock commentBlock in commentBlocks)
        {
            CommentBlockView.Create(commentBlock, window, nodeDict);
        }
    }

    /// <summary>
    /// 获取可连线的节点列表
    /// </summary>
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        foreach (var port in ports.ToList())
        {
            if (startPort.node == port.node || startPort.direction == port.direction)
            {
                //不能自己连自己
                //只能input和output连接
                continue;
            }

            compatiblePorts.Add(port);
        }

        return compatiblePorts;

    }

    /// <summary>
    /// 添加黑板参数
    /// </summary>
    public void AddBlackBoardParam(string key, Type type, object value = null)
    {
        window.RecordObject($"AddBlackBoard {key}");
        
        BBParam param = (BBParam)Activator.CreateInstance(type);
        param.ValueObj = value;
        BTSO.SetParam(key, param);
        OnBlackBoardChanged?.Invoke();
    }

    /// <summary>
    /// 移除黑板参数
    /// </summary>
    public void RemoveBlackBoardParam(string key)
    {
        window.RecordObject($"RemoveBlackBoard {key}");
        
        BTSO.RemoveParam(key);
        OnBlackBoardChanged?.Invoke();
    }

    /// <summary>
    /// 重命名黑板参数
    /// </summary>
    public bool RenameBlackBoardParam(string oldKey, string newKey, BBParam param)
    {
        if (BTSO.ContainsParamKey(newKey))
        {
            return false;
        }
        
        window.RecordObject($"RenameBlackBoard {oldKey} -> {newKey}");

        BTSO.RemoveParam(oldKey);
        BTSO.SetParam(newKey, param);

        return true;
    }

    /// <summary>
    /// 点击复制时，序列化节点的回调
    /// </summary>
    private string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
    {
        CopyPasteData data = new CopyPasteData();
        List<BaseNode> allNodes = new List<BaseNode>();
        string result = null;

        List<NodeView> graphNodes = new List<NodeView>();
        HashSet<NodeView> graphNodeSet = new HashSet<NodeView>();
        foreach (GraphElement element in elements)
        {
            if (element is NodeView graphNode)
            {
                if (graphNode.RuntimeNode is RootNode)
                {
                    //跳过根节点
                    continue;
                }
                graphNodes.Add(graphNode);
                graphNodeSet.Add(graphNode);
            }
        }

        foreach (var node in BTSO.AllNodes)
        {
            //先置空所有节点的id，防止把未选中但具有父子关系的节点也当成了要复制的节点
            node.ClearId();
        }

        foreach (var graphNode in graphNodes)
        {
            //记录位置
            graphNode.RuntimeNode.Position = graphNode.GetPosition().position;

            //添加到allNodes里 建立ID
            allNodes.Add(graphNode.RuntimeNode);
            graphNode.RuntimeNode.Id = allNodes.Count;
        }

        //记录子节点ID
        foreach (BaseNode node in allNodes)
        {
            node.RebuildId();
            data.CopiedNodes.Add(JsonSerializer.Serialize(node));
        }
            
        ClearSelection();
            
        result = JsonUtility.ToJson(data, true);
        return result;
    }
    
    /// <summary>
    /// 检测是否可点击粘贴的回调
    /// </summary>
    private bool CanPasteSerializedDataCallback(string data)
    {
        try
        {
            return JsonUtility.FromJson(data, typeof(CopyPasteData)) != null;
        } catch {
            return false;
        }
    }
    
    /// <summary>
    /// 点击粘贴时，反序列化节点的回调
    /// </summary>
    private void UnserializeAndPasteCallback(string operationName, string data)
    {
        var copyPasteData = JsonUtility.FromJson<CopyPasteData>(data);

        List<BaseNode> allNodes = new List<BaseNode>();
        foreach (JsonElement jsonElement in copyPasteData.CopiedNodes)
        {
            BaseNode node = JsonSerializer.DeserializeNode(jsonElement);
            node.Position += new Vector2(100, 100);  //被复制的节点 位置要做一点偏移量
                
            BTSO.AllNodes.Add(node);
            allNodes.Add(node);
        }
        //恢复父子节点的引用
        foreach (BaseNode node in allNodes)
        {
            node.RebuildNodeReference(allNodes);
        }
            
        Dictionary<BaseNode, NodeView> nodeDict = new Dictionary<BaseNode, NodeView>();
            
        //创建节点
        CreateGraphNode(nodeDict,allNodes);
            
        //根据父子关系连线
        BuildConnect(nodeDict,allNodes);

        //选中粘贴后的节点
        foreach (var value in nodeDict.Values)
        {
            AddToSelection(value);
        }
    }
}