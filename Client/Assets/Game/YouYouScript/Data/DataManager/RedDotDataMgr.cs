using Main;
using Sirenix.OdinInspector.Editor.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;
using YouYou;

public class RedDotDataMgr : DataMgrBase<RedDotDataMgr.EventName>
{
    public enum EventName : uint
    {
        //红点获取回调
        E_SVR_MSG_ID_GET_GLOBAL_RED,
        //红点清除回调
        E_SVR_MSG_ID_CLEAR_GLOBAL_RED,
    }

    public void Init()
    {

    }

    private List<int> sVecRedModelsGet = new List<int>();
    private Dictionary<int, string> redModelsGetParams = new Dictionary<int, string>();
    private object sGetRedScheduler = null;
    public void SendTGetGlobalRedReq(List<int> vecRedModels)
    {
        if (vecRedModels == null || vecRedModels.Count == 0)
        {
            GameEntry.LogError(LogCategory.NetWork, "请求的红点列表为空");
            return;
        }

        //去重
        for (int i = 0; i < vecRedModels.Count; i++)
        {
            bool bFound = false;
            int tGlobalRedInfoNew = vecRedModels[i];
            for (int j = 0; j < sVecRedModelsGet.Count; j++)
            {
                if (sVecRedModelsGet.Contains(tGlobalRedInfoNew))
                {
                    bFound = true;
                    break;
                }
            }

            if (!bFound)
            {
                sVecRedModelsGet.Add(tGlobalRedInfoNew);
            }
        }

        if (sGetRedScheduler == null)
        {
            sGetRedScheduler = GameEntry.Time.CreateTimer(GameEntry.Instance, 0.3f, () =>
            {
                TGetGlobalRedReq req = new TGetGlobalRedReq();
                req.vectRedModels = sVecRedModelsGet;
                req.mapExtraData = redModelsGetParams;

                //这里请求后端
                //MsgManager.sendData(E_SVR_MSG_ID.E_SVR_MSG_ID_GET_GLOBAL_RED, req);

                sVecRedModelsGet.Clear();
                redModelsGetParams.Clear();
                sGetRedScheduler = null;
            }, true);
        }
    }

    public void SetRedDotGetParam(int redDotType, string reqParam)
    {
        if (redModelsGetParams.ContainsKey(redDotType))
            redModelsGetParams[redDotType] = reqParam;
        else
            redModelsGetParams.Add(redDotType, reqParam);
    }

    private List<int> sVecRedModelsCleared = new List<int>();
    private object sClearRedScheduler = null;
    public void SendTClearGlobalRedReq(List<int> vecRedModels)
    {
        if (vecRedModels.Count == 0)
        {
            return;
        }

        //去重
        for (int i = 0; i < vecRedModels.Count; i++)
        {
            bool bFound = false;
            int tGlobalRedInfoNew = vecRedModels[i];
            for (int j = 0; j < sVecRedModelsCleared.Count; j++)
            {
                if (sVecRedModelsCleared.Contains(tGlobalRedInfoNew))
                {
                    bFound = true;
                    break;
                }
            }

            if (!bFound)
            {
                sVecRedModelsCleared.Add(tGlobalRedInfoNew);
            }
        }

        if (sClearRedScheduler == null)
        {
            sClearRedScheduler = GameEntry.Time.CreateTimer(GameEntry.Instance.gameObject, 0.3f, () =>
            {
                TClearGlobalRedReq req = new TClearGlobalRedReq();
                req.vecRedModel = sVecRedModelsCleared;

                //请求后端
                //MsgManager.sendData(E_SVR_MSG_ID.E_SVR_MSG_ID_CLEAR_GLOBAL_RED, req);

                sVecRedModelsCleared.Clear();
                sClearRedScheduler = null;
            }, true);
        }
    }


    /// <summary>
    /// 后台下发的红点数据
    /// </summary>
    private Dictionary<int, int> mMapGlobalRedInfo = new Dictionary<int, int>();

    public bool IsGlobalRedInfoExist()
    {
        return mMapGlobalRedInfo.Count != 0;
    }

    public int GetTGlobalRedInfo(int redDotType)
    {
        int TGlobalRedInfo = 0;
        if (mMapGlobalRedInfo.ContainsKey(redDotType))
        {
            TGlobalRedInfo = mMapGlobalRedInfo[redDotType];
        }
        return TGlobalRedInfo;
    }

    public void ParseTGetGlobalRedRsp(TGetGlobalRedRsp rsp)
    {
        if (rsp.iRet != 0)
        {
            GameEntry.LogError(LogCategory.NetWork, "拉取小红点信息出错");
            return;
        }

        foreach (int key in rsp.mapRedModelResults.Keys)
        {
            mMapGlobalRedInfo.Add(key, rsp.mapRedModelResults[key]);
        }
        Dispatch(EventName.E_SVR_MSG_ID_GET_GLOBAL_RED, rsp.mapRedModelResults);
    }

    public void ParseTClearGlobalRedRsp(TClearGlobalRedRsp rsp)
    {
        if (rsp.iRet != 0)
        {
            GameEntry.LogError(LogCategory.UI, rsp.iRet + "==清除小红点信息出错");
            return;
        }

        for (int j = 0; j < rsp.vecRedModel.Count; j++)
        {
            int tGlobalRedInfo = rsp.vecRedModel[j];
            if (mMapGlobalRedInfo.ContainsKey(tGlobalRedInfo))
            {
                mMapGlobalRedInfo[tGlobalRedInfo] = 0;
            }
        }
        Dispatch(EventName.E_SVR_MSG_ID_CLEAR_GLOBAL_RED, rsp.vecRedModel);
    }

}
