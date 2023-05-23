//using DG.Tweening;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using YouYou;

//public static class DoTweenUtil
//{
//    public static async ETTask<T> OnComplete<T>(this T t) where T : Tween
//    {
//        ETTask<T> task = ETTask<T>.Create();
//        t.OnComplete(() => task.SetResult(t));
//        return await task;
//    }
//    public static Tweener DoShowScale(this Transform target, float duration)
//    {
//        return target.DoShowScale(duration, 1);
//    }
//    public static Tweener DoShowScale(this Transform target, float duration, float targetScale)
//    {
//        target.DOKill();
//        target.localScale = Vector3.zero;
//        return target.DOScale(targetScale, duration).SetEase(Ease.OutBack);
//    }
//    public static Tweener DoShowScale(this Transform target, float duration, Vector3 targetScale)
//    {
//        target.DOKill();
//        target.localScale = Vector3.zero;
//        return target.DOScale(targetScale, duration).SetEase(Ease.OutBack);
//    }
//    public static Tweener DoShowColor(this Image target, float duration)
//    {
//        target.DOKill();
//        if (target == null) return null;
//        target.gameObject.SetActive(true);
//        Color color = target.color;
//        target.color = Color.clear;
//        return target.DOColor(color, duration);
//    }
//    public static Tweener DoShowColor(this Image target, float duration, Color targetColor)
//    {
//        target.DOKill();
//        if (target == null) return null;
//        target.gameObject.SetActive(true);
//        target.color = Color.clear;
//        return target.DOColor(targetColor, duration);
//    }
//    public static Tweener DoHideColor(this Image target, float duration, Color begColor)
//    {
//        target.DOKill();
//        if (target == null) return null;
//        target.color = begColor;
//        return target.DOColor(Color.clear, duration);
//    }
//}
