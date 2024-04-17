using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class objectUtil
{
    public static int ToInt(this object value, int defaultValue = 0)
    {
        int temp = defaultValue;
        if (value != null) int.TryParse(value.ToString(), out temp);
        return temp;
    }
    public static float ToFloat(this object value, float defaultValue = 0)
    {
        float temp = defaultValue;
        if (value != null) float.TryParse(value.ToString(), out temp);
        return temp;
    }

    public static bool CustomContains<T>(this IList<T> list, T t) where T : class
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == t) return true;
        }
        return false;
    }
}
