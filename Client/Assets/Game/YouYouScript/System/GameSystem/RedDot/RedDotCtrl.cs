using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class RedDotCtrl : Singleton<RedDotCtrl>
{
    public enum TestEnum
    {
        E_SVR_MSG_ID_GET_GLOBAL_RED,
        E_SVR_MSG_ID_CLEAR_GLOBAL_RED
    }
    public RedDotCtrl()
    {
        //这里改成你自己监听后端回调的代码
        GameEntry.Event.Common.AddEventListener((int)TestEnum.E_SVR_MSG_ID_GET_GLOBAL_RED, (x) => ParseTGetGlobalRedRsp(null));
        GameEntry.Event.Common.AddEventListener((int)TestEnum.E_SVR_MSG_ID_CLEAR_GLOBAL_RED, (x) => ParseTClearGlobalRedRsp(null));
    }

    //请求小红点
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

                //这里改成你自己请求后端的代码
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
    private void ParseTGetGlobalRedRsp(TGetGlobalRedRsp rsp)
    {
        if (rsp.iRet != 0)
        {
            GameEntry.LogError(LogCategory.NetWork, "拉取小红点信息出错");
            return;
        }
        GameEntry.Model.GetModel<RedDotModel>().SetTGetGlobalRedRsp(rsp);
    }

    //清除小红点
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

                //这里改成你自己请求后端的代码
                //MsgManager.sendData(E_SVR_MSG_ID.E_SVR_MSG_ID_CLEAR_GLOBAL_RED, req);

                sVecRedModelsCleared.Clear();
                sClearRedScheduler = null;
            }, true);
        }
    }
    private void ParseTClearGlobalRedRsp(TClearGlobalRedRsp rsp)
    {
        if (rsp.iRet != 0)
        {
            GameEntry.LogError(LogCategory.NetWork, "清除小红点信息出错");
            return;
        }
        GameEntry.Model.GetModel<RedDotModel>().SetTClearGlobalRedRsp(rsp);
    }

}