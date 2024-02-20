using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 搜索窗口
/// </summary>
public class NodeSearchWindowProvider : ScriptableObject, ISearchWindowProvider
{
    private BehaviourTreeWindow window;
    private BehaviourTreeGraphView graphView;

    private NodeView sourceNode;
    private bool isParentWithSourceNode;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(BehaviourTreeWindow window, Edge edge = null)
    {
        this.window = window;
        graphView = window.GraphView;

        if (edge != null)
        {
            if (edge.output != null)
            {
                sourceNode = (NodeView)edge.output.node;
                isParentWithSourceNode = true;
            }
            else
            {
                sourceNode = (NodeView)edge.input.node;
                isParentWithSourceNode = false;
            }
        }
    }

    /// <summary>
    /// 创建搜索树
    /// </summary>
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var entries = new List<SearchTreeEntry>();

        entries.Add(new SearchTreeGroupEntry(new GUIContent("行为树节点"), 0));

        entries.Add(new SearchTreeGroupEntry(new GUIContent("复合节点"), 1));
        AddNodeOptions<BaseCompositeNode>(entries);

        entries.Add(new SearchTreeGroupEntry(new GUIContent("装饰节点"), 1));
        AddNodeOptions<BaseDecoratorNode>(entries);

        entries.Add(new SearchTreeGroupEntry(new GUIContent("动作节点"), 1));
        AddNodeOptions<BaseActionNode>(entries);

        return entries;
    }

    /// <summary>
    /// 添加节点选项
    /// </summary>
    private void AddNodeOptions<T>(List<SearchTreeEntry> entries)
    {
        var types = TypeCache.GetTypesDerivedFrom<T>().ToList();
        for (int i = types.Count - 1; i >= 0; i--)
        {
            var type = types[i];
            if (type.IsAbstract)
            {
                //跳过抽象类节点
                types.RemoveAt(i);
            }
        }

        var titlePaths = new HashSet<string>();
        types.OrderBy((type =>
        {
            var attr = type.GetCustomAttribute<NodeInfoAttribute>();
            return attr.Name;
        }));



        foreach (Type type in types)
        {
            if (type == typeof(RootNode))
            {
                //跳过根节点
                continue;
            }
            
            var nodeAttr = type.GetCustomAttribute<NodeInfoAttribute>();
            if (nodeAttr == null)
            {
                Debug.LogError($"节点{type.Name}未标记NodeInfo特性");
                continue;
            }


            //根据节点路径计算level
            string nodeName = nodeAttr.Name;
            string[] parts = nodeAttr.Name.Split('/');
            int level = 1;
            if (parts.Length > 1)
            {
                //存在路径
                level += 2;
                nodeName = parts[parts.Length - 1];
                string fullTitleAsPath = "";

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    //各级路径
                    string title = parts[i];
                    fullTitleAsPath += title;
                    level = i + 2;

                    if (!titlePaths.Contains(fullTitleAsPath))
                    {
                        //添加父路径
                        entries.Add(new SearchTreeGroupEntry(new GUIContent(title))
                        {
                            level = level,
                        });
                        titlePaths.Add(fullTitleAsPath);
                    }
                }
            }

            var guiContent = new GUIContent(nodeName);
            entries.Add(new SearchTreeEntry(guiContent) { level = level + 1, userData = type });
        }
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        //取出类型信息
        Type type = (Type)searchTreeEntry.userData;

        //创建节点 
        var point = context.screenMousePosition - window.position.position; //鼠标相对于窗口的位置
        Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(point); //鼠标在节点图下的位置
        NodeView nodeView = NodeView.Create(type, window, graphMousePosition);

        //如果是通过拖动线创建的节点 就连接起来
        if (sourceNode != null)
        {
            if (isParentWithSourceNode)
            {
                sourceNode.AddChild(nodeView);
            }
            else
            {
                nodeView.AddChild(sourceNode);
            }
        }

        return true;
    }
}