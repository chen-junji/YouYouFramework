using UnityEngine;
using UnityEngine.UI;

//自动生成于：2024/3/31 19:40:41
	public partial class LoadingForm
	{
		private Scrollbar m_Sbar_Progress;
		private Text m_Txt_Tip;

		protected override void GetBindComponents(GameObject go)
		{
			base.GetBindComponents(go);
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Sbar_Progress = autoBindTool.GetBindComponent<Scrollbar>(0);
			m_Txt_Tip = autoBindTool.GetBindComponent<Text>(1);
		}
	}
