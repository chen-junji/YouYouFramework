using YouYouMain;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YouYouFramework;

[CustomEditor(typeof(PoolAnalyze_AssetBundlePool))]
public class PoolAnalyze_AssetBundlePoolEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		#region 资源包池
		GUILayout.Space(10);

		GUIStyle titleStyle = new GUIStyle();
		titleStyle.normal.textColor = new Color(102, 232, 255);

		if (GameEntry.Pool != null)
		{
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("下次释放剩余时间: " + Mathf.Abs(Time.time - (GameEntry.Pool.ReleaseAssetBundleNextRunTime + MainEntry.ParamsSettings.PoolReleaseAssetBundleInterval)), titleStyle);
			GUILayout.EndHorizontal();
		}
		//===================资源池变量计数==================
		GUILayout.Space(10);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("资源包");
		GUILayout.Label("计数", GUILayout.Width(50));
		GUILayout.Label("剩余时间", GUILayout.Width(50));
		GUILayout.EndHorizontal();

		if (GameEntry.Pool != null)
		{
			foreach (var item in GameEntry.Pool.AssetBundlePool.InspectorDic)
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Label(item.Key);

				titleStyle.fixedWidth = 50;
				GUILayout.Label(item.Value.ReferenceCount.ToString(), titleStyle);
				float remain = Mathf.Max(0, GameEntry.Pool.ReleaseAssetBundleNextRunTime - (Time.time - item.Value.LastUseTime));

				GUILayout.Label(remain.ToString(), titleStyle);
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
		//=================================变量计数结束==========================

		serializedObject.ApplyModifiedProperties();
		//重绘
		Repaint();
		#endregion
	}
}
