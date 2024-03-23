using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class PinnedElementView : GraphElement
{
    private const string pinnedElementTree = "UXML/PinnedElementView";
    private const string pinnedElementStyle = "USS/PinnedElementView";

    protected BehaviourTreeGraphView graphView;
    protected BehaviourTreeWindow window;

    private ScrollView scrollView;
    private bool scrollable;

    private VisualElement main;
    protected VisualElement root;
    protected VisualElement header;
    private Label titleLabel;
    protected VisualElement content;

    protected event Action onResized;
    
    public override string title
    {
        get => titleLabel.text;
        set => titleLabel.text = value;
    }

    /// <summary>
    /// 是否可滚动
    /// </summary>
    protected bool Scrollable
    {
        get { return scrollable; }
        set
        {
            if (scrollable == value)
            {
                return;
            }

            scrollable = value;

            //将位置修改为绝对位置
            style.position = Position.Absolute;

            if (scrollable)
            {
                //将content移动到ScrollView下
                content.RemoveFromHierarchy();
                root.Add(scrollView);
                scrollView.Add(content);
                AddToClassList("scrollable");
            }
            else
            {
                //将content从ScrollView下拿出来
                scrollView.RemoveFromHierarchy();
                content.RemoveFromHierarchy();
                root.Add(content);
                RemoveFromClassList("scrollable");
            }
        }
    }

    public PinnedElementView()
    {
        scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);

        var tpl = Resources.Load<VisualTreeAsset>(pinnedElementTree);
        styleSheets.Add(Resources.Load<StyleSheet>(pinnedElementStyle));

        main = tpl.CloneTree();
        main.AddToClassList("mainContainer");
        root = main.Q("content");
        header = main.Q("header");
        titleLabel = main.Q<Label>(name: "titleLabel");
        content = main.Q<VisualElement>(name: "contentContainer");
        hierarchy.Add(main);


        capabilities |= Capabilities.Movable | Capabilities.Resizable; //可移动 可修改大小
        style.overflow = Overflow.Hidden; //隐藏溢出的部分

        ClearClassList();
        AddToClassList("pinnedElement");

        //可拖动
        this.AddManipulator(new Dragger { clampToParentEdges = true });
        Scrollable = false;

        //监听Resizer
        hierarchy.Add(new Resizer(() => onResized?.Invoke()));
    }



    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(BehaviourTreeWindow window)
    {
        this.window = window;
        this.graphView = window.GraphView;
    }

    /// <summary>
    /// 刷新
    /// </summary>
    public virtual void Refresh()
    {

    }

}