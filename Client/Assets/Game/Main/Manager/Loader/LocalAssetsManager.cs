using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Main
{
	/// <summary>
	/// 可写区资源管理器
	/// </summary>
	public class LocalAssetsManager
	{
		/// <summary>
		/// 本地版本文件路径
		/// </summary>
		public string LocalVersionFilePath
		{
			get
			{
				return string.Format("{0}/{1}", Application.persistentDataPath, YFConstDefine.VersionFileName);
			}
		}

		#region 获取可写区版本文件是否存在 GetVersionFileExists
		/// <summary>
		/// 获取可写区版本文件是否存在
		/// </summary>
		/// <returns></returns>
		public bool GetVersionFileExists()
		{
			return File.Exists(LocalVersionFilePath);
		}
		#endregion

		#region GetFileBuffer 获取本地文件的字节数组
		/// <summary>
		/// 获取本地文件的字节数组
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public byte[] GetFileBuffer(string path)
		{
			return IOUtil.GetFileBuffer(string.Format("{0}/{1}", Application.persistentDataPath, path));
		}

		public bool CheckFileExists(string assetBundlePath)
		{
			return File.Exists(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
		}
		#endregion

		#region SetResourceVersion 保存资源版本号
		/// <summary>
		/// 保存资源版本号
		/// </summary>
		/// <param name="version"></param>
		public void SetAssetVersion(string version)
		{
			PlayerPrefs.SetString(YFConstDefine.AssetVersion, version);
		}
		#endregion

		#region SaveVersionFile 保存版本文件
		/// <summary>
		/// 保存版本文件
		/// </summary>
		/// <param name="version"></param>
		public void SaveVersionFile(Dictionary<string, VersionFileEntity> dic)
		{
			string json = dic.ToJson();
			IOUtil.CreateTextFile(LocalVersionFilePath, json);
		}
		#endregion

		#region GetAssetBundleVersionList 加载可写区资源包信息
		/// <summary>
		/// 加载可写区资源包信息
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public Dictionary<string, VersionFileEntity> GetAssetBundleVersionList(ref string version)
		{
			version = PlayerPrefs.GetString(YFConstDefine.AssetVersion);
			string json = IOUtil.GetFileText(LocalVersionFilePath);
			return json.ToObject<Dictionary<string, VersionFileEntity>>();
		}
		#endregion
	}
}
