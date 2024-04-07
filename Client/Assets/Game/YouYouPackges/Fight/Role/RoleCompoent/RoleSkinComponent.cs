using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

/// <summary>
/// 角色皮肤组件
/// </summary>
public class RoleSkinComponent : MonoBehaviour
{
    /// <summary>
    /// 皮肤部件配置
    /// </summary>
    [Serializable]
    public class SkinnedMeshRendererPart
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int No;

        /// <summary>
        /// 皮肤部件
        /// </summary>
        public SkinnedMeshRenderer Part;
    }

    /// <summary>
    /// 根骨骼
    /// </summary>
    [SerializeField]
    private Transform m_RootBone;

    /// <summary>
    /// 角色身上的SkinnedMeshRenderer 一个角色只有一个SkinnedMeshRenderer
    /// </summary>
    [SerializeField]
    private SkinnedMeshRenderer m_CurrSkinnedMeshRenderer;

    /// <summary>
    /// 部件
    /// </summary>
    [SerializeField]
    private SkinnedMeshRendererPart[] Parts;
    private int m_PartsLen;

    /// <summary>
    /// 角色当前穿戴的部件列表
    /// </summary>
    private List<SkinnedMeshRenderer> m_CurrPartList;

    /// <summary>
    /// 合并网格对象集合
    /// </summary>
    private List<CombineInstance> m_CombineInstances;

    /// <summary>
    /// 材质集合
    /// </summary>
    private List<Material> m_Materials;

    /// <summary>
    /// SkinnedMeshRenderer对应的骨骼信息
    /// </summary>
    private List<Transform> m_Bones;

    /// <summary>
    /// 角色身上的骨骼数组
    /// </summary>
    private Transform[] m_BoneTransforms;

    /// <summary>
    /// 当前的皮肤
    /// </summary>
    public GameObject CurrSkin { get; private set; }

    private void Awake()
    {
        m_CurrPartList = new List<SkinnedMeshRenderer>();
        m_CombineInstances = new List<CombineInstance>();
        m_Materials = new List<Material>();
        m_Bones = new List<Transform>();
    }
    private void Start()
    {
        m_PartsLen = Parts.Length;

        m_BoneTransforms = m_RootBone.GetComponentsInChildren<Transform>();
    }
    private void OnDestroy()
    {
        UnLoadSkin();
    }

    /// <summary>
    /// 加载皮肤预设
    /// </summary>
    public void LoadSkin(string skinName)
    {
        //先把当前皮肤卸载
        UnLoadSkin();

        //加载皮肤
        CurrSkin = GameEntry.Pool.GameObjectPool.Spawn(skinName);
        CurrSkin.transform.SetParent(transform, false);

        m_CurrSkinnedMeshRenderer = CurrSkin.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    /// <summary>
    /// 加载皮肤材质
    /// </summary>
    public void LoadSkinMaterial(string materialPath)
    {
        if (m_CurrSkinnedMeshRenderer == null) return;
#if UNITY_EDITOR
        m_CurrSkinnedMeshRenderer.material = GameEntry.Loader.LoadMainAsset<Material>(materialPath, m_CurrSkinnedMeshRenderer.gameObject);
#else
        m_CurrSkinnedMeshRenderer.sharedMaterial = GameEntry.Loader.LoadMainAsset<Material>(materialPath, m_CurrSkinnedMeshRenderer.gameObject);
#endif
    }

    /// <summary>
    /// 加载部件
    /// </summary>
    public void LoadPart(List<int> parts)
    {
        m_CombineInstances.Clear();
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

            //添加合并网格
            for (int sub = 0; sub < skinnedMeshRenderer.sharedMesh.subMeshCount; sub++)
            {
                CombineInstance ci = new CombineInstance();
                ci.mesh = skinnedMeshRenderer.sharedMesh;
                ci.subMeshIndex = sub;
                m_CombineInstances.Add(ci);
            }

            //======================此处是加载SkinnedMeshRenderer对应的骨骼 也就是bones的数量实际上会大于玩家身上的骨骼数量============================
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

        m_CurrSkinnedMeshRenderer.sharedMesh.CombineMeshes(m_CombineInstances.ToArray(), false, false); //合并模型
        m_CurrSkinnedMeshRenderer.bones = m_Bones.ToArray(); //赋予骨骼
        m_CurrSkinnedMeshRenderer.materials = m_Materials.ToArray(); //赋予材质
    }
    /// <summary>
    /// 根据编号获取部件
    /// </summary>
    private SkinnedMeshRenderer GetPartByNo(int no)
    {
        for (int i = 0; i < m_PartsLen; i++)
        {
            SkinnedMeshRendererPart skinnedMeshRendererPart = Parts[i];
            if (skinnedMeshRendererPart.No == no)
            {
                return skinnedMeshRendererPart.Part;
            }
        }
        return null;
    }

    private void UnLoadSkin()
    {
        if (CurrSkin != null)
        {
            GameEntry.Pool.GameObjectPool.Despawn(CurrSkin);
            CurrSkin = null;
        }
    }

}
