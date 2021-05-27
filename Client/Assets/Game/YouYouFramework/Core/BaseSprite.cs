using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;
using YouYou;

public class BaseSprite : MonoBehaviour
{
	private void Awake()
	{
		OnAwake();
	}
	private void Update()
	{
		OnUpdate();
	}
	private void OnDestroy()
	{
		OnBeforDestroy();
	}

	protected virtual void OnAwake() { }
	/// <summary>
	/// 克隆时调用
	/// </summary>
	public virtual void OnInit() { }
	/// <summary>
	/// 从对象池取出时调用
	/// </summary>
	public virtual void OnOpen() { }
	/// <summary>
	/// 退回到对象池时调用
	/// </summary>
	public virtual void OnClose() { UnLoadSkin(); }
	/// <summary>
	/// 销毁时调用
	/// </summary>
	protected virtual void OnBeforDestroy()
	{
		if (m_PlayableGraph.IsValid()) m_PlayableGraph.Destroy();
	}
	protected virtual void OnUpdate() { }


	public void LoadSkin(Sys_PrefabEntity sys_PrefabEntity, BaseAction<Transform> onComplete)
	{

		UnLoadSkin();
		//加载 角色皮肤
		GameEntry.Pool.GameObjectSpawn(sys_PrefabEntity, transform, (Transform trans, bool isNewInstance) =>
		{
			m_CurrSkinTransform = trans;
			//m_CurrSkinTransform.SetParent(transform);
			//m_CurrSkinTransform.localPosition = Vector3.zero;

			//初始化皮肤组件
			m_CurrRoleSkinComponent = m_CurrSkinTransform.GetComponent<RoleSkinComponent>();

			if (m_CurrRoleSkinComponent == null)
				m_CurrSkinnedMeshRenderer = m_CurrSkinTransform.GetComponentInChildren<SkinnedMeshRenderer>();

			onComplete?.Invoke(m_CurrSkinTransform);
		});
	}

	#region 角色皮肤
	private Transform m_CurrSkinTransform;
	private RoleSkinComponent m_CurrRoleSkinComponent;
	private SkinnedMeshRenderer m_CurrSkinnedMeshRenderer;


	private void LoadSkinMaterial(string materialName)
	{
		if (m_CurrSkinnedMeshRenderer == null) return;
		GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(AssetCategory.Role, materialName, (Material material) =>
		{
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

	#region 角色动画
	public class RoleAnimInfo
	{
		public int inputPort;
		public string AnimClipName;
		public AnimationClipPlayable CurrPlayable;
		public Sys_AnimationEntity CurrRoleAnimationData;
		/// <summary>
		/// 最后使用时间
		/// </summary>
		public float LastUserTime;
		/// <summary>
		/// 是否已经加载
		/// </summary>
		public bool IsLoad;
		/// <summary>
		/// 是否正在播放
		/// </summary>
		public bool IsPlaying;
		/// <summary>
		/// 是否已经过期
		/// </summary>
		public bool IsExpire
		{
			get
			{
				if (!IsPlaying &&
					IsLoad &&
					CurrRoleAnimationData != null &&
					CurrRoleAnimationData.InitLoad == 0 &&
					Time.time > LastUserTime + CurrRoleAnimationData.Expire)
				{
					return true;
				}
				return false;
			}
		}
	}
	private Dictionary<int, RoleAnimInfo> m_RoleAnimInfoDic = new Dictionary<int, RoleAnimInfo>(m_AnimCount);
	private PlayableGraph m_PlayableGraph;
	private AnimationPlayableOutput m_AnimationPlayableOutput;
	private AnimationMixerPlayable m_AnimationMixerPlayable;
	private static int m_AnimCount = 100;//可以大于实际数量, 不能小于实际数量

	public RoleAnimInfo PlayAnim(string animName, BaseAction onComplete = null)
	{
		var enumerator = m_RoleAnimInfoDic.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.AnimClipName.Equals(animName))
			{
				return PlayAnim(enumerator.Current.Key, onComplete);
			}
		}
		onComplete?.Invoke();
		return null;
	}
	public RoleAnimInfo PlayAnim(int animId, BaseAction onComplete = null)
	{
		var enumerator = m_RoleAnimInfoDic.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.IsPlaying = false;
		}

		RoleAnimInfo roleAnimInfo = null;
		if (m_RoleAnimInfoDic.TryGetValue(animId, out roleAnimInfo))
		{
			roleAnimInfo.LastUserTime = Time.time;
			roleAnimInfo.IsPlaying = true;

			if (roleAnimInfo.IsLoad)
			{
				PlayAnimByInputPort(roleAnimInfo, onComplete);
			}
			else
			{
				//动画池中不存在, 加载动画
				LoadRoleAnimation(roleAnimInfo.CurrRoleAnimationData, (retRoleAnimInfo) =>
				{
					PlayAnimByInputPort(retRoleAnimInfo, onComplete);
				});
			}
		}
		else
		{
			onComplete?.Invoke();
		}
		return roleAnimInfo;
	}

	private void PlayAnimByInputPort(RoleAnimInfo roleAnimInfo, BaseAction onComplete)
	{
		m_PlayableGraph.Play();

		Playable playable = m_AnimationMixerPlayable.GetInput(roleAnimInfo.inputPort);
		playable.SetTime(0);
		playable.Play();

		for (int i = 0; i < m_AnimCount; i++)
		{
			if (i == roleAnimInfo.inputPort)
			{
				m_AnimationMixerPlayable.SetInputWeight(i, 1);
			}
			else
			{
				m_AnimationMixerPlayable.SetInputWeight(i, 0);
			}
		}
		if (onComplete != null)
		{
			GameEntry.Time.CreateTimeAction().Init(delayTime: roleAnimInfo.CurrPlayable.GetAnimationClip().length, onStar: () =>
			{
				playable.Pause();
				onComplete();
			}).Run();
		}
	}

	/// <summary>
	/// 根据项目业务情况自行选择Init参数, 可能只有FBX嵌入动画, 也可能是动画文件, 也可能两者共用
	/// </summary>
	/// <param name="animator"></param>
	/// <param name="animFBXPath"></param>
	/// <param name="animGroupId"></param>
	/// <param name="onComplete"></param>
	public void InitAnim(Animator animator, string animFBXPath = null, int animGroupId = -1, BaseAction onComplete = null)
	{
		//初始化Playable
		if (string.IsNullOrWhiteSpace(animFBXPath) && animGroupId == -1) return;
		if (m_PlayableGraph.IsValid()) m_PlayableGraph.Destroy();
		if (animFBXPath != null)
		{
			m_PlayableGraph = PlayableGraph.Create("PlayableGraph_" + animFBXPath);
		}
		else
		{
			m_PlayableGraph = PlayableGraph.Create("PlayableGraph_" + animGroupId);
		}
		m_AnimationPlayableOutput = AnimationPlayableOutput.Create(m_PlayableGraph, "output", animator);
		CreateMixerPlayable();

		//初始化动画文件
		m_RoleAnimInfoDic.Clear();
		LoadInitRoleAnimations(animGroupId);
		LoadInitRoleAnimationsByFBX(animFBXPath, onComplete);
	}
	/// <summary>
	/// 创建混合Playable
	/// </summary>
	private void CreateMixerPlayable()
	{
		m_AnimationMixerPlayable = AnimationMixerPlayable.Create(m_PlayableGraph, m_AnimCount);
		m_AnimationPlayableOutput.SetSourcePlayable(m_AnimationMixerPlayable, 0);
	}

	#region FBX嵌入性动画
	private void LoadInitRoleAnimationsByFBX(string path, BaseAction omComplete)
	{
#if EDITORLOAD && UNITY_EDITOR
		UnityEngine.Object[] objs = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
		List<AnimationClip> clips = new List<AnimationClip>();
		foreach (var item in objs)
		{
			if (item is AnimationClip) clips.Add(item as AnimationClip);
		}
		LoadRoleAnimation(clips.ToArray(), omComplete);
#elif RESOURCES
		string resourcesPath = path.Replace("Assets/Download/", string.Empty);
			LoadRoleAnimation(Resources.LoadAll<AnimationClip>(resourcesPath), omComplete);
#else
		AssetEntity m_CurrAssetEnity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(AssetCategory.Role, path);
		GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(m_CurrAssetEnity.AssetBundleName, onComplete: (ResourceEntity bundleEntity) =>
			{
			AssetBundle bundle = bundleEntity.Target as AssetBundle;
				LoadRoleAnimation(bundle.LoadAllAssets<AnimationClip>(), omComplete);
			});
#endif
	}
	private void LoadRoleAnimation(AnimationClip[] clips, BaseAction omComplete)
	{
		for (int i = 0; i < clips.Length; i++)
		{
			AnimationClip clip = clips[i];
			AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(m_PlayableGraph, clip);
			m_PlayableGraph.Connect(animationClipPlayable, 0, m_AnimationMixerPlayable, i);
			m_AnimationMixerPlayable.SetInputWeight(i, 0);

			m_RoleAnimInfoDic.Add(i, new RoleAnimInfo()
			{
				IsLoad = true,
				inputPort = i,
				AnimClipName = clip.name,
				CurrPlayable = animationClipPlayable,
			});
		}
		omComplete?.Invoke();
	}
	#endregion
	#region 复用性动画
	/// <summary>
	/// 通过Anim组的编号加载(复用Anim文件独用方式)
	/// </summary>
	/// <param name="animGroupId"></param>
	private void LoadInitRoleAnimations(int animGroupId)
	{
		List<Sys_AnimationEntity> sys_AnimationList = GameEntry.DataTable.Sys_AnimationDBModel.GetListByGroupId(animGroupId);
		if (sys_AnimationList == null) return;
		for (int i = 0; i < sys_AnimationList.Count; i++)
		{
			Sys_AnimationEntity sys_Animation = sys_AnimationList[i];
			m_RoleAnimInfoDic.Add(sys_Animation.Id, new RoleAnimInfo()
			{
				CurrRoleAnimationData = sys_Animation,
				IsLoad = false,
				inputPort = i,
			});

			if (sys_Animation.InitLoad == 1)
			{
				LoadRoleAnimation(sys_Animation);
			}
		}
	}
	/// <summary>
	/// 加载单个Anim文件
	/// </summary>
	/// <param name="sys_Animation"></param>
	/// <param name="onComplete"></param>
	private void LoadRoleAnimation(Sys_AnimationEntity sys_Animation, BaseAction<RoleAnimInfo> onComplete = null)
	{
		GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(AssetCategory.Role, sys_Animation.AnimPath, (AnimationClip animationClip) =>
		{
			AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(m_PlayableGraph, animationClip);

			RoleAnimInfo roleAnimInfo = null;
			if (m_RoleAnimInfoDic.TryGetValue(sys_Animation.Id, out roleAnimInfo))
			{
				roleAnimInfo.CurrPlayable = animationClipPlayable;
				roleAnimInfo.IsLoad = true;

				m_PlayableGraph.Connect(animationClipPlayable, 0, m_AnimationMixerPlayable, roleAnimInfo.inputPort);
				m_AnimationMixerPlayable.SetInputWeight(roleAnimInfo.inputPort, 0);

				onComplete?.Invoke(roleAnimInfo);
			}
		});
	}
	public void CheckUnloadRoleAnimation()
	{
		var enumerator = m_RoleAnimInfoDic.GetEnumerator();
		while (enumerator.MoveNext())
		{
			RoleAnimInfo roleAnimInfo = enumerator.Current.Value;
			if (roleAnimInfo.IsExpire)
			{
				roleAnimInfo.IsLoad = false;
				roleAnimInfo.CurrPlayable.Destroy();
			}
		}
	}
	#endregion
	#endregion
}
