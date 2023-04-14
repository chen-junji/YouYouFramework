using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestProcedure : MonoBehaviour
{
    void Update()
    {
		if (Input.GetKeyUp(KeyCode.A))
		{
			GameEntry.Procedure.ChangeState(ProcedureState.Game);
		}
	}
}