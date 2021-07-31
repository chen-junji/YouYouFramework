using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
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
		tree.AddAssetAtPath("YouYouFramework", "Game/YouYouFramework/YouYouAssets/AboutUs.asset");

		//宏设置
		tree.AddAssetAtPath("MacroSettings", "Game/YouYouFramework/YouYouAssets/MacroSettings.asset");

		//参数设置
		tree.AddAssetAtPath("ParamsSettings", "Game/YouYouFramework/YouYouAssets/ParamsSettings.asset");

		//AssetBundle打包管理
		tree.AddAssetAtPath("AssetBundleSettings", "Game/YouYouFramework/YouYouAssets/AssetBundleSettings.asset");

		//类对象池
		tree.AddAssetAtPath("PoolAnalyze/ClassObjectPool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_ClassObjectPool.asset");
		//AssetBundele池
		tree.AddAssetAtPath("PoolAnalyze/AssetBundlePool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_AssetBundlePool.asset");
		//Asset池
		tree.AddAssetAtPath("PoolAnalyze/AssetPool", "Game/YouYouFramework/YouYouAssets/PoolAnalyze_AssetPool.asset");

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

			if (name.Equals("AssetInfo.json", System.StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("Windows", System.StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("Windows.manifest", System.StringComparison.CurrentCultureIgnoreCase)

				|| name.Equals("Android", System.StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("Android.manifest", System.StringComparison.CurrentCultureIgnoreCase)

				|| name.Equals("iOS", System.StringComparison.CurrentCultureIgnoreCase)
				|| name.Equals("iOS.manifest", System.StringComparison.CurrentCultureIgnoreCase)
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

    #region SetFBXAnimationMode 设置文件动画循环为true
    [MenuItem("YouYouTools/设置文件动画循环为true")]
    public static void SetFBXAnimationMode()
    {
        Object[] objs = Selection.objects;
        for (int i = 0; i < objs.Length; i++)
        {
            string relatepath = AssetDatabase.GetAssetPath(objs[i]);

            if (relatepath.IsSuffix(".FBX"))
            {
                string path = Application.dataPath.Replace("Assets", "") + relatepath + ".meta";
                path = path.Replace("\\", "/");
                StreamReader fs = new StreamReader(path);
                List<string> ret = new List<string>();
                string line;
                while ((line = fs.ReadLine()) != null)
                {
                    line = line.Replace("\n", "");
                    if (line.IndexOf("loopTime: 0") != -1)
                    {
                        line = "      loopTime: 1";
                    }
                    ret.Add(line);
                }
                fs.Close();
                File.Delete(path);
                StreamWriter writer = new StreamWriter(path + ".tmp");
                foreach (var each in ret)
                {
                    writer.WriteLine(each);
                }
                writer.Close();
                File.Copy(path + ".tmp", path);
                File.Delete(path + ".tmp");
            }
        }
        AssetDatabase.Refresh();
    }
	#endregion

	#region 复制Hotfix.dll, Hotfix.pdb到Download/Hotfix
	//[InitializeOnLoad]
	public class Startup
	{
		private const string ScriptAssembliesDir = "Assets/Game/HotFix_Project~/bin/Debug";
		private const string CodeDir = "Assets/Download/Hotfix/";
		private const string HotfixDll = "Hotfix.dll";
		private const string HotfixPdb = "Hotfix.pdb";

		static Startup()
		{
			CopyHofix();
		}
		[MenuItem("YouYouTools/复制Hotfix.dll, Hotfix.pdb")]
		static void CopyHofix()
		{
			File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
			File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);
			Debug.Log($"复制Hotfix.dll, Hotfix.pdb到Download/Hotfix完成");
			AssetDatabase.Refresh();
		}
	}
    #endregion
}
