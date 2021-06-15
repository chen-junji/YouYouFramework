using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYou
{
	/// <summary>
	/// Image×Ô¶¨Òå×ÓÀà
	/// </summary>
	public class YouYouImage : Image
	{
		[Header("本地化语言Key")]
		[SerializeField]
		private string m_Localization;

		protected override void Start()
		{
			base.Start();
			if (GameEntry.Localization != null)
			{
				string path = GameUtil.GetUIResPath(GameEntry.Localization.GetString(m_Localization));

				GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(path, onComplete: (Texture2D texture) =>
				{
					Sprite obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
					sprite = obj;
					SetNativeSize();
				});
			}
		}
	}
}