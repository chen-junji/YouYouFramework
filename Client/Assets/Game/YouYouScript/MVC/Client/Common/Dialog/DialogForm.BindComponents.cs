using UnityEngine;
using UnityEngine.UI;

//自动生成于：2024/3/27 16:13:10
	public partial class DialogForm
	{
		private Text m_Txt_Title;
		private Text m_Txt_Message;
		private Button m_Btn_OK;
		private Button m_Btn_Cancel;

		private void GetBindComponents(GameObject go)
		{
			ComponentAutoBindTool autoBindTool = go.GetComponent<ComponentAutoBindTool>();

			m_Txt_Title = autoBindTool.GetBindComponent<Text>(0);
			m_Txt_Message = autoBindTool.GetBindComponent<Text>(1);
			m_Btn_OK = autoBindTool.GetBindComponent<Button>(2);
			m_Btn_Cancel = autoBindTool.GetBindComponent<Button>(3);
		}
	}
