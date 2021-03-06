using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestAnimation : MonoBehaviour
{
	private RoleCtrl testRoleCtrl;
	void Update()
	{
		if (Input.GetKeyUp(KeyCode.M))
		{
			GameEntry.Data.RoleDataManager.CreatePlayer("zhujiao_cike_animation", "Assets/Download/Role/RoleSources/cike/zhujiao_cike_animation.FBX", (RoleCtrl obj) =>
			{
				testRoleCtrl = obj;
			});

		}
		if (Input.GetKeyDown(KeyCode.J))
		{
			testRoleCtrl.PlayAnim("Skill6");
			//float animLen = m_RoleAnimInfoDic[GetRoleAnimInfoId("Skill6")].CurrPlayable.GetAnimationClip().length;//动画的长度
			//Debug.LogError(animLen);
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			testRoleCtrl.PlayAnim("Run");
		}
	}
}
