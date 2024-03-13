using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// 注释块View
/// </summary>
public class CommentBlockView : Group
{
    private BehaviourTreeWindow window;
    private CommentBlock commentBlock;

    public List<NodeView> NodeViews = new List<NodeView>();

    private ColorField colorField;

    public CommentBlockView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("USS/CommentBlockView"));
    }


    public static CommentBlockView Create(BehaviourTreeWindow window, Vector2 pos)
    {
        window.RecordObject($"Create CommentBlock");
        
        CommentBlock commentBlock = new CommentBlock("注释块", pos);
        window.GraphView.BTSO.CommentBlocks.Add(commentBlock);

        CommentBlockView view = Create(commentBlock, window, null);

        return view;
    }

    public static CommentBlockView Create(CommentBlock commentBlock, BehaviourTreeWindow window,
        Dictionary<BaseNode, NodeView> nodeDict)
    {
        CommentBlockView view = new CommentBlockView();
        view.Init(window, commentBlock, nodeDict);
        window.GraphView.AddElement(view);
        window.GraphView.CommentBlockViews.Add(view);
        return view;
    }

    private static void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(BehaviourTreeWindow window, CommentBlock commentBlock, Dictionary<BaseNode, NodeView> nodeDict)
    {
        this.window = window;
        this.commentBlock = commentBlock;

        title = commentBlock.Comment;
        SetPosition(new Rect(commentBlock.Position, new Vector2(400, 200)));

        this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

        //注释
        headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>((evt =>
        {
            window.RecordObject($"Update Comment");
            commentBlock.Comment = evt.newValue;
        }));

        //颜色
        colorField = new ColorField { value = commentBlock.Color, name = "headerColorPicker" };
        headerContainer.Add(colorField);
        colorField.RegisterValueChangedCallback(e =>
        {
            window.RecordObject($"Update Color");
            commentBlock.Color = e.newValue;
            style.backgroundColor = e.newValue;
        });
        style.backgroundColor = commentBlock.Color;

        //初始节点
        if (commentBlock.Nodes != null)
        {
            //修复commentBlock.Nodes未知原因被清空的问题
            if (commentBlock.NodeIds.Count > 0 && commentBlock.Nodes.Count == 0)
            {
                commentBlock.RebuildNodeReference(window.GraphView.BTSO.AllNodes);
            }

            foreach (BaseNode node in commentBlock.Nodes)
            {
                NodeView nodeView = nodeDict[node];
                NodeViews.Add(nodeView);
                AddElement(nodeView);

            }
        }

    }

    /// <summary>
    /// 设置位置
    /// </summary>
    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        commentBlock.Position = newPos.position;
    }

    /// <summary>
    /// 添加节点到注释块内
    /// </summary>
    public void AddNode(NodeView nodeView)
    {
        commentBlock.Nodes.Add(nodeView.RuntimeNode);
        NodeViews.Add(nodeView);
        AddElement(nodeView);
    }

    /// <summary>
    /// 删除注释块
    /// </summary>
    public void RemoveSelf()
    {
        window.RecordObject($"Remove CommentBlock");
        window.GraphView.BTSO.CommentBlocks.Remove(commentBlock);
        window.GraphView.CommentBlockViews.Remove(this);
    }

    /// <summary>
    /// 添加元素
    /// </summary>
    protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
    {
        window.RecordObject($"Add Node");
        
        base.OnElementsAdded(elements);
        foreach (GraphElement element in elements)
        {
            if (element is NodeView nodeView)
            {
                if (commentBlock.Nodes.Contains(nodeView.RuntimeNode))
                {
                    continue;
                }

                commentBlock.Nodes.Add(nodeView.RuntimeNode);
                NodeViews.Add(nodeView);
            }
        }
        
        window.BuildNodeId();
    }

    /// <summary>
    /// 删除元素
    /// </summary>
    protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
    {
        window.RecordObject($"Remove Node");
        if (window.IsRefreshing)
        {
            //因为刷新graph view导致的节点删除 不处理后续逻辑
            return;
        }
        base.OnElementsRemoved(elements);
        foreach (GraphElement element in elements)
        {
            if (element is NodeView nodeView)
            {
                commentBlock.Nodes.Remove(nodeView.RuntimeNode);
                NodeViews.Remove(nodeView);
            }
        }
        
        window.BuildNodeId();
    }
}