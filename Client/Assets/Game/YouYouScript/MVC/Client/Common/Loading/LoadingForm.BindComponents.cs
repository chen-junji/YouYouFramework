using UnityEngine;
using UnityEngine.UI;

//自动生成于：2024/3/27 15:28:22
	public partial class LoadingForm
	{
		private Scrollbar m_Sbar_Progress;
		private Text m_Txt_Tip;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Sbar_Progress = autoBindTool.GetBindComponent<Scrollbar>(0);
			m_Txt_Tip = autoBindTool.GetBindComponent<Text>(1);
		}
	}
