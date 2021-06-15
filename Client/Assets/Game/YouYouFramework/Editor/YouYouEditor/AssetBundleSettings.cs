using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YouYou;

[CreateAssetMenu]
public class AssetBundleSettings : ScriptableObject
{
	public enum CusBuildTarget
	{
		Windows,
		Android,
		IOS
	}

	[HorizontalGroup("Common", LabelWidth = 70)]
	[VerticalGroup("Common/Left")]
	[LabelText("资源版本号")]
	public string ResourceVersion = "1.0.1";

	[PropertySpace(10)]
	[VerticalGroup("Common/Left")]
	[LabelText("目标平台")]
	public CusBuildTarget CurrBuildTarget;

	public BuildTarget GetBuildTarget()
	{
		switch (CurrBuildTarget)
		{
			default:
			case CusBuildTarget.Windows:
				return BuildTarget.StandaloneWindows;
			case CusBuildTarget.Android:
				return BuildTarget.Android;
			case CusBuildTarget.IOS:
				return BuildTarget.iOS;
		}
	}

	[PropertySpace(10)]
	[VerticalGroup("Common/Left")]
	[LabelText("参数")]
	public BuildAssetBundleOptions Options;

	[VerticalGroup("Common/Right")]
	[Button(ButtonSizes.Medium)]
	[LabelText("更新版本号")]
	public void UpdateResourceVersion()
	{
		string version = ResourceVersion;
		string[] arr = version.Split('.');

		int shortVersion = 0;
		int.TryParse(arr[2], out shortVersion);
		version = string.Format("{0}.{1}.{2}", arr[0], arr[1], ++shortVersion);
		ResourceVersion = version;
	}


	[VerticalGroup("Common/Right")]
	[Button(ButtonSizes.Medium)]
	[LabelText("清空资源包")]
	public void ClearAssetBundle()
	{
		if (Directory.Exists(TempPath))
		{
			Directory.Delete(TempPath, true);
		}
		EditorUtility.DisplayDialog("", "清空完毕", "确定");
	}

	/// <summary>
	/// 要收集的资源包
	/// </summary>
	List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

	[VerticalGroup("Common/Right")]
	[Button(ButtonSizes.Medium)]
	[LabelText("打包")]
	public void BuildAssetBundle()
	{
		builds.Clear();
		int len = Datas.Length;
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			if (assetBundleData.IsCanEditor)
			{
				int lenPath = assetBundleData.Path.Length;
				for (int j = 0; j < lenPath; j++)
				{
					//打包这个路径
					string path = assetBundleData.Path[j];
					BuildAssetBundleForPath(path, assetBundleData.Overall);
				}
			}
		}

		if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);

		if (builds.Count == 0) return;
		Debug.Log("builds count=" + builds.Count);

		BuildPipeline.BuildAssetBundles(TempPath, builds.ToArray(), Options, GetBuildTarget());
		Debug.Log("临时资源包打包完毕");

		CopyFile(TempPath);
		Debug.Log("拷贝到输出目录完毕");

		AssetBundleEncrypt();
		Debug.Log("资源包加密完毕");

		CreateDependenciesFile();
		Debug.Log("AssetInfo==生成依赖关系文件完毕");

		CreateVersionFile();
		Debug.Log("VersionFile==生成版本文件完毕");
	}

	#region TempPath OutPath
	/// <summary>
	/// 临时目录
	/// </summary>
	public string TempPath
	{
		get
		{
			return Application.dataPath + "/../" + AssetBundleSavePath + "/" + ResourceVersion + "_Temp/" + CurrBuildTarget;
		}
	}

	/// <summary>
	/// 输出目录
	/// </summary>
	public string OutPath
	{
		get
		{
			return TempPath.Replace("_Temp", "");
		}
	}
	#endregion

	#region CopyFile 拷贝文件到正式目录
	/// <summary>
	/// 拷贝文件到正式目录
	/// </summary>
	/// <param name="oldPath"></param>
	private void CopyFile(string oldPath)
	{
		if (Directory.Exists(OutPath))
		{
			Directory.Delete(OutPath, true);
		}

		IOUtil.CopyDirectory(oldPath, OutPath);
		DirectoryInfo directory = new DirectoryInfo(OutPath);

		//拿到文件夹下所有文件
		FileInfo[] arrFiles = directory.GetFiles("*.y", SearchOption.AllDirectories);
		int len = arrFiles.Length;
		for (int i = 0; i < len; i++)
		{
			FileInfo file = arrFiles[i];
			File.Move(file.FullName, file.FullName.Replace(".ab.y", ".assetbundle"));
		}
	}
	#endregion

	#region AssetBundleEncrypt 资源包加密
	/// <summary>
	/// 资源包加密
	/// </summary>
	/// <param name="path"></param>
	private void AssetBundleEncrypt()
	{
		int len = Datas.Length;
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			if (assetBundleData.IsEncrypt && assetBundleData.IsCanEditor)
			{
				//如果需要加密
				for (int j = 0; j < assetBundleData.Path.Length; j++)
				{
					string path = OutPath + "/" + assetBundleData.Path[j];

					if (assetBundleData.Overall)
					{
						//不是遍历文件夹打包 说明这个路径就是一个包
						path = path + ".assetbundle";

						AssetBundleEncryptFile(path);
					}
					else
					{
						AssetBundleEncryptFolder(path);
					}
				}
			}
		}
	}

	/// <summary>
	/// 加密文件夹下所有文件
	/// </summary>
	/// <param name="folderPath"></param>
	private void AssetBundleEncryptFolder(string folderPath, bool isDelete = false)
	{
		DirectoryInfo directory = new DirectoryInfo(folderPath);

		//拿到文件夹下所有文件
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		foreach (FileInfo file in arrFiles)
		{
			AssetBundleEncryptFile(file.FullName, isDelete);
		}
	}

	/// <summary>
	/// 加密文件
	/// </summary>
	/// <param name="filePath"></param>
	private void AssetBundleEncryptFile(string filePath, bool isDelete = false)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		byte[] buffer = null;

		using (FileStream fs = new FileStream(filePath, FileMode.Open))
		{
			buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
		}

		buffer = SecurityUtil.Xor(buffer);

		using (FileStream fs = new FileStream(filePath, FileMode.Create))
		{
			fs.Write(buffer, 0, buffer.Length);
			fs.Flush();
		}
	}
	#endregion

	#region OnCreateDependenciesFile 生成依赖关系文件
	/// <summary>
	/// 生成依赖关系文件
	/// </summary>
	private void CreateDependenciesFile()
	{
		//第一次循环 把所有的Asset存储到一个列表里

		//临时列表
		List<AssetEntity> tempLst = new List<AssetEntity>();

		int len = Datas.Length;
		//循环设置文件夹包括子文件里边的项
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			for (int j = 0; j < assetBundleData.Path.Length; j++)
			{
				string path = Application.dataPath + "/" + assetBundleData.Path[j];
				//Debug.LogError("CreateDependenciesFile path=" + path);
				CollectFileInfo(tempLst, path);
			}
		}

		//
		len = tempLst.Count;

		//资源列表
		List<AssetEntity> assetList = new List<AssetEntity>();

		for (int i = 0; i < len; i++)
		{
			AssetEntity entity = tempLst[i];

			AssetEntity newEntity = new AssetEntity();
			newEntity.AssetFullName = entity.AssetFullName;
			newEntity.AssetBundleName = entity.AssetBundleName;

			assetList.Add(newEntity);

			//场景不需要检查依赖项
			//if (entity.Category == AssetCategory.Scenes) continue;

			newEntity.DependsAssetList = new List<AssetDependsEntity>();

			string[] arr = AssetDatabase.GetDependencies(entity.AssetFullName);
			foreach (string str in arr)
			{
				if (!str.Equals(newEntity.AssetFullName, StringComparison.CurrentCultureIgnoreCase) && GetIsAsset(tempLst, str))
				{
					AssetDependsEntity assetDepends = new AssetDependsEntity();
					assetDepends.AssetFullName = str;

					//把依赖资源 加入到依赖资源列表
					newEntity.DependsAssetList.Add(assetDepends);
				}
			}
		}

		//生成一个Json文件
		string targetPath = OutPath;
		if (!Directory.Exists(targetPath))
		{
			Directory.CreateDirectory(targetPath);
		}

		string strJsonFilePath = targetPath + "/AssetInfo.json"; //版本文件路径
		IOUtil.CreateTextFile(strJsonFilePath, assetList.ToJson());
		Debug.Log("生成 AssetInfo.json 完毕");

		MMO_MemoryStream ms = new MMO_MemoryStream();
		//生成二进制文件
		len = assetList.Count;
		ms.WriteInt(len);

		for (int i = 0; i < len; i++)
		{
			AssetEntity entity = assetList[i];
			ms.WriteUTF8String(entity.AssetFullName);
			ms.WriteUTF8String(entity.AssetBundleName);

			if (entity.DependsAssetList != null)
			{
				//添加依赖资源
				int depLen = entity.DependsAssetList.Count;
				ms.WriteInt(depLen);
				for (int j = 0; j < depLen; j++)
				{
					AssetDependsEntity assetDepends = entity.DependsAssetList[j];
					ms.WriteUTF8String(assetDepends.AssetFullName);
				}
			}
			else
			{
				ms.WriteInt(0);
			}
		}

		string filePath = targetPath + "/AssetInfo.bytes"; //版本文件路径
		byte[] buffer = ms.ToArray();
		buffer = ZlibHelper.CompressBytes(buffer);
		FileStream fs = new FileStream(filePath, FileMode.Create);
		fs.Write(buffer, 0, buffer.Length);
		fs.Close();
		fs.Dispose();
	}

	/// <summary>
	/// 判断某个资源是否存在于资源列表
	/// </summary>
	/// <param name="tempLst"></param>
	/// <param name="assetFullName"></param>
	/// <returns></returns>
	private bool GetIsAsset(List<AssetEntity> tempLst, string assetFullName)
	{
		int len = tempLst.Count;
		for (int i = 0; i < len; i++)
		{
			AssetEntity entity = tempLst[i];
			if (entity.AssetFullName.Equals(assetFullName, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
	#endregion



	#region CollectFileInfo 收集文件信息
	/// <summary>
	/// 收集文件信息
	/// </summary>
	/// <param name="tempLst"></param>
	/// <param name="folderPath"></param>
	private void CollectFileInfo(List<AssetEntity> tempLst, string folderPath)
	{
		DirectoryInfo directory = new DirectoryInfo(folderPath);

		//拿到文件夹下所有文件
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		for (int i = 0; i < arrFiles.Length; i++)
		{
			FileInfo file = arrFiles[i];
			if (file.Extension == ".meta") continue;
			if (file.FullName.IndexOf(".idea") != -1) continue;

			//绝对路径
			string filePath = file.FullName;
			//Debug.LogError("filePath==" + filePath);

			AssetEntity entity = new AssetEntity();
			//相对路径
			entity.AssetFullName = filePath.Substring(filePath.IndexOf("Assets\\")).Replace("\\", "/");
			//Debug.LogError("AssetFullName==" + entity.AssetFullName);

			entity.AssetBundleName = (GetAssetBundleName(entity.AssetFullName) + ".assetbundle").ToLower();
			tempLst.Add(entity);
		}
	}
	#endregion

	#region CreateVersionFile 生成版本文件
	/// <summary>
	/// 生成版本文件
	/// </summary>
	private void CreateVersionFile()
	{
		string path = OutPath;
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		string strVersionFilePath = path + "/VersionFile.txt"; //版本文件路径

		//如果版本文件存在 则删除
		IOUtil.DeleteFile(strVersionFilePath);

		StringBuilder sbContent = new StringBuilder();

		DirectoryInfo directory = new DirectoryInfo(path);

		//拿到文件夹下所有文件
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		sbContent.AppendLine(this.ResourceVersion);
		for (int i = 0; i < arrFiles.Length; i++)
		{
			FileInfo file = arrFiles[i];

			if (file.Extension == ".manifest")
			{
				continue;
			}
			string fullName = file.FullName; //全名 包含路径扩展名

			//相对路径
			string name = fullName.Substring(fullName.IndexOf(CurrBuildTarget.ToString()) + CurrBuildTarget.ToString().Length + 1);

			string md5 = EncryptUtil.GetFileMD5(fullName); //文件的MD5
			if (md5 == null) continue;

			string size = file.Length.ToString(); //文件大小

			bool isFirstData = false; //是否初始数据
			bool isEncrypt = false;
			bool isBreak = false;

			for (int j = 0; j < Datas.Length; j++)
			{
				foreach (string mPath in Datas[j].Path)
				{
					string tempPath = mPath;

					name = name.Replace("\\", "/");
					if (name.IndexOf(tempPath, StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						isFirstData = Datas[j].IsFirstData;
						isEncrypt = Datas[j].IsEncrypt;
						isBreak = true;
						break;
					}
				}
				if (isBreak) break;
			}

			string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", name, md5, size, isFirstData ? 1 : 0, isEncrypt ? 1 : 0);
			sbContent.AppendLine(strLine);
		}

		IOUtil.CreateTextFile(strVersionFilePath, sbContent.ToString());

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
				ms.WriteULong(ulong.Parse(arrInner[2]));
				ms.WriteByte(byte.Parse(arrInner[3]));
				ms.WriteByte(byte.Parse(arrInner[4]));
			}
		}

		string filePath = path + "/VersionFile.bytes"; //版本文件路径
		byte[] buffer = ms.ToArray();
		buffer = ZlibHelper.CompressBytes(buffer);
		FileStream fs = new FileStream(filePath, FileMode.Create);
		fs.Write(buffer, 0, buffer.Length);
		fs.Close();
		fs.Dispose();
	}

	#endregion

	#region GetAssetBundleName 获取资源包的名称
	/// <summary>
	/// 获取资源包的名称
	/// </summary>
	/// <param name="assetFullName"></param>
	/// <returns></returns>
	private string GetAssetBundleName(string assetFullName)
	{
		int len = Datas.Length;
		//循环设置文件夹包括子文件里边的项
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			for (int j = 0; j < assetBundleData.Path.Length; j++)
			{
				if (assetFullName.IndexOf(assetBundleData.Path[j], StringComparison.CurrentCultureIgnoreCase) > -1)
				{
					if (assetBundleData.Overall)
					{
						//文件夹是个整包 则返回这个特文件夹名字
						return assetBundleData.Path[j].ToLower();
					}
					else
					{
						//零散资源
						return assetFullName.Substring(0, assetFullName.LastIndexOf('.')).ToLower().Replace("assets/", "");
					}
				}
			}
		}
		return null;
	}
	#endregion


	/// <summary>
	/// 根据路径打包资源
	/// </summary>
	/// <param name="path"></param>
	/// <param name="overall">打成一个资源包</param>
	private void BuildAssetBundleForPath(string path, bool overall)
	{
		string fullPath = Application.dataPath + "/" + path;
		//Debug.LogError("fullPath=" + fullPath);
		//1.拿到文件夹下所有文件
		DirectoryInfo directory = new DirectoryInfo(fullPath);

		//拿到文件夹下所有文件
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		//Debug.LogError("arrFile=" + arrFile.Length);
		if (overall)
		{
			//打成一个资源包
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = path + ".ab";
			build.assetBundleVariant = "y";
			string[] arr = GetValidateFiles(arrFiles);
			build.assetNames = arr;
			builds.Add(build);
		}
		else
		{
			//每个文件打成一个包
			string[] arr = GetValidateFiles(arrFiles);
			for (int i = 0; i < arr.Length; i++)
			{
				AssetBundleBuild build = new AssetBundleBuild();
				build.assetBundleName = arr[i].Substring(0, arr[i].LastIndexOf('.')).Replace("Assets/", "") + ".ab";
				build.assetBundleVariant = "y";
				build.assetNames = new string[] { arr[i] };

				//Debug.LogError("assetBundleName==" + build.assetBundleName);
				builds.Add(build);
			}
		}
	}

	private string[] GetValidateFiles(FileInfo[] arrFiles)
	{
		List<string> lst = new List<string>();

		int len = arrFiles.Length;
		for (int i = 0; i < len; i++)
		{
			FileInfo file = arrFiles[i];
			if (!file.Extension.Equals(".meta", StringComparison.CurrentCultureIgnoreCase))
			{
				lst.Add("Assets" + file.FullName.Replace("\\", "/").Replace(Application.dataPath, ""));
			}
		}

		return lst.ToArray();
	}


	[LabelText("资源包保存路径")]
	[FolderPath]
	/// <summary>
	/// 资源包保存路径
	/// </summary>
	public string AssetBundleSavePath;

	[LabelText("勾选进行编辑")]
	public bool IsCanEditor;

	[EnableIf("IsCanEditor")]
	[BoxGroup("AssetBundleSettings")]
	public AssetBundleData[] Datas;

	//必须加上可序列化标记
	[Serializable]
	public class AssetBundleData
	{
		[LabelText("名称")]
		/// <summary>
		/// 名称
		/// </summary>
		public string Name;

		[LabelText("是否要打包")]
		public bool IsCanEditor = true;

		[LabelText("文件夹为一个资源包")]
		/// <summary>
		/// 打成一个资源包
		/// </summary>
		public bool Overall;

		[LabelText("是否初始资源")]
		/// <summary>
		/// 是否初始资源
		/// </summary>
		public bool IsFirstData;

		[LabelText("是否加密")]
		/// <summary>
		/// 是否加密
		/// </summary>
		public bool IsEncrypt;

		[FolderPath(ParentFolder = "Assets")]
		/// <summary>
		/// 路径
		/// </summary>
		public string[] Path;
	}
}