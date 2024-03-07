using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;
using System;

public class RedDotServerSend : MonoBehaviour
{
    [Serializable]
    public class RedDotData
    {
        [Header("红点类型（枚举 emum RedDotType）")]
        public int redDotType;
        public string description;

        [Header("可选：红点清除的时机（没选则不在这里清除红点）")]
        public bool clearOnStart = false;
        public bool clearOnEnable = false;
        public bool clearOnDisable = false;
        public bool clearOnDestroy = false;
        public Button clearOnButton;
        public Toggle clearOnToggle;

        [Header("可选：红点清除的必要条件1(以下对象都必须不可见)")]
        public GameObject[] needObjDisabled;

        [Header("可选：红点清除的必要条件2(以下对象都必须可见)")]
        public GameObject[] needObjEnabled;
    }
    [Header("红点类型列表（或逻辑）")]
    public List<RedDotData> redDots;

    [Header("可选：需要请求后台刷新的红点类型列表和时机")]
    public bool requestOnStart = false;
    public bool requestOnEnable = false;
    public bool requestOnDisable = false;
    public bool requestOnDestroy = false;
    public List<int> requestRedDotTypes;

    private bool enableDisableCalled = false;


    public void Start()
    {
        StartCoroutine(Start_Enumator());
    }

    public IEnumerator Start_Enumator()
    {
        if (requestOnStart)
        {
            RequestRedDotType();
        }

        WaitForSeconds wfs = new WaitForSeconds(0.1f);
        while (GameEntry.Model.GetModel<RedDotModel>().IsGlobalRedInfoExist() == false)
        {
            yield return wfs;
        }

        InitButtonToggleClear();

        ClearRedDotOnStart();
    }

    public void RequestRedDotType()
    {
        List<int> vecRedModels = new List<int>();
        for (int i = 0; i < requestRedDotTypes.Count; i++)
        {
            int redDotType = requestRedDotTypes[i];
            if (redDotType > 0)
            {
                vecRedModels.Add(redDotType);
            }
        }

        if (vecRedModels.Count > 0)
        {
            RedDotCtrl.Instance.SendTGetGlobalRedReq(vecRedModels);
        }
    }

    public void OnEnable()
    {
        StartCoroutine(OnEnable_Enumator());
    }

    public IEnumerator OnEnable_Enumator()
    {
        if (requestOnEnable && enableDisableCalled)
        {
            RequestRedDotType();
        }
        enableDisableCalled = true;

        WaitForSeconds wfs = new WaitForSeconds(0.1f);
        while (GameEntry.Model.GetModel<RedDotModel>().IsGlobalRedInfoExist() == false)
        {
            yield return wfs;
        }

        ClearRedDotOnEnable();
    }

    public void OnDisable()
    {
        ClearRedDotOnDisable();

        if (requestOnDisable && enableDisableCalled)
        {
            RequestRedDotType();
        }
        enableDisableCalled = true;
    }

    public void OnDestroy()
    {
        ClearRedDotOnDestroy();

        if (requestOnDestroy)
        {
            RequestRedDotType();
        }
    }

    public void InitButtonToggleClear()
    {
        Dictionary<Button, List<int>> mapButtonRedDotTypes = new Dictionary<Button, List<int>>();
        Dictionary<Toggle, List<int>> mapToggleRedDotTypes = new Dictionary<Toggle, List<int>>();

        //找到Button和Toggle相关的所有RedDotType
        for (int i = 0; i < redDots.Count; i++)
        {
            RedDotData redDotData = redDots[i];
            bool bClearable = IsClearable(redDotData);
            if (redDotData.clearOnButton != null && bClearable)
            {
                if (!mapButtonRedDotTypes.ContainsKey(redDotData.clearOnButton))
                {
                    mapButtonRedDotTypes.Add(redDotData.clearOnButton, new List<int>());
                }

                mapButtonRedDotTypes[redDotData.clearOnButton].Add(redDotData.redDotType);
            }
            if (redDotData.clearOnToggle != null && bClearable)
            {
                if (!mapToggleRedDotTypes.ContainsKey(redDotData.clearOnToggle))
                {
                    mapToggleRedDotTypes.Add(redDotData.clearOnToggle, new List<int>());
                }

                mapToggleRedDotTypes[redDotData.clearOnToggle].Add(redDotData.redDotType);
            }
        }

        //为Button注册事件，清理指定RedDotType
        foreach (var key in mapButtonRedDotTypes.Keys)
        {
            Button button = key;
            List<int> redDotTypes = mapButtonRedDotTypes[key];

            button.onClick.AddListener(() =>
            {
                List<int> vecRedModelsServer = new List<int>();
                for (int i = 0; i < redDotTypes.Count; i++)
                {
                    int tGlobalRedInfo = GameEntry.Model.GetModel<RedDotModel>().GetTGlobalRedInfo(redDotTypes[i]);
                    if (tGlobalRedInfo > 0)
                    {
                        vecRedModelsServer.Add(redDotTypes[i]);
                    }
                }

                RedDotCtrl.Instance.SendTClearGlobalRedReq(vecRedModelsServer);
            });
        }

        //为Toggle注册事件，清理指定RedDotType
        foreach (var key in mapToggleRedDotTypes.Keys)
        {
            Toggle toggle = key;
            List<int> redDotTypes = mapToggleRedDotTypes[key];

            toggle.onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn)
                {
                    List<int> vecRedModelsServer = new List<int>();
                    for (int i = 0; i < redDotTypes.Count; i++)
                    {
                        int tGlobalRedInfo = GameEntry.Model.GetModel<RedDotModel>().GetTGlobalRedInfo(redDotTypes[i]);
                        if (tGlobalRedInfo > 0)
                        {
                            vecRedModelsServer.Add(redDotTypes[i]);
                        }
                    }

                    RedDotCtrl.Instance.SendTClearGlobalRedReq(vecRedModelsServer);
                }
            });
        }
    }

    public void ClearRedDotOnStart()
    {
        List<int> vecRedModelsServer = new List<int>();

        for (int i = 0; i < redDots.Count; i++)
        {
            RedDotData redDotData = redDots[i];
            if (redDotData.clearOnStart && IsClearable(redDotData))
            {
                int tGlobalRedInfo = GameEntry.Model.GetModel<RedDotModel>().GetTGlobalRedInfo(redDotData.redDotType);
                if (tGlobalRedInfo > 0)
                {
                    vecRedModelsServer.Add(redDotData.redDotType);
                }
            }
        }

        if (vecRedModelsServer.Count > 0)
        {
            RedDotCtrl.Instance.SendTClearGlobalRedReq(vecRedModelsServer);
        }
    }

    public void ClearRedDotOnEnable()
    {
        List<int> vecRedModelsServer = new List<int>();

        for (int i = 0; i < redDots.Count; i++)
        {
            RedDotData redDotData = redDots[i];
            if (redDotData.clearOnEnable && enableDisableCalled && IsClearable(redDotData))
            {
                int tGlobalRedInfo = GameEntry.Model.GetModel<RedDotModel>().GetTGlobalRedInfo(redDotData.redDotType);
                if (tGlobalRedInfo > 0)
                {
                    vecRedModelsServer.Add(redDotData.redDotType);
                }
            }
        }

        if (vecRedModelsServer.Count > 0)
        {
            RedDotCtrl.Instance.SendTClearGlobalRedReq(vecRedModelsServer);
        }
    }

    public void ClearRedDotOnDisable()
    {
        List<int> vecRedModelsServer = new List<int>();

        for (int i = 0; i < redDots.Count; i++)
        {
            RedDotData redDotData = redDots[i];
            if (redDotData.clearOnDisable && enableDisableCalled && IsClearable(redDotData))
            {
                int tGlobalRedInfo = GameEntry.Model.GetModel<RedDotModel>().GetTGlobalRedInfo(redDotData.redDotType);
                if (tGlobalRedInfo > 0)
                {
                    vecRedModelsServer.Add(redDotData.redDotType);
                }
            }
        }

        if (vecRedModelsServer.Count > 0)
        {
            RedDotCtrl.Instance.SendTClearGlobalRedReq(vecRedModelsServer);
        }
    }

    public void ClearRedDotOnDestroy()
    {
        List<int> vecRedModelsServer = new List<int>();

        for (int i = 0; i < redDots.Count; i++)
        {
            RedDotData redDotData = redDots[i];
            if (redDotData.clearOnDestroy && IsClearable(redDotData))
            {
                int tGlobalRedInfo = GameEntry.Model.GetModel<RedDotModel>().GetTGlobalRedInfo(redDotData.redDotType);
                if (tGlobalRedInfo > 0)
                {
                    vecRedModelsServer.Add(redDotData.redDotType);
                }
            }
        }

        if (vecRedModelsServer.Count > 0)
        {
            RedDotCtrl.Instance.SendTClearGlobalRedReq(vecRedModelsServer);
        }
    }

    public bool IsClearable(RedDotData redDotData)
    {
        bool bCleared = true;

        if (redDotData.needObjDisabled != null)
        {
            for (int i = 0; i < redDotData.needObjDisabled.Length; i++)
            {
                GameObject gameObject = redDotData.needObjDisabled[i];
                if (gameObject != null)
                {
                    if (gameObject.activeSelf)
                    {
                        bCleared = false;
                        break;
                    }
                }
            }
        }

        if (bCleared && redDotData.needObjEnabled != null)
        {
            for (int i = 0; i < redDotData.needObjEnabled.Length; i++)
            {
                GameObject gameObject = redDotData.needObjEnabled[i];
                if (gameObject != null)
                {
                    if (!gameObject.activeSelf)
                    {
                        bCleared = false;
                        break;
                    }
                }
            }
        }

        return bCleared;
    }
}
