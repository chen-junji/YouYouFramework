using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYou
{
    /// <summary>
    /// Image自定义子类
    /// </summary>
    public class YouYouImage : Image
    {
        [Header("本地化语言的Key")]
        [SerializeField]
        private string m_Localization;

        protected override void Start()
        {
            base.Start();
            if (GameEntry.Localization != null)
            {
                string path = GameUtil.GetUIResPath(GameEntry.Localization.GetString(m_Localization));
                Texture2D texture = null;
#if UNITY_DEITOR
                texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path) as Texture2D;

                Sprite obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprite = obj;
                SetNativeSize();
#endif
            }
        }
    }
}