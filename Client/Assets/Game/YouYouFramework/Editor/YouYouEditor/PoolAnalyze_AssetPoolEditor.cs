using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YouYou;

[CustomEditor(typeof(PoolAnalyze_AssetPool))]
public class PoolAnalyze_AssetPoolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        #region 分类资源池
        GUILayout.Space(10);

        GUIStyle titleStyle = new GUIStyle();
        titleStyle.normal.textColor = new Color(102, 232, 255);

        if (GameEntry.Pool != null)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("下次释放剩余时间: " + Mathf.Abs(Time.time - (GameEntry.Pool.ReleaseAssetNextRunTime + GameEntry.Pool.ReleaseAssetInterval)), titleStyle);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("");
        GUILayout.Label("计数", GUILayout.Width(50));
        GUILayout.Label("剩余时间", GUILayout.Width(50));
        GUILayout.EndHorizontal();

        if (GameEntry.Pool != null)
        {
            foreach (var item in GameEntry.Pool.AssetPool.InspectorDic)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(item.Key);

                titleStyle.fixedWidth = 50;
                GUILayout.Label(item.Value.ReferenceCount.ToString(), titleStyle);

                float remain = Mathf.Max(0, GameEntry.Pool.ReleaseAssetInterval - (Time.time - item.Value.LastUseTime));

                GUILayout.Label(remain.ToString(), titleStyle);
                GUILayout.EndHorizontal();
            }
        }
        GUILayout.EndVertical();
        GUILayout.Space(30);
        #endregion

        serializedObject.ApplyModifiedProperties();
        //重绘
        Repaint();
    }
}
