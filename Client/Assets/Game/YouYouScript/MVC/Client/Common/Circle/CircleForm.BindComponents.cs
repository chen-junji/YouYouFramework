using UnityEngine;
using UnityEngine.UI;

//自动生成于：2024/3/27 14:58:14
	public partial class CircleForm
	{
		private RectTransform m_Trans_Circle;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Trans_Circle = autoBindTool.GetBindComponent<RectTransform>(0);
		}
	}
