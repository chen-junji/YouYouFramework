//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestProcedure : MonoBehaviour
{
    void Start()
    {
		
    }

    void Update()
    {
		//if (Input.GetKeyUp(KeyCode.B))
		//{
		//    GameEntry.UI.OpenUIForm(UIFormId.UI_Task);
		//}
		if (Input.GetKeyUp(KeyCode.C))
		{
			GameEntry.Procedure.ChangeState(ProcedureState.CheckVersion);
		}
	}
}