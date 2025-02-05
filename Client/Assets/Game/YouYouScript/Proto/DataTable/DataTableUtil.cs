using System;
using System.Collections.Generic;
using System.Numerics;

public static class DataTableUtil
{
    /// <summary>
    /// 添加获得的道具奖励 --Reawrds的数据都要统一为 "id,count|" 格式
    /// </summary>
    public static int[][] RewardItems(string rewardItems)
    {
        var reawrds = rewardItems.Split('|');
        int[][] retValues = new int[reawrds.Length][];
        for (int i = 0; i < reawrds.Length; i++)
        {
            retValues[i] = GetRewardItem(reawrds[i]);
        }
        return retValues;
    }
    /// <summary>
    /// 添加获得的道具奖励 --Reawrd的数据都要统一为 "id,count" 格式
    /// </summary>
    public static int[] GetRewardItem(string rewardItem)
    {
        string[] stringArray = rewardItem.Split(",");
        int[] intArray = Array.ConvertAll(stringArray, s => int.Parse(s));
        return intArray;
    }

}