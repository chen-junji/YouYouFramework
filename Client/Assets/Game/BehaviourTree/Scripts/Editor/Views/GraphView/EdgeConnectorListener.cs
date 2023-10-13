using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 自定义的线连接监听
/// </summary>
public class EdgeConnectorListener : IEdgeConnectorListener
{
    private GraphViewChange m_GraphViewChange;
    private List<Edge> m_EdgesToCreate;
    private List<GraphElement> m_EdgesToDelete;

    public EdgeConnectorListener()
    {
        m_EdgesToCreate = new List<Edge>();
        m_EdgesToDelete = new List<GraphElement>();
        m_GraphViewChange.edgesToCreate = this.m_EdgesToCreate;
    }
        
    public void OnDropOutsidePort(Edge edge, Vector2 position)
    {
        ShowNodeCreationMenuFromEdge(edge,position);
    }
        
        
    public void OnDrop(GraphView graphView, Edge edge)
    {
        var parentNode = (NodeView)edge.output.node;
        var childNode = (NodeView)edge.input.node;
        parentNode.AddChild(childNode);
    }
        
    private void ShowNodeCreationMenuFromEdge(Edge edge, Vector2 position)
    {
        var searchWindowProvider = ScriptableObject.CreateInstance<NodeSearchWindowProvider>();
        var window = (BehaviourTreeWindow)EditorWindow.focusedWindow;
        searchWindowProvider.Init(window,edge);
        var screenMousePosition = position + EditorWindow.focusedWindow.position.position;  //鼠标在屏幕坐标
        SearchWindow.Open(new SearchWindowContext(screenMousePosition), searchWindowProvider);
    }
}