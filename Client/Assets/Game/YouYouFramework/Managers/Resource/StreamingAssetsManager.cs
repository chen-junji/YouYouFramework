using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.Networking;

namespace YouYou
{
	/// <summary>
	/// StreamingAssets管理器
	/// </summary>
	public class StreamingAssetsManager
	{
		#region ReadStreamingAsset 读取StreamingAssets下的资源
		/// <summary>
		/// 读取StreamingAssets下的资源
		/// </summary>
		/// <param name="url">资源路径</param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		private IEnumerator ReadStreamingAsset(string url, Action<byte[]> onComplete)
		{
			var uri = new System.Uri(Path.Combine(Application.streamingAssetsPath, url));
			using (UnityWebRequest request = UnityWebRequest.Get(uri.AbsoluteUri))
			{
				yield return request.SendWebRequest();

				if (request.isNetworkError || request.isHttpError)
				{
					onComplete?.Invoke(null);
				}
				else
				{
					onComplete?.Invoke(request.downloadHandler.data);
				}
			}
		}
		#endregion

		#region ReadAssetBundle 读取只读区资源包
		/// <summary>
		/// 读取只读区资源包
		/// </summary>
		/// <param name="fileUrl">资源路径</param>
		/// <param name="onComplete"></param>
		public void ReadAssetBundle(string fileUrl, Action<byte[]> onComplete)
		{
			GameEntry.Instance.StartCoroutine(ReadStreamingAsset(fileUrl + "AssetBundle", onComplete));
		}
		#endregion
	}
}