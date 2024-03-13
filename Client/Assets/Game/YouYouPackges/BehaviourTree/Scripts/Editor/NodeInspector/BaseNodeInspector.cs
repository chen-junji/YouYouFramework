using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;


/// <summary>
/// 节点属性面板Inspector基类
/// </summary>
public class BaseNodeInspector
{
    public BehaviourTreeSO BTSO;

    /// <summary>
    /// 要绘制的目标节点
    /// </summary>
    public BaseNode Target;

    /// <summary>
    /// 基于IMGUI绘制节点属性面板
    /// </summary>
    public virtual void OnGUI()
    {
        var infoAttr = Target.GetType().GetCustomAttribute<NodeInfoAttribute>();
        if (!string.IsNullOrEmpty(infoAttr.Desc))
        {
            //节点描述信息
            EditorGUILayout.LabelField($"【{infoAttr.Desc}】");
        }

        EditorGUILayout.Space();

        Type type = typeof(BBParam);
        foreach (FieldInfo fieldInfo in Target.FieldInfos)
        {
            if (!type.IsAssignableFrom(fieldInfo.FieldType))
            {
                continue;
            }

            //参数名
            var nameAttr = fieldInfo.GetCustomAttribute<BBParamInfoAttribute>();
            string name = null;
            if (nameAttr != null)
            {
                name = nameAttr.Name;
            }

            if (!string.IsNullOrEmpty(name))
            {
                EditorGUILayout.LabelField($"{name}({BBParam.GetBBParamTypeName(fieldInfo.FieldType)})");
            }

            //获取参数对象
            var bbParam = (BBParam)fieldInfo.GetValue(Target);
            if (bbParam == null)
            {
                bbParam = (BBParam)Activator.CreateInstance(fieldInfo.FieldType);
                fieldInfo.SetValue(Target, bbParam);
            }

            //获取对应的drawer
            if (BaseBBParamDrawer.BBParamDrawerDict.TryGetValue(fieldInfo.FieldType, out var drawer))
            {
                drawer.Target = bbParam;
                drawer.OnGUI(BTSO, true);
            }

            EditorGUILayout.Space();
        }
    }
}