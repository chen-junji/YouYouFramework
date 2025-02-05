using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YouYouFramework;
using YouYouMain;


[CustomEditor(typeof(PoolAnalyze_ClassObjectPool))]
public class PoolAnalyze_ClassObjectPoolEditor : Editor
{

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(10);

		GUIStyle titleStyle = new GUIStyle();
		titleStyle.normal.textColor = new Color(102, 232, 255);

		if (GameEntry.Pool != null)
		{
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("下次释放剩余时间: " + Mathf.Abs(Time.time - (GameEntry.Pool.ClassObjectPool.ReleaseNextRunTime + GameEntry.ParamsSettings.PoolReleaseClassObjectInterval)), titleStyle);
			GUILayout.EndHorizontal();
		}
		GUILayout.Space(10);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("类名");
		GUILayout.Label("池中数量", GUILayout.Width(50));
		GUILayout.Label("常驻数量", GUILayout.Width(50));
		GUILayout.EndHorizontal();

		if (GameEntry.Pool != null)
		{
			foreach (var item in GameEntry.Pool.ClassObjectPool.InspectorDic)
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Label(item.Key.Name);

				titleStyle.fixedWidth = 50;

				GUILayout.Label(item.Value.ToString(), titleStyle);

				int key = item.Key.GetHashCode();
				byte resideCount = 0;
                GameEntry.Pool.ClassObjectPool.ClassObjectCount.TryGetValue(key, out resideCount);

				GUILayout.Label(resideCount.ToString(), titleStyle);
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
		//重绘
		Repaint();
	}
}
