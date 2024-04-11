using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;


public class RedDotModel
{
    //红点获取
    public Action<Dictionary<int, int>> GetRedDotAction;

    //红点清除
    public Action<List<int>> ClearRedDotAction;

    /// <summary>
    /// 后台下发的红点数据
    /// </summary>
    private Dictionary<int, int> mapGlobalRedInfo = new Dictionary<int, int>();

    public bool IsGlobalRedInfoExist()
    {
        return mapGlobalRedInfo.Count != 0;
    }

    public int GetTGlobalRedInfo(int redDotType)
    {
        int TGlobalRedInfo = 0;
        if (mapGlobalRedInfo.ContainsKey(redDotType))
        {
            TGlobalRedInfo = mapGlobalRedInfo[redDotType];
        }
        return TGlobalRedInfo;
    }

    public void SetTGetGlobalRedRsp(TGetGlobalRedRsp rsp)
    {
        foreach (int key in rsp.mapRedModelResults.Keys)
        {
            mapGlobalRedInfo[key] = rsp.mapRedModelResults[key];
            ReddotManager.Instance.ChangeValue(ReddotManager.Instance.GetServerIdOfPath(key), rsp.mapRedModelResults[key]);
        }
        GetRedDotAction?.Invoke(rsp.mapRedModelResults);
    }

    public void SetTClearGlobalRedRsp(TClearGlobalRedRsp rsp)
    {
        for (int j = 0; j < rsp.vecRedModel.Count; j++)
        {
            int tGlobalRedInfo = rsp.vecRedModel[j];
            if (mapGlobalRedInfo.ContainsKey(tGlobalRedInfo))
            {
                mapGlobalRedInfo[tGlobalRedInfo] = 0;
                ReddotManager.Instance.ChangeValue(ReddotManager.Instance.GetServerIdOfPath(tGlobalRedInfo), 0);
            }
        }
        ClearRedDotAction?.Invoke(rsp.vecRedModel);
    }
}
