using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

/// <summary>
/// 行为树窗口
/// </summary>
public class BehaviourTreeWindow : EditorWindow
{
    private DropdownField dropdownField;
    private ObjectField objField;
    private Button btnSave;
    private Button btnSaveAs;
    private Button btnNew;

    private SplitView splitView;
    public InspectorView NodeInspector;
    public BehaviourTreeGraphView GraphView;

    private string assetPath;

    private BehaviourTreeSO originBTSO;
    private BehaviourTreeSO curBTSO;
    
    private delegate void GetUndoListDelegate(List<string> undoList, out int index);
    private static MethodInfo getUndoListMethod = typeof(Undo).GetMethod("GetUndoList", BindingFlags.Static | BindingFlags.NonPublic);
    private static GetUndoListDelegate getUndoList = (GetUndoListDelegate)Delegate.CreateDelegate(typeof(GetUndoListDelegate), getUndoListMethod);
    private static List<string> undoList = new List<string>();
    
    public bool IsRefreshing;
    
    /// <summary>
    /// 是否为调试模式
    /// </summary>
    public bool IsDebugMode { get; private set; }
    
    /// <summary>
    /// 是否被修改过
    /// </summary>
    public bool IsModify
    {
        get
        {
            return titleContent.text[titleContent.text.Length - 1] == '*';
        }
        set
        {
            if (IsModify == value)
            {
                return;
            }
                
            if (value)
            {
                titleContent.text += '*';
            }
            else
            {
                titleContent.text = titleContent.text.Remove(titleContent.text.Length - 1);
            }
        }
    }
    
    [MenuItem("BehaviourTree/打开行为树窗口")]
    public static void Open()
    {
        OpenFromAssetPath(null);
    }

    /// <summary>
    /// 双击行为树SO打开窗口
    /// </summary>
    [OnOpenAsset(1)]
    public static bool OnOpenAssets(int id, int line)
    {
        if (EditorUtility.InstanceIDToObject(id) is BehaviourTreeSO btSO)
        {
            string assetPath = AssetDatabase.GetAssetPath(btSO);
            OpenFromAssetPath(assetPath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 通过行为树SO资源路径打开窗口
    /// </summary>
    public static void OpenFromAssetPath(string assetPath)
    {
        BehaviourTreeWindow window = CreateWindow<BehaviourTreeWindow>("行为树窗口");
        window.Init(false);
        window.RefreshFromAssetPath(assetPath);
    }
    
    /// <summary>
    /// 通过调试器打开窗口
    /// </summary>
    public static void OpenFromDebugger(BehaviourTree debugBT)
    {
        BehaviourTreeWindow window = CreateWindow<BehaviourTreeWindow>("行为树窗口");
        window.Init(true);
        window.RefreshFromDebugger(debugBT);
    }

    private void OnEnable()
    {
        VisualElement root = rootVisualElement;

        var visualTree = Resources.Load<VisualTreeAsset>("UXML/BehaviourTreeWindow");
        visualTree.CloneTree(root);

        var styleSheet = Resources.Load<StyleSheet>("USS/BehaviourTreeWindow");
        root.styleSheets.Add(styleSheet);

        //下拉列表
        dropdownField = rootVisualElement.Q<DropdownField>("dropdownBTSO");
        dropdownField.RegisterValueChangedCallback((evt =>
        {
            if (string.IsNullOrEmpty(evt.newValue))
            {
                return;
            }
            ClearAllRecord();

            if (!IsDebugMode)
            {
                string path = evt.newValue.Replace('\\', '/');
                RefreshFromAssetPath(path);
            }
            else
            {
                var debugBT = BTDebugger.Get(evt.newValue);
                RefreshFromDebugger(debugBT);
            }
        }));

        objField = rootVisualElement.Q<ObjectField>("ObjBTSO");

        btnSave = root.Q<Button>("btnSave");
        btnSave.clicked += () => { Save(false); };

        btnSaveAs = root.Q<Button>("btnSaveAs");
        btnSaveAs.clicked += () => { Save(true); };

        btnNew = root.Q<Button>("btnNew");
        btnNew.clicked += New;

        splitView = root.Q<SplitView>();
        NodeInspector = root.Q<InspectorView>("inspector");
        GraphView = rootVisualElement.Q<BehaviourTreeGraphView>("graphView");
        GraphView.Init(this);

        Undo.undoRedoPerformed += OnUndoRedoPerformed;
        ClearAllRecord();
        
        //还原数据 防止编译后窗口数据丢失
        if (!string.IsNullOrEmpty(assetPath))
        {
            dropdownField.SetValueWithoutNotify(assetPath.Replace('/', '\\'));
            objField.value = AssetDatabase.LoadAssetAtPath<BehaviourTreeSO>(assetPath);
        }

        if (curBTSO != null)
        {
            Refresh(curBTSO);
        }
    }

    private void OnDisable()
    {
        if (curBTSO == null)
        {
            return;
        }
        
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        ClearAllRecord();
        
        //记录一些信息 用于编译后的窗口还原
        curBTSO.InspectorWidth = splitView.Q("left").layout.width;
        curBTSO.ViewportRect = new Rect(GraphView.viewTransform.position, GraphView.viewTransform.scale);
        curBTSO.BlackBoardRect = GraphView.BlackboardView.GetPosition();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init(bool isDebugMode)
    {
        IsDebugMode = isDebugMode;
        RefreshDropDown(isDebugMode);

        if (isDebugMode)
        {
            //调试状态下 删掉SO引用 保存 另存为和新建按钮
            objField.RemoveFromHierarchy();
        }
    }
    
    /// <summary>
    /// 刷新下拉菜单
    /// </summary>
    private void RefreshDropDown(bool isDebugMode)
    {
        List<string> paths;
        if (!isDebugMode)
        {
            paths = AssetDatabase.FindAssets($"t:{nameof(BehaviourTreeSO)}")
                .Select(((s, i) => AssetDatabase.GUIDToAssetPath(s).Replace('/','\\'))).ToList();
        }
        else
        {
            paths = BTDebugger.BTInstanceDict.Keys.ToList();
        }
        dropdownField.choices = paths.ToList();
    }

    /// <summary>
    /// 使用行为树SO路径刷新
    /// </summary>
    private void RefreshFromAssetPath(string assetPath)
    {
        this.assetPath = assetPath;

        //加载行为树SO
        if (!string.IsNullOrEmpty(assetPath))
        {
            originBTSO = AssetDatabase.LoadAssetAtPath<BehaviourTreeSO>(assetPath);
            if (originBTSO != null)
            {
                //深拷贝一份用于编辑
                curBTSO = Instantiate(originBTSO);
            }

            dropdownField.SetValueWithoutNotify(assetPath.Replace('/', '\\'));
            objField.value = originBTSO;
        }


        Refresh(curBTSO);
    }
    
    /// <summary>
    /// 使用调试用行为树对象刷新
    /// </summary>
    private void RefreshFromDebugger(BehaviourTree debugBT)
    {
        int index = dropdownField.choices.IndexOf(debugBT.DebugName.Replace('/','\\'));
        dropdownField.index = index;

        //将debugBT转换为btSO
        var btSO = CreateInstance<BehaviourTreeSO>();
        btSO.Serialize(debugBT);
                
        Refresh(btSO);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    private void Refresh(BehaviourTreeSO btSO)
    {
        IsRefreshing = true;
        
        //若传入的行为树SO为空则创建个默认的
        if (btSO == null)
        {
            var bt = new BehaviourTree();
            bt.RootNode = new RootNode
            {
                Position = new Vector2(GraphView.contentContainer.layout.position.x + 500, GraphView.contentContainer.layout.position.y + 100),
                Owner = bt
            };
            bt.AllNodes.Add(bt.RootNode);
                
            btSO = CreateInstance<BehaviourTreeSO>();
            btSO.Serialize(bt);
        }
        else
        {
            if (!IsDebugMode)
            {
                //重建节点引用
                btSO.Deserialize();
            }
            
        }

        curBTSO = btSO;

        //设置节点属性面板的宽度
        if (curBTSO.InspectorWidth != 0)
        {
            splitView.fixedPaneInitialDimension = curBTSO.InspectorWidth;
        }
        else
        {
            splitView.fixedPaneInitialDimension = 420;
        }

        GraphView.Refresh(curBTSO);
        
        IsRefreshing = false;
    }

    /// <summary>
    /// 节点被点击时的回调
    /// </summary>
    public void OnNodeClick(NodeView nodeView)
    {
        NodeInspector.DrawInspector(curBTSO, nodeView);
    }
    
    /// <summary>
    /// 保存行为树
    /// </summary>
    private void Save(bool isSaveAs)
    {
        if (originBTSO == null || isSaveAs)
        {
            //是新建的 或者另存为
            string path = EditorUtility.SaveFilePanelInProject("选择保存位置", "BehaviourTree", "asset", "");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("取消保存");
                return;
            }

            AssetDatabase.DeleteAsset(path);
                
            assetPath = path;
            AssetDatabase.CreateAsset(curBTSO,path);
            originBTSO = curBTSO;
        }
        else
        {
            if (!EditorUtility.DisplayDialog("提示","确认保存？","是","否"))
            {
                return;
            }
                
            originBTSO.AllNodes = curBTSO.AllNodes;
            originBTSO.BBParams = curBTSO.BBParams;
            originBTSO.CommentBlocks = curBTSO.CommentBlocks;
        }
            
        //记录属性面板宽度
        originBTSO.InspectorWidth = splitView.Q("left").layout.width;
            
        //记录节点图位置
        originBTSO.ViewportRect = new Rect(GraphView.viewTransform.position, GraphView.viewTransform.scale);

        //记录黑板位置
        originBTSO.BlackBoardRect = GraphView.BlackboardView.GetPosition();
        
        originBTSO.BuildNodeId();
        EditorUtility.SetDirty(originBTSO);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RefreshDropDown(false);
        RefreshFromAssetPath(assetPath);
            
            
        Debug.Log($"保存行为树成功，路径:{assetPath}");
    }

    /// <summary>
    /// 新建行为树
    /// </summary>
    private void New()
    {
        assetPath = null;
        curBTSO = null;
        originBTSO = null;
        dropdownField.index = -1;
        objField.value = null;
        RefreshFromAssetPath(null);
    }
    
    /// <summary>
    /// 发生Undo Redo时的监听
    /// </summary>
    private void OnUndoRedoPerformed()
    {
        if (curBTSO == null)
        {
            return;
        }

        //若当前已撤销到最初的状态，则视为未修改过
        undoList.Clear();
        getUndoList(undoList, out int index);
        IsModify = index != -1;
            
        Refresh(curBTSO);
    }
        
    /// <summary>
    /// 记录修改
    /// </summary>
    public void RecordObject(string undoName)
    {
        if (IsRefreshing)
        {
            return;
        }
            
        Undo.RecordObject(curBTSO, undoName);

        IsModify = true;
    }
    
    public void BuildNodeId()
    {
        curBTSO.BuildNodeId();
    }

    /// <summary>
    /// 清理所有修改记录
    /// </summary>
    public void ClearAllRecord()
    {
        Undo.ClearAll();
        IsModify = false;
    }
}