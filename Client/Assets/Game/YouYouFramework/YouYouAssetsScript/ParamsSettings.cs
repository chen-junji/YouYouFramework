using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

[CreateAssetMenu]
public class ParamsSettings : ScriptableObject
{
	[BoxGroup("InitUrl")] public string WebAccountUrl;
	[BoxGroup("InitUrl")] public string TestWebAccountUrl;
	[BoxGroup("InitUrl")] public bool IsTest;
	[BoxGroup("InitUrl")] public bool PostIsEncrypt;//是否加密(如时间戳)
	[BoxGroup("InitUrl")] public string PostContentType;//设置ContentType



	[BoxGroup("GeneralParams")]
	[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
	[HideLabel]
	public GeneralParamData[] GeneralParams;

	[BoxGroup("GradeParams")]
	[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
	[HideLabel]
	public GradeParamData[] GradeParams;

	/// <summary>
	/// 常规数据
	/// </summary>
	[Serializable]
	public class GeneralParamData
	{
		[TableColumnWidth(260, Resizable = false)]
		/// <summary>
		/// 参数Key
		/// </summary>
		public string Key;

		/// <summary>
		/// 参数值
		/// </summary>
		public int Value;
	}

	/// <summary>
	/// 设备等级
	/// </summary>
	public enum DeviceGrade
	{
		Low = 0,
		Middle = 1,
		High = 2
	}

	/// <summary>
	/// 等级参数数据
	/// </summary>
	[Serializable]
	public class GradeParamData
	{
		[TableColumnWidth(260, Resizable = false)]
		/// <summary>
		/// 参数Key
		/// </summary>
		public string Key;

		/// <summary>
		/// 低配值
		/// </summary>
		public int LowValue;

		/// <summary>
		/// 中配值
		/// </summary>
		public int MiddleValue;

		/// <summary>
		/// 高配值
		/// </summary>
		public int HighValue;

		/// <summary>
		/// 获取参数值
		/// </summary>
		/// <param name="grade"></param>
		/// <returns></returns>
		public int GetValueByGrade(DeviceGrade grade)
		{
			switch (grade)
			{
				default:
				case DeviceGrade.Low:
					return LowValue;
				case DeviceGrade.Middle:
					return MiddleValue;
				case DeviceGrade.High:
					return HighValue;
			}
		}
	}

	private int m_LenGradeParams = 0;

	/// <summary>
	/// 根据key和设备等级获取参数
	/// </summary>
	/// <param name="key"></param>
	/// <param name="grade"></param>
	/// <returns></returns>
	public int GetGradeParamData(string key, DeviceGrade grade)
	{
		m_LenGradeParams = GradeParams.Length;
		for (int i = 0; i < m_LenGradeParams; i++)
		{
			GradeParamData gradeParamData = GradeParams[i];
			if (gradeParamData.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
			{
				return gradeParamData.GetValueByGrade(grade);
			}
		}

		GameEntry.LogError("GetGradeParamData Fail key={0}", key);
		return 0;
	}
}