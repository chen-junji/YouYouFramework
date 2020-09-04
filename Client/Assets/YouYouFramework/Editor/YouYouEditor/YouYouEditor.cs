using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class YouYouEditor : OdinMenuEditorWindow
{
	[MenuItem("YouYouTools/YouYouEditor")]
	private static void OpenYouYouEditor()
	{
		var window = GetWindow<YouYouEditor>();
		window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 500);
	}
	protected override OdinMenuTree BuildMenuTree()
	{
		var tree = new OdinMenuTree(true);

		//简介
		tree.AddAssetAtPath("YouYouFramework", "YouYouFramework/YouYouAssets/AboutUs.asset");

		//宏设置
		tree.AddAssetAtPath("MacroSettings", "YouYouFramework/YouYouAssets/MacroSettings.asset");

		//参数设置
		tree.AddAssetAtPath("ParamsSettings", "YouYouFramework/YouYouAssets/ParamsSettings.asset");

		//AssetBundle打包管理
		tree.AddAssetAtPath("AssetBundleSettings", "YouYouFramework/YouYouAssets/AssetBundleSettings.asset");

		//类对象池
		tree.AddAssetAtPath("PoolAnalyze/ClassObjectPool", "YouYouFramework/YouYouAssets/PoolAnalyze_ClassObjectPool.asset");
		//AssetBundele池
		tree.AddAssetAtPath("PoolAnalyze/AssetBundlePool", "YouYouFramework/YouYouAssets/PoolAnalyze_AssetBundlePool.asset");
		//Asset池
		tree.AddAssetAtPath("PoolAnalyze/AssetPool", "YouYouFramework/YouYouAssets/PoolAnalyze_AssetPool.asset");

		return tree;
	}

	#region AssetBundleCopyToStreamingAsstes 初始资源拷贝到StreamingAsstes
	[MenuItem("YouYouTools/资源管理/初始资源拷贝到StreamingAsstes")]
	public static void AssetBundleCopyToStreamingAsstes()
	{
		string toPath = Application.streamingAssetsPath + "/AssetBundles/";

		if (Directory.Exists(toPath))
		{
			Directory.Delete(toPath, true);
		}
		Directory.CreateDirectory(toPath);

		IOUtil.CopyDirectory(Application.persistentDataPath, toPath);

		//重新生成版本文件
		//1.先读取persistentDataPath里边的版本文件 这个版本文件里 存放了所有的资源包信息

		byte[] buffer = IOUtil.GetFileBuffer(Application.persistentDataPath + "/VersionFile.bytes");
		string version = "";
		Dictionary<string, AssetBundleInfoEntity> dic = ResourceManager.GetAssetBundleVersionList(buffer, ref version);
		Dictionary<string, AssetBundleInfoEntity> newDic = new Dictionary<string, AssetBundleInfoEntity>();

		DirectoryInfo directory = new DirectoryInfo(toPath);

		//拿到文件夹下所有文件
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		for (int i = 0; i < arrFiles.Length; i++)
		{
			FileInfo file = arrFiles[i];
			string fullName = file.FullName.Replace("\\", "/"); //全名 包含路径扩展名
			string name = fullName.Replace(toPath, "").Replace(".assetbundle", "").Replace(".unity3d", "");

			if (name.Equals("AssetInfo.json", StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("Windows", StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("Windows.manifest", StringComparison.CurrentCultureIgnoreCase)

				|| name.Equals("Android", StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("Android.manifest", StringComparison.CurrentCultureIgnoreCase)

				|| name.Equals("iOS", StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("iOS.manifest", StringComparison.CurrentCultureIgnoreCase)
				)
			{
				File.Delete(file.FullName);
				continue;
			}

			AssetBundleInfoEntity entity = null;
			dic.TryGetValue(name, out entity);


			if (entity != null)
			{
				newDic[name] = entity;
			}
		}

		StringBuilder sbContent = new StringBuilder();
		sbContent.AppendLine(version);

		foreach (var item in newDic)
		{
			AssetBundleInfoEntity entity = item.Value;
			string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", entity.AssetBundleName, entity.MD5, entity.Size, entity.IsFirstData ? 1 : 0, entity.IsEncrypt ? 1 : 0);
			sbContent.AppendLine(strLine);
		}

		IOUtil.CreateTextFile(toPath + "VersionFile.txt", sbContent.ToString());

		//=======================
		MMO_MemoryStream ms = new MMO_MemoryStream();
		string str = sbContent.ToString().Trim();
		string[] arr = str.Split('\n');
		int len = arr.Length;
		ms.WriteInt(len);
		for (int i = 0; i < len; i++)
		{
			if (i == 0)
			{
				ms.WriteUTF8String(arr[i]);
			}
			else
			{
				string[] arrInner = arr[i].Split('|');
				ms.WriteUTF8String(arrInner[0]);
				ms.WriteUTF8String(arrInner[1]);
				ms.WriteInt(int.Parse(arrInner[2]));
				ms.WriteByte(byte.Parse(arrInner[3]));
				ms.WriteByte(byte.Parse(arrInner[4]));
			}
		}

		string filePath = toPath + "/VersionFile.bytes"; //版本文件路径
		buffer = ms.ToArray();
		buffer = ZlibHelper.CompressBytes(buffer);
		FileStream fs = new FileStream(filePath, FileMode.Create);
		fs.Write(buffer, 0, buffer.Length);
		fs.Close();

		AssetDatabase.Refresh();
		Debug.Log("初始资源拷贝到StreamingAsstes完毕");
	}
	#endregion

	#region AssetBundleOpenPersistentDataPath 打开persistentDataPath
	[MenuItem("YouYouTools/资源管理/打开persistentDataPath")]
	public static void AssetBundleOpenPersistentDataPath()
	{
		string output = Application.persistentDataPath;
		if (!Directory.Exists(output))
		{
			Directory.CreateDirectory(output);
		}
		output = output.Replace("/", "\\");
		System.Diagnostics.Process.Start("explorer.exe", output);
	}
	#endregion

	#region YouYouPrefab
	[MenuItem("GameObject/UI/YouYouText", false)]
	private static void MakeYouYouText(MenuCommand menuCommand)
	{
		GameObject obj = MakeYouYouPrefab("YouYouText", menuCommand.context as GameObject);
		if (menuCommand.context == null) AttachToCanvas(obj);
	}
	[MenuItem("CONTEXT/Text/ChangeToYouYouText")]
	public static void ChangeToYouYouText(MenuCommand menuCommand)
	{
		Text currText = menuCommand.context as Text;

		GameObject newObj = MakeYouYouPrefab("YouYouText", currText.transform.parent.gameObject);
		YouYouText youYouText = newObj.GetComponent<YouYouText>();
		youYouText.rectTransform.sizeDelta = currText.rectTransform.sizeDelta;

		youYouText.text = currText.text;
		youYouText.color = currText.color;
		youYouText.font = currText.font;
		youYouText.fontSize = currText.fontSize;
		youYouText.fontStyle = currText.fontStyle;
		youYouText.alignment = currText.alignment;
		youYouText.supportRichText = currText.supportRichText;
		youYouText.raycastTarget = currText.raycastTarget;

		UnityEngine.Object.DestroyImmediate(currText.gameObject);
	}
	[MenuItem("GameObject/UI/YouYouImage", false)]
	private static void MakeYouYouImage(MenuCommand menuCommand)
	{
		GameObject obj = MakeYouYouPrefab("YouYouImage", menuCommand.context as GameObject);
		if (menuCommand.context == null) AttachToCanvas(obj);
	}
	[MenuItem("CONTEXT/Image/ChangeToYouYouImage")]
	public static void ChangeToYouYouImage(MenuCommand menuCommand)
	{
		Image image = menuCommand.context as Image;

		GameObject newObj = MakeYouYouPrefab("YouYouImage", image.transform.parent.gameObject);
		YouYouImage youYouImage = newObj.GetComponent<YouYouImage>();
		youYouImage.rectTransform.sizeDelta = image.rectTransform.sizeDelta;

		youYouImage.sprite = image.sprite;
		youYouImage.color = image.color;
		youYouImage.raycastTarget = image.raycastTarget;

		UnityEngine.Object.DestroyImmediate(image.gameObject);
	}

	private static GameObject MakeYouYouPrefab(string prefabName, GameObject parent)
	{
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/YouYouFramework/Editor/YouYouPrefabs/" + prefabName + ".prefab");
		GameObject obj = UnityEngine.Object.Instantiate(prefab);

		obj.name = prefab.name;
		GameObjectUtility.SetParentAndAlign(obj, parent);
		Selection.activeObject = obj;

		return obj;
	}
	private static void AttachToCanvas(GameObject gameObject)
	{
		Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
		if (canvas == null)
		{
			GameObject obj = new GameObject();
			canvas = obj.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.gameObject.AddComponent<CanvasScaler>();
			canvas.gameObject.AddComponent<GraphicRaycaster>();
		}
		gameObject.transform.SetParent(canvas.transform);
	}
	#endregion
}
