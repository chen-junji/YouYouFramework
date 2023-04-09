//===================================================
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using YouYou;
using DG.Tweening;
using UnityEngine.U2D;
using System.IO;
using System;
using Object = UnityEngine.Object;

/// <summary>
/// GameObject拓展类
/// </summary>
public static class GameObjectUtil
{
    #region GetOrCreatComponent 获取或创建组件
    /// <summary>
    /// 获取或创建组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T GetOrCreatComponent<T>(this GameObject obj) where T : MonoBehaviour
    {
        T t = obj.GetComponent<T>();
        if (t == null)
        {
            t = obj.AddComponent<T>();
        }
        return t;
    }
    #endregion

    #region SetLayer 设置当前gameObject及所有子物体的层
    /// <summary>
    /// 设置当前gameObject及所有子物体的层
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="layerName">层的名称</param>
    public static void SetLayer(this GameObject obj, string layerName)
    {
        Transform[] transArr = obj.transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transArr.Length; i++)
        {
            transArr[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
    #endregion

    #region AutoLoadTexture 自动加载图片
    /// <summary>
    /// 自动加载图片
    /// </summary>
    public static async void AutoLoadTexture(this Image img, string imgPath, bool isSetNativeSize = false)
    {
        if (img != null && !string.IsNullOrEmpty(imgPath))
        {
            Object asset = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync<Object>(imgPath);
            if (asset == null) return;
            Sprite obj;
            if (asset is Sprite sprite)
            {
                obj = sprite;
            }
            else
            {
                Texture2D texture = (Texture2D)asset;
                obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            //img.overrideSprite = obj;
            img.sprite = obj;
            if (isSetNativeSize)
            {
                img.SetNativeSize();
            }
        }
    }
    public static async void AutoLoadTexture(this RawImage img, string imgPath, bool isSetNativeSize = false)
    {
        if (img != null && !string.IsNullOrEmpty(imgPath))
        {
            Object asset = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync<Object>("UI/UIRes/UITexture/" + imgPath);
            if (asset == null) return;
            if (asset is Texture2D) img.texture = (Texture2D)asset;
            if (isSetNativeSize) img.SetNativeSize();
        }
    }
    public static async void AutoLoadSprite(this Image img, string imgPath, bool isSetNativeSize = false)
    {
        var sprite = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync<Sprite>(imgPath);
        img.sprite = sprite;
        if (isSetNativeSize)
        {
            img.SetNativeSize();
        }
    }
    public static async void LoadTexture(string imgPath, Action<Texture2D> onComplete)
    {
        if (!string.IsNullOrEmpty(imgPath))
        {
            Object asset = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync<Object>(imgPath);
            if (asset == null) return;

            if (asset is Texture2D)
            {
                Texture2D texture = (Texture2D)asset;
                onComplete?.Invoke(texture);
            }
        }
    }
    #endregion

    /// <summary>
    /// 设置特效渲染层级
    /// </summary>
    public static void SetEffectOrder(this Transform trans, int sortingOrder)
    {
        Renderer[] renderers = trans.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++) renderers[i].sortingOrder = sortingOrder;
    }
}