//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2016-05-10 23:13:56
//备    注：
//===================================================
using UnityEngine;
using System.Collections;

public class DeviceUtil
{
    /// <summary>
    /// 获取设备标识符
    /// </summary>
    public static string DeviceIdentifier
    {
        get
        {
            return SystemInfo.deviceUniqueIdentifier;
        }
    }

    /// <summary>
    /// 获取设备型号
    /// </summary>
    public static string DeviceModel
    {
        get
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return UnityEngine.iOS.Device.generation.ToString();
#else
            return SystemInfo.deviceModel;
#endif
        }
    }
}