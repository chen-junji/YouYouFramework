using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYou
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]//脚本依赖
    public class YouYouImage : MonoBehaviour
    {
        [Header("本地化语言Key")]
        [SerializeField]
        private string m_Localization;

        private Image m_Image;

        private void Start()
        {
            m_Image = GetComponent<Image>();

            if (GameEntry.Localization != null)
            {
                string path = GameUtil.GetUIResPath(GameEntry.Localization.GetString(m_Localization));

                GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAction(path, onComplete: (Texture2D texture) =>
                {
                    Sprite obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    m_Image.sprite = obj;
                    m_Image.SetNativeSize();
                });
            }
        }
    }
}