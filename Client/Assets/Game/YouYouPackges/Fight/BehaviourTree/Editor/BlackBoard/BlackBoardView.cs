    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 黑板界面
/// </summary>
public class BlackBoardView : PinnedElementView
{
    public BlackBoardView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("USS/BlackBoardView"));
    }

    public static BlackBoardView Create(BehaviourTreeWindow window)
    {
        BlackBoardView view = new BlackBoardView();
        window.GraphView.Add(view);
        view.Init(window);
        return view;
    }

    public override void Init(BehaviourTreeWindow window)
    {
        base.Init(window);

        base.title = "黑板";
        Scrollable = true; //可滚动
        SetPosition(new Rect(10, 30, 250, 250));
        header.Add(new Button(OnAddClicked)
        {
            text = "+"
        });
        graphView.OnBlackBoardChanged += UpdateContent;
    }

    public override void Refresh()
    {
        if (graphView.BTSO.BlackBoardRect != default)
        {
            SetPosition(graphView.BTSO.BlackBoardRect);
        }

        UpdateContent();
    }

    /// <summary>
    /// 点击了黑板上的加号
    /// </summary>
    private void OnAddClicked()
    {
        var paramType = new GenericMenu();

        foreach (Type type in GetBlackBoardParamTypes())
        {
            string typeName = BBParam.GetBBParamTypeName(type);
            paramType.AddItem(new GUIContent(typeName), false, () =>
            {
                string uniqueKey = $"New {typeName}";
                uniqueKey = GetUniqueBlackBoardKey(uniqueKey);
                graphView.AddBlackBoardParam(uniqueKey, type);
            });
        }

        paramType.ShowAsContext();
    }


    /// <summary>
    /// 获取黑板参数Type
    /// </summary>
    private List<Type> GetBlackBoardParamTypes()
    {
        List<Type> types = new List<Type>();

        foreach (Type type in TypeCache.GetTypesDerivedFrom<BBParam>())
        {
            if (type.IsGenericType || type.IsAbstract)
            {
                continue;
            }

            types.Add(type);
        }

        return types;
    }


    /// <summary>
    /// 获取唯一的黑板Key
    /// </summary>
    private string GetUniqueBlackBoardKey(string key)
    {
        string uniqueKey = key;
        int i = 0;
        var keys = graphView.BTSO.GetAllParamKey();
        while (keys.Any(e => e == key))
        {
            key = uniqueKey + " " + i++;
        }

        return key;
    }


    /// <summary>
    /// 更新黑板内容
    /// </summary>
    private void UpdateContent()
    {
        content.Clear();

        foreach (BBParam param in graphView.BTSO.BBParams)
        {
            Type type = param.GetType();
            string typeName = BBParam.GetBBParamTypeName(type);
            var keyView = new BlackboardParamKeyView(graphView, param, param.Key, typeName);
            var valueView = new BlackboardParamValueView();
            valueView.DrawValue(param);
            var row = new BlackboardRow(keyView, valueView);
            row.expanded = true;

            content.Add(row);
        }

    }
}