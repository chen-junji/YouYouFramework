using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class RoleCtrl : BaseSprite
{
	public override void OnOpen()
	{
		base.OnOpen();
		LoadSkin();
	}
	public override void OnClose()
	{
		base.OnClose();
		UnLoadSkin();
	}

	public void Init(int skinId)
	{
		m_SkinId = skinId;
	}

	#region ½ÇÉ«Æ¤·ô

	private int m_SkinId;

	private Transform m_CurrSkinTransform;
	private RoleSkinComponent m_CurrRoleSkinComponent;
	private SkinnedMeshRenderer m_CurrSkinnedMeshRenderer;
	private void ChangeSkin(int skinId)
	{
		if (m_SkinId == skinId)
		{
			return;
		}
		m_SkinId = skinId;
		LoadSkin();
	}
	private void LoadSkin()
	{
		UnLoadSkin();
		//¼ÓÔØ ½ÇÉ«Æ¤·ô
		GameEntry.Pool.GameObjectSpawn(m_SkinId, (Transform trans, bool isNewInstance) =>
		{
			m_CurrSkinTransform = trans;
			m_CurrSkinTransform.SetParent(transform);
			m_CurrSkinTransform.localPosition = Vector3.zero;

			m_CurrRoleSkinComponent = m_CurrSkinTransform.GetComponent<RoleSkinComponent>();
			if (m_CurrRoleSkinComponent == null)
				m_CurrSkinnedMeshRenderer = m_CurrSkinTransform.GetComponentInChildren<SkinnedMeshRenderer>();
		});
	}
	private void LoadSkinMaterial(string materialName)
	{
		if (m_CurrSkinnedMeshRenderer == null) return;
		GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(AssetCategory.RoleSources, materialName, (entity) =>
		{
			Material material = entity.Target as Material;

#if UNITY_EDITOR
			m_CurrSkinnedMeshRenderer.material = material;
#else
			m_CurrSkinnedMeshRenderer.sharedMaterial = material;
#endif
		});
	}
	private void UnLoadSkin()
	{
		if (m_CurrSkinTransform != null)
		{
			GameEntry.Pool.GameObjectDespawn(m_CurrSkinTransform);
			m_CurrSkinTransform = null;
		}
		m_CurrSkinnedMeshRenderer = null;
	}
	#endregion
}
