
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System;

//namespace YouYou
//{
//	[CustomEditor(typeof(PoolComponent))]
//	public class PoolComponentInspector : Editor
//	{
//		/// <summary>
//		/// 类对象池 释放间隔
//		/// </summary>
//		private SerializedProperty ReleaseClassObjectInterval = null;
//		/// <summary>
//		/// 类对象池 分组
//		/// </summary>
//		private SerializedProperty m_GameObjectPoolGroups = null;

//		/// <summary>
//		/// 锁定的AssetBundle(不会释放)
//		/// </summary>
//		private SerializedProperty LockedAssetBundle = null;
//		/// <summary>
//		/// AssetBundle池 释放间隔
//		/// </summary>
//		private SerializedProperty ReleaseResourceInterval = null;

//		/// <summary>
//		/// Asset池 释放间隔
//		/// </summary>
//		private SerializedProperty ReleaseAssetInterval = null;
//		/// <summary>
//		/// 分类资源池 显示与隐藏
//		/// </summary>
//		private SerializedProperty ShowAssetPool = null;


//		public override void OnInspectorGUI()
//		{
//			serializedObject.Update();

//			PoolComponent component = base.target as PoolComponent;

//			#region 游戏物体对象池
//			EditorGUILayout.PropertyField(m_GameObjectPoolGroups, true);
//			GUILayout.Space(30);
//			#endregion

//			#region 类对象池
//			int clearInterval = (int)EditorGUILayout.Slider("类对象池 释放间隔", ReleaseClassObjectInterval.intValue, 10, 1800);
//			if (clearInterval != ReleaseClassObjectInterval.intValue)
//			{
//				component.ReleaseClassObjectInterval = clearInterval;
//			}
//			else
//			{
//				ReleaseClassObjectInterval.intValue = clearInterval;
//			}
//			GUILayout.BeginVertical("box");
//			GUILayout.BeginHorizontal("box");
//			GUILayout.Label("类名");
//			GUILayout.Label("池中数量", GUILayout.Width(50));
//			GUILayout.Label("常驻数量", GUILayout.Width(50));
//			GUILayout.EndHorizontal();

//			if (component != null && component.PoolManager != null)
//			{
//				foreach (var item in component.PoolManager.ClassObjectPool.InspectorDic)
//				{
//					GUILayout.BeginHorizontal("box");
//					GUILayout.Label(item.Key.Name);
//					GUILayout.Label(item.Value.ToString(), GUILayout.Width(50));

//					int key = item.Key.GetHashCode();
//					byte resideCount = 0;
//					component.PoolManager.ClassObjectPool.ClassObjectCount.TryGetValue(key, out resideCount);

//					GUILayout.Label(resideCount.ToString(), GUILayout.Width(50));
//					GUILayout.EndHorizontal();
//				}
//			}
//			GUILayout.EndVertical();
//			//================类对象池变量计数===============
//			GUILayout.BeginVertical("box");
//			GUILayout.BeginHorizontal("box");
//			GUILayout.Label("变量名");
//			GUILayout.Label("计数", GUILayout.Width(50));
//			GUILayout.EndHorizontal();

//			if (component != null)
//			{
//				foreach (var item in component.VarObjectInspectorDic)
//				{
//					GUILayout.BeginHorizontal("box");
//					GUILayout.Label(item.Key.Name);
//					GUILayout.Label(item.Value.ToString(), GUILayout.Width(50));
//					GUILayout.EndHorizontal();
//				}
//			}
//			GUILayout.EndVertical();
//			GUILayout.Space(30);
//			#endregion

//			#region 资源包池
//			EditorGUILayout.PropertyField(LockedAssetBundle, true);
//			//绘制滑动条 资源包池释放间隔
//			int releaseAssetBundleInterval = (int)EditorGUILayout.Slider("AssetBundle池 释放间隔", ReleaseResourceInterval.intValue, 10, 1800);
//			if (releaseAssetBundleInterval != ReleaseResourceInterval.intValue)
//			{
//				component.ReleaseResourceInterval = releaseAssetBundleInterval;
//			}
//			else
//			{
//				ReleaseResourceInterval.intValue = releaseAssetBundleInterval;
//			}
//			//===================资源池变量计数==================
//			GUILayout.BeginVertical("box");
//			GUILayout.BeginHorizontal("box");
//			GUILayout.Label("资源包名");
//			GUILayout.Label("计数", GUILayout.Width(50));
//			GUILayout.EndHorizontal();

//			if (component != null && component.PoolManager != null)
//			{
//				foreach (var item in component.PoolManager.AssetBundlePool.InspectorDic)
//				{
//					GUILayout.BeginHorizontal("box");
//					GUILayout.Label(item.Key);
//					GUILayout.Label(item.Value.ToString(), GUILayout.Width(50));
//					GUILayout.EndHorizontal();
//				}
//			}
//			GUILayout.EndVertical();
//			GUILayout.Space(30);
//			#endregion

//			#region 分类资源池
//			int releaseAssetInterval = (int)EditorGUILayout.Slider("Asset池 释放间隔", ReleaseAssetInterval.intValue, 10, 1800);
//			if (releaseAssetInterval != ReleaseAssetInterval.intValue)
//			{
//				component.ReleaseAssetInterval = releaseAssetInterval;
//			}
//			else
//			{
//				ReleaseAssetInterval.intValue = releaseAssetInterval;
//			}
//			bool showAssetPool = EditorGUILayout.Toggle("显示Asset池", ShowAssetPool.boolValue);
//			if (showAssetPool != ShowAssetPool.boolValue)
//			{
//				component.ShowAssetPool = showAssetPool;
//			}
//			else
//			{
//				ShowAssetPool.boolValue = showAssetPool;
//			}

//			if (showAssetPool)
//			{
//				var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
//				while (enumerator.MoveNext())
//				{
//					AssetCategory assetCategory = (AssetCategory)enumerator.Current;
//					if (assetCategory == AssetCategory.None) continue;

//					GUILayout.BeginVertical("box");
//					GUILayout.BeginHorizontal("box");
//					GUILayout.Label("分类资源-" + assetCategory.ToString());
//					GUILayout.Label("计数", GUILayout.Width(50));
//					GUILayout.EndHorizontal();

//					if (component != null && component.PoolManager != null)
//					{
//						foreach (var item in component.PoolManager.AssetPool[assetCategory].InspectorDic)
//						{
//							GUILayout.BeginHorizontal("box");
//							GUILayout.Label(item.Key);
//							GUILayout.Label(item.Value.ToString(), GUILayout.Width(50));
//							GUILayout.EndHorizontal();
//						}
//					}
//					GUILayout.EndVertical();
//				}
//			}
//			GUILayout.Space(30);
//			#endregion

//			serializedObject.ApplyModifiedProperties();
//			//刷新UI(重绘)
//			Repaint();
//		}

//		void OnEnable()
//		{
//			//建立属性关系
//			ReleaseClassObjectInterval = serializedObject.FindProperty("ReleaseClassObjectInterval");
//			m_GameObjectPoolGroups = serializedObject.FindProperty("m_GameObjectPoolGroups");

//			LockedAssetBundle = serializedObject.FindProperty("LockedAssetBundle");
//			ReleaseResourceInterval = serializedObject.FindProperty("ReleaseResourceInterval");

//			ReleaseAssetInterval = serializedObject.FindProperty("ReleaseAssetInterval");
//			ShowAssetPool = serializedObject.FindProperty("ShowAssetPool");


//			serializedObject.ApplyModifiedProperties();
//		}

//	}
//}