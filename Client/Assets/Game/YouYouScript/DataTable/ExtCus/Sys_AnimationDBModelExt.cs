using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Sys_AnimationDBModel
{
	private Dictionary<int, List<Sys_AnimationEntity>> retDic;

	private void InitByGroupDic()
	{
		retDic = new Dictionary<int, List<Sys_AnimationEntity>>();
		int len = m_List.Count;
		for (int i = 0; i < len; i++)
		{
			int groupId = m_List[i].GroupId;
			if (!retDic.ContainsKey(groupId))
			{
				retDic.Add(groupId, new List<Sys_AnimationEntity>());

			}
			retDic[groupId].Add(m_List[i]);
		}
	}

	public List<Sys_AnimationEntity> GetListByGroupId(int groupId)
	{
		if (retDic == null) InitByGroupDic();

		List<Sys_AnimationEntity> lst;
		retDic.TryGetValue(groupId, out lst);
		return lst;
	}
}
