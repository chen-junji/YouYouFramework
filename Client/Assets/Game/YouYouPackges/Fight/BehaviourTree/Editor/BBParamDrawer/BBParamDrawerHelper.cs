using System;
using UnityEditor;

public static class BBParamDrawerHelper
{
    /// <summary>
    /// 绘制指定Value类型的黑板Key的下拉选择菜单
    /// </summary>
    public static (int, string) DrawBBParamKeyPopup(BehaviourTreeSO btSO, Type paramType, string label,
        int curIndex, string curKey)

    {
        var keys = btSO.GetParamKeys(paramType);
        return DrawBBParamKeyPopup(keys, label, curIndex, curKey);
    }

    /// <summary>
    /// 绘制给定黑板Key的下拉选择菜单
    /// </summary>
    public static (int, string) DrawBBParamKeyPopup(string[] keys, string label,
        int curIndex, string curKey)
    {
        //修正索引 防止因为删除了前面的key导致顺序变化 最终索引到了错误的key
        for (int i = 0; i < keys.Length; i++)
        {
            if (curKey == keys[i] && curIndex != i)
            {
                curIndex = i;
                break;
            }
        }

        int newIndex = EditorGUILayout.Popup(label, curIndex, keys);
        if (newIndex == 0 || newIndex >= keys.Length)
        {
            //选择了Null 或者无效索引 就将key置空
            curIndex = 0;
            curKey = null;
        }
        else
        {
            if (curIndex > 0 && curKey != keys[curIndex])
            {
                //当前索引到的key和保存的key不一致 重置为null
                //这时可能是之前的key被删了，或者被重命名了
                curIndex = 0;
                curKey = null;
            }
            else
            {
                var newKey = keys[newIndex];
                curIndex = newIndex;
                curKey = newKey;
            }
        }

        return (curIndex, curKey);
    }
}