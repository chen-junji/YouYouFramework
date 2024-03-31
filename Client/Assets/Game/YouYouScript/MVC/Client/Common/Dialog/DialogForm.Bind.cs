using UnityEngine;
using UnityEngine.UI;

//自动生成于：2024/3/31 19:11:01
	public partial class DialogForm
	{
		private Text m_Txt_Title;
		private Text m_Txt_Message;
		private Button m_Btn_OK;
		private Button m_Btn_Cancel;

		protected override void GetBindComponents(GameObject go)
		{
			base.GetBindComponents(go);
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Txt_Title = autoBindTool.GetBindComponent<Text>(0);
			m_Txt_Message = autoBindTool.GetBindComponent<Text>(1);
			m_Btn_OK = autoBindTool.GetBindComponent<Button>(2);
			m_Btn_Cancel = autoBindTool.GetBindComponent<Button>(3);
		}
	}
