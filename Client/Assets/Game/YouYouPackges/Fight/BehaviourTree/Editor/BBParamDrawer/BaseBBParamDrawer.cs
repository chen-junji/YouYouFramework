using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 黑板参数Drawer基类
/// </summary>
public abstract class BaseBBParamDrawer
{
    /// <summary>
    /// 黑板参数类型 -> 黑板参数Drawer
    /// </summary>
    public static Dictionary<Type, BaseBBParamDrawer>
        BBParamDrawerDict = new Dictionary<Type, BaseBBParamDrawer>();

    /// <summary>
    /// 黑板参数对象 -> 可重排序列表
    /// </summary>
    protected static Dictionary<BBParam, ReorderableList> reorderableListDict =
        new Dictionary<BBParam, ReorderableList>();

    /// <summary>
    /// 要绘制的目标黑板参数
    /// </summary>
    public BBParam Target;

    private int selectedKeyIndex = 0;

    static BaseBBParamDrawer()
    {
        //收集黑板参数Drawer对象
        BBParamDrawerDict.Clear();
        var types = TypeCache.GetTypesWithAttribute<BBParamDrawerAttribute>();
        foreach (Type type in types)
        {
            var attr = type.GetCustomAttribute<BBParamDrawerAttribute>();
            var bbParamDrawer = (BaseBBParamDrawer)Activator.CreateInstance(type);
            BBParamDrawerDict.Add(attr.BBParamType, bbParamDrawer);
        }
    }

    /// <summary>
    /// 基于IMGUI绘制黑板值
    /// </summary>
    public virtual void OnGUI(BehaviourTreeSO btSO, bool isInspector)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (isInspector)
            {
                //在节点属性面板下 绘制选择黑板key下拉菜单
                var (curIndex, curKey) =
                    BBParamDrawerHelper.DrawBBParamKeyPopup(btSO, Target.GetType(), "黑板Key:",
                        selectedKeyIndex, Target.Key);
                selectedKeyIndex = curIndex;
                Target.Key = curKey;
            }

            using (new EditorGUI.DisabledGroupScope(isInspector && !string.IsNullOrEmpty(Target.Key)))
            {
                //在节点属性面板下 若Key不为空 则Disable掉值输入区域
                OnGUI();
            }

        }
    }

    /// <summary>
    /// 基于IMGUI绘制黑板值
    /// </summary>
    protected virtual void OnGUI()
    {

    }
    
    protected void RecordBBParam()
    {
        BehaviourTreeWindow window = (BehaviourTreeWindow)EditorWindow.focusedWindow;
        window.RecordObject($"Change BBParam {Target.Key}");
    }
}

public abstract class BaseBBParamDrawer<T> : BaseBBParamDrawer
{

    protected override void OnGUI()
    {
        BBParam<T> param = (BBParam<T>) Target;
        var newValue = DrawValue(param.Value);
        if (IsDirty(param.Value,newValue))
        {
            RecordBBParam();
            param.Value = newValue;
        }
    }

    protected abstract T DrawValue(T value);
    protected abstract bool IsDirty(T value, T newValue);
}

[BBParamDrawer(BBParamType = typeof(BBParamBool))]
    public class BBParamBoolDrawer : BaseBBParamDrawer<bool>
    {
        protected override bool DrawValue(bool value)
        {
            var newValue = EditorGUILayout.Toggle("Value",value);
            return newValue;
        }

        protected override bool IsDirty(bool value, bool newValue)
        {
            return value != newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamInt))]
    public class BBParamIntDrawer : BaseBBParamDrawer<int>
    {
        protected override int DrawValue(int value)
        {
            var newValue = EditorGUILayout.IntField("Value",value);
            return newValue;
        }

        protected override bool IsDirty(int value, int newValue)
        {
            return value != newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamFloat))]
    public class BBParamFloatDrawer : BaseBBParamDrawer <float>
    {
        protected override float DrawValue(float value)
        {
            var newValue = EditorGUILayout.FloatField("Value",value);
            return newValue;
        }

        protected override bool IsDirty(float value, float newValue)
        {
            return Math.Abs(value - newValue) > float.Epsilon;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamString))]
    public class BBParamStringDrawer : BaseBBParamDrawer<string>
    {
        protected override string DrawValue(string value)
        {
            var newValue = EditorGUILayout.TextField("Value",value);
            return newValue;
        }

        protected override bool IsDirty(string value, string newValue)
        {
            return value != newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamVector2))]
    public class BBParamVector2Drawer : BaseBBParamDrawer<Vector2>
    {
        protected override Vector2 DrawValue(Vector2 value)
        {
            var newValue = EditorGUILayout.Vector2Field("Value",value);
            return newValue;
        }

        protected override bool IsDirty(Vector2 value, Vector2 newValue)
        {
            return value != newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamVector3))]
    public class BBParamVector3Drawer : BaseBBParamDrawer<Vector3>
    {

        protected override Vector3 DrawValue(Vector3 value)
        {
            var newValue = EditorGUILayout.Vector3Field("Value",value);
            return newValue;
        }

        protected override bool IsDirty(Vector3 value, Vector3 newValue)
        {
            return value != newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamVector4))]
    public class BBParamVector4Drawer : BaseBBParamDrawer<Vector4>
    {
        protected override Vector4 DrawValue(Vector4 value)
        {
            var newValue = EditorGUILayout.Vector4Field("Value",value);
            return newValue;
        }

        protected override bool IsDirty(Vector4 value, Vector4 newValue)
        {
            return value != newValue;
        }
    }

    /// <summary>
    /// 黑板List参数Drawer基类
    /// </summary>
    public abstract class BBParamListDrawer<T> : BaseBBParamDrawer<List<T>>
    {
        protected override List<T> DrawValue(List<T> value)
        {
            if (value == null)
            {
                value = new List<T>();
                Target.ValueObj = value;
            }
            
            if (!reorderableListDict.TryGetValue(Target,out var list))
            {
                list = new ReorderableList(value, typeof(T), true, false, true, true);
                list.drawElementCallback += (rect, index, active, focused) =>
                {
                    value[index] = DrawElement(rect, value[index]);
                };
                reorderableListDict.Add(Target,list);
            }
            
            list.DoLayoutList();
            return value;
        }

        protected override bool IsDirty(List<T> value, List<T> newValue)
        {
            //列表判断不了是否Dirty 因为value和newValue都是同一个列表的引用
            return false;
        }

        protected abstract T DrawElement(Rect rect, T element);
    }

    [BBParamDrawer(BBParamType = typeof(BBParamBoolList))]
    public class BBParamBoolListDrawer : BBParamListDrawer<bool>
    {
        protected override bool DrawElement(Rect rect, bool element)
        {
            var newValue = EditorGUI.Toggle(rect,element);
            return newValue;
        }
    }

    [BBParamDrawer(BBParamType = typeof(BBParamIntList))]
    public class BBParamIntListDrawer : BBParamListDrawer<int>
    {
        protected override int DrawElement(Rect rect, int element)
        {
            var newValue = EditorGUI.IntField(rect,element);
            return newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamFloatList))]
    public class BBParamFloatListDrawer : BBParamListDrawer<float>
    {
        protected override float DrawElement(Rect rect, float element)
        {
            var newValue = EditorGUI.FloatField(rect,element);
            return newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamStringList))]
    public class BBParamStringListDrawer : BBParamListDrawer<string>
    {
        protected override string DrawElement(Rect rect, string element)
        {
            var newValue = EditorGUI.TextField(rect,element);
            return newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamVector2List))]
    public class BBParamVector2ListDrawer : BBParamListDrawer<Vector2>
    {
        protected override Vector2 DrawElement(Rect rect, Vector2 element)
        {
            var newValue = EditorGUI.Vector2Field(rect, string.Empty, element);
            return newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamVector3List))]
    public class BBParamVector3ListDrawer : BBParamListDrawer<Vector3>
    {
        protected override Vector3 DrawElement(Rect rect, Vector3 element)
        {
            var newValue = EditorGUI.Vector3Field(rect, string.Empty, element);
            return newValue;
        }
    }
    
    [BBParamDrawer(BBParamType = typeof(BBParamVector4List))]
    public class BBParamVector4ListDrawer : BBParamListDrawer<Vector4>
    {
        protected override Vector4 DrawElement(Rect rect, Vector4 element)
        {
            var newValue = EditorGUI.Vector4Field(rect, string.Empty, element);
            return newValue;
        }
    }