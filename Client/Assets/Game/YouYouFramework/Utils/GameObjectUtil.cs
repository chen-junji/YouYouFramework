using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using YouYou;
using System.IO;
using System;
using Object = UnityEngine.Object;

/// <summary>
/// GameObject拓展类
/// </summary>
public static class GameObjectUtil
{
    /// <summary>
    /// 获取或创建组件
    /// </summary>
    public static T GetOrCreatComponent<T>(this GameObject obj) where T : MonoBehaviour
    {
        T t = obj.GetComponent<T>();
        if (t == null)
        {
            t = obj.AddComponent<T>();
        }
        return t;
    }
    /// <summary>
    /// 设置当前gameObject及所有子物体的层
    /// </summary>
    public static void SetLayer(this GameObject obj, string layerName)
    {
        Transform[] transArr = obj.transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transArr.Length; i++)
        {
            transArr[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    /// <summary>
    /// 自动加载图片
    /// </summary>
    public static async void AutoLoadTexture(this Image img, string imgPath, bool isSetNativeSize = false)
    {
        Object asset = await GameEntry.Loader.LoadMainAssetAsync<Object>(imgPath, img.gameObject);
        Sprite obj = null;
        if (asset is Sprite)
        {
            obj = (Sprite)asset;
        }
        else
        {
            Texture2D texture = (Texture2D)asset;
            obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        img.sprite = obj;
        if (isSetNativeSize) img.SetNativeSize();
    }
    /// <summary>
    /// 自动加载图片
    /// </summary>
    public static async void AutoLoadTexture(this RawImage img, string imgPath, bool isSetNativeSize = false)
    {
        Texture2D asset = await GameEntry.Loader.LoadMainAssetAsync<Texture2D>(imgPath, img.gameObject);
        img.texture = asset;
        if (isSetNativeSize) img.SetNativeSize();
    }

    /// <summary>
    /// 设置特效渲染层级
    /// </summary>
    public static void SetEffectOrder(this Transform trans, int sortingOrder)
    {
        Renderer[] renderers = trans.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++) renderers[i].sortingOrder = sortingOrder;
    }
}