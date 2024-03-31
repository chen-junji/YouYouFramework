using UnityEngine;
using UnityEngine.UI;

//自动生成于：2024/3/31 19:39:45
	public partial class CircleForm
	{
		private RectTransform m_Trans_Circle;

		protected override void GetBindComponents(GameObject go)
		{
			base.GetBindComponents(go);
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Trans_Circle = autoBindTool.GetBindComponent<RectTransform>(0);
		}
	}
