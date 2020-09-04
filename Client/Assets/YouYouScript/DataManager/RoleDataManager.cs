using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;
using System;

public class RoleDataManager : IDisposable
{
	private LinkedList<RoleCtrl> m_RoleList;
	public RoleDataManager()
	{
		m_RoleList = new LinkedList<RoleCtrl>();
	}

	public void CreatePlayerByJobId(int jobId, Action<RoleCtrl> onComplete = null)
	{
		//皮肤编号
		int skinId = GameEntry.DataTable.JobDBModel.GetDic(jobId).SkinId;

		//加载角色控制器
		GameEntry.Pool.GameObjectSpawn(SysPrefabId.RoleCtrl, (Transform trans, bool isNewInstance) =>
		 {
			 RoleCtrl roleCtrl = trans.GetComponent<RoleCtrl>();
			 roleCtrl.Init(skinId);

			 if (!isNewInstance)
			 {
				 //如果不是新实例 在这里执行OnOpen方法
				 roleCtrl.OnOpen();
			 }
			 m_RoleList.AddLast(roleCtrl);
			 onComplete?.Invoke(roleCtrl);
		 });
	}
	public void DespawnRole(RoleCtrl roleCtrl)
	{
		//先执行角色回池的方法 把角色依赖的其他零件回池
		roleCtrl.OnClose();

		//然后回池角色
		GameEntry.Pool.GameObjectDespawn(roleCtrl.transform);
		m_RoleList.Remove(roleCtrl);
	}

	public void DespawnAllRole()
	{
		for (LinkedListNode<RoleCtrl> curr = m_RoleList.First; curr != null;)
		{
			LinkedListNode<RoleCtrl> next = curr.Next;
			DespawnRole(curr.Value);
			curr = next;
		}
	}

	public void Dispose()
	{

	}
}
