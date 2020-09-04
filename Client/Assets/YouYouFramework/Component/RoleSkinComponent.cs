using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ɫƤ�����
/// </summary>
public class RoleSkinComponent : MonoBehaviour
{
	/// <summary>
	/// �������ڵ�
	/// </summary>
	[SerializeField]
	private Transform m_RootBone;
	/// <summary>
	/// ��ǰ��ɫ���ڵ������, ֻ��һ��
	/// </summary>
	[SerializeField]
	private SkinnedMeshRenderer m_CurrSkinnedMeshRenderer;

	[SerializeField]
	private SkinnedMeshRendererPart[] Parts;
	private int m_PartsLen;


	/// <summary>
	/// ��ǰ�����Ĳ����б�
	/// </summary>
	private List<SkinnedMeshRenderer> m_CurrPartList;
	/// <summary>
	/// �ϲ�������󼯺�
	/// </summary>
	private List<CombineInstance> m_CombneInstances;
	private List<Material> m_Materials;
	/// <summary>
	/// SkinnedMeshRenderer��Ӧ�Ĺ�����Ϣ
	/// </summary>
	private List<Transform> m_Bones;
	/// <summary>
	/// ��ɫ���ϵĹ�������
	/// </summary>
	private Transform[] m_BoneTransforms;


	private void Awake()
	{
		m_CurrPartList = new List<SkinnedMeshRenderer>();
		m_CombneInstances = new List<CombineInstance>();
		m_Materials = new List<Material>();
		m_Bones = new List<Transform>();
	}
	private void OnDestroy()
	{
		m_CurrPartList.Clear();
		m_CurrPartList = null;

		m_CombneInstances.Clear();
		m_CombneInstances = null;

		m_Bones.Clear();
		m_Bones = null;

		m_RootBone = null;
		m_CurrSkinnedMeshRenderer = null;
	}
	private void Start()
	{
		m_PartsLen = Parts.Length;
		m_BoneTransforms = m_RootBone.GetComponentsInChildren<Transform>();//��ȡ��ɫ�͹���
		m_CurrSkinnedMeshRenderer.sharedMesh = new Mesh();
		LoadPart(new List<int> { 1, 2 });
	}


	private void LoadPart(List<int> parts)
	{
		m_CombneInstances.Clear();
		m_Materials.Clear();
		m_Bones.Clear();
		m_CurrPartList.Clear();

		int len = parts.Count;
		for (int i = 0; i < len; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = GetPartByNo(parts[i]);
			if (skinnedMeshRenderer != null)
			{
				m_CurrPartList.Add(skinnedMeshRenderer);
			}
		}
		for (int i = 0; i < m_CurrPartList.Count; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = m_CurrPartList[i];
			m_Materials.AddRange(skinnedMeshRenderer.materials);

			//��Ӻϲ�����
			for (int sub = 0; sub < skinnedMeshRenderer.sharedMesh.subMeshCount; sub++)
			{
				CombineInstance ci = new CombineInstance();
				ci.mesh = skinnedMeshRenderer.sharedMesh;
				ci.subMeshIndex = sub;
				m_CombneInstances.Add(ci);
			}

			//�˴��Ǽ���SkinnedMeshRenderer��Ӧ�Ĺ��� Ҳ����Bones������ ʵ���ϻ����������Ϲ���������
			for (int sub = 0; sub < skinnedMeshRenderer.bones.Length; sub++)
			{
				int lenBoneTrans = m_BoneTransforms.Length;
				for (int m = 0; m < lenBoneTrans; m++)
				{
					Transform trans = m_BoneTransforms[m];
					if (trans.name != skinnedMeshRenderer.bones[sub].name) continue;
					m_Bones.Add(trans);
					break;
				}
			}
		}

		m_CurrSkinnedMeshRenderer.sharedMesh.CombineMeshes(m_CombneInstances.ToArray(), false, false);//�ϲ�ģ��
		m_CurrSkinnedMeshRenderer.bones = m_Bones.ToArray();//�������
		m_CurrSkinnedMeshRenderer.materials = m_Materials.ToArray();//�������
	}
	private SkinnedMeshRenderer GetPartByNo(int no)
	{
		for (int i = 0; i < m_PartsLen; i++)
		{
			SkinnedMeshRendererPart skinnedMeshRendererPart = Parts[i];
			if (skinnedMeshRendererPart.No==no)
			{
				return skinnedMeshRendererPart.part;
			}
		}
		return null;
	}


	/// <summary>
	/// Ƥ������
	/// </summary>
	[Serializable]
	public class SkinnedMeshRendererPart
	{
		/// <summary>
		/// ���
		/// </summary>
		public int No;
		public SkinnedMeshRenderer part;
	}
}
