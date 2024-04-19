using UnityEngine;
using UnityEngine.UI;

//自动生成于：2024/4/19 7:52:09
	public partial class MainForm
	{
		private RectTransform m_Trans_BtnGroup;

		protected override void GetBindComponents(GameObject go)
		{
			base.GetBindComponents(go);
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Trans_BtnGroup = autoBindTool.GetBindComponent<RectTransform>(0);
		}
	}
