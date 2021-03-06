using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// 音频管理器
	/// </summary>
	public class AudioManager : ManagerBase,IDisposable
	{
		internal override void Init()
		{
			m_NextReleaseTime = Time.time;
		}
		public void Dispose()
		{
			ShopAllAudio();
			StopBGM();
		}
		internal void OnUpdate()
		{
			if (Time.time > m_NextReleaseTime + m_ReleaseInterval)
			{
				m_NextReleaseTime = Time.time;
				Release();
			}
		}

		internal void LoadBanks(BaseAction onComplete)
		{
#if EDITORLOAD
			string[] arr = Directory.GetFiles(Application.dataPath + "/Download/Audio/", "*.bytes");
			for (int i = 0; i < arr.Length; i++)
			{
				FileInfo file = new FileInfo(arr[i]);
				TextAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Download/Audio/" + file.Name);
				RuntimeManager.LoadBank(asset);
			}
			if (onComplete != null) onComplete();
#elif ASSETBUNDLE
			GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.AudioAssetBundlePath, onComplete: (ResourceEntity bundleEntity) =>
			{
				AssetBundle bundle = bundleEntity.Target as AssetBundle;
				if (bundle != null)
				{
					TextAsset[] arr = bundle.LoadAllAssets<TextAsset>();
					int len = arr.Length;
					for (int i = 0; i < len; i++)
					{
						RuntimeManager.LoadBank(arr[i]);
					}
				}
				if (onComplete != null) onComplete();
			});
#elif RESOURCES
			TextAsset[] assets = Resources.LoadAll<TextAsset>("Audio");
            for (int i = 0; i < assets.Length; i++)
            {
                RuntimeManager.LoadBank(assets[i]);
            }
            if (onComplete != null) onComplete();
#endif

		}

		#region BGM

		private string m_CurrBGMAudio;

		private float m_CurrBGMVolume;

		private float m_CurrBGMMaxVolume;

		private EventInstance BGMEvent;

		private TimeAction m_CurrBGMTimeAction;

		/// <summary>
		public float PlayerBGMVolume
		{
			get { return m_PlayerBGMVolume; }
			set
			{
				m_PlayerBGMVolume = value;
				SetBGMVolume(m_CurrBGMVolume);
			}
		}
		private float m_PlayerBGMVolume = 1;
		/// 播放BGM
		/// </summary>
		public void PlayBGM(int audioId)
		{
			Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetDic(audioId);
			if (sys_Audio != null)
			{
				PlayBGM(sys_Audio.AssetPath, sys_Audio.Volume);
			}
			else
			{
				GameEntry.LogError("BGM不存在Id={0}", audioId);
			}
		}
		/// <summary>
		/// 播放BGM
		/// </summary>
		/// <param name="bgmPath"></param>
		public void PlayBGM(string bgmPath, float volume = 1)
		{
			m_CurrBGMAudio = bgmPath;
			m_CurrBGMMaxVolume = volume;
			CheckBGMEventInstance();
		}
		/// <summary>
		/// BGM切换参数
		/// </summary>
		private void CheckBGMEventInstance(string switchName, float value)
		{
			BGMEvent.setParameterByName(switchName, value);
		}
		/// <summary>
		/// 设置BGM音量
		/// </summary>
		/// <param name="value"></param>
		private void SetBGMVolume(float value)
		{
			BGMEvent.setVolume(value * PlayerBGMVolume);
		}
		/// <summary>
		/// 暂停BGM
		/// </summary>
		/// <param name="pause"></param>
		public void PauseBGM(bool pause)
		{
			if (!BGMEvent.isValid()) CheckBGMEventInstance();
			if (BGMEvent.isValid()) BGMEvent.setPaused(pause);
		}
		/// <summary>
		/// 检查BGM实例把之前的释放掉
		/// </summary>
		private void CheckBGMEventInstance()
		{
			if (!string.IsNullOrEmpty(m_CurrBGMAudio))
			{
				if (BGMEvent.isValid())
				{
					BGMEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
					BGMEvent.release();
				}

				BGMEvent = RuntimeManager.CreateInstance(m_CurrBGMAudio);

				m_CurrBGMVolume = 0;
				SetBGMVolume(m_CurrBGMVolume);

				BGMEvent.start();

				//把音量逐渐变成Max
				m_CurrBGMTimeAction = GameEntry.Time.CreateTimeAction();
				m_CurrBGMTimeAction.Init(null, 0, 0.05f, 100, null, (int loop) =>
				{
					m_CurrBGMVolume += m_CurrBGMMaxVolume / 10;
					m_CurrBGMVolume = Mathf.Min(m_CurrBGMVolume, m_CurrBGMMaxVolume);
					SetBGMVolume(m_CurrBGMVolume);
				}, null).Run();
			}
		}

		/// <summary>
		/// 停止播放BGM
		/// </summary>
		internal void StopBGM()
		{
			if (BGMEvent.isValid())
			{
				//把音量逐渐变成0 再停止
				m_CurrBGMTimeAction = GameEntry.Time.CreateTimeAction();
				m_CurrBGMTimeAction.Init("StopBGM", 0, 0.05f, 100, null, (int loop) =>
				{
					m_CurrBGMVolume -= m_CurrBGMMaxVolume / 10;
					m_CurrBGMVolume = Mathf.Max(m_CurrBGMVolume, 0);
					SetBGMVolume(m_CurrBGMVolume);

					if (m_CurrBGMVolume == 0)
					{
						BGMEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
					}
				}, () =>
				{
					BGMEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
				}).Run();
			}
		}



		/// <summary>
		/// 开始播放BGM
		/// </summary>
		internal void StartBGM()
		{
			BGMEvent.start();
		}

		#endregion

		#region 音效
		/// <summary>
		/// 释放间隔
		/// </summary>
		private int m_ReleaseInterval = 120;
		/// <summary>
		/// 下次释放时间
		/// </summary>
		private float m_NextReleaseTime = 0f;

		/// <summary>
		/// 序号
		/// </summary>
		private int m_Serial = 0;

		/// <summary>
		/// 音效字典
		/// </summary>
		private Dictionary<int, EventInstance> m_CurrAudioEventsDic = new Dictionary<int, EventInstance>();

		/// <summary>
		/// 需要释放的音效编号
		/// </summary>
		private LinkedList<int> m_NeedRemoveList = new LinkedList<int>();

		/// <summary>
		public float PlayerAudioVolume = 1;
		/// 播放音效
		/// </summary>
		public int PlayAudio(int audioId, string parameterName = null, float parameterValue = 0, Vector3 pos3D = default)
		{
			if (GameEntry.Procedure != null && (int)GameEntry.Procedure.CurrProcedureState <= 2) return -1;

			Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetDic(audioId);
			if (sys_Audio != null)
			{
				return PlayAudio(sys_Audio.AssetPath, sys_Audio.Volume, parameterName, parameterValue, sys_Audio.Is3D == 1, pos3D);
			}
			else
			{
				GameEntry.LogError("Audio不存在Id={0}", audioId);
				return -1;
			}
		}
		/// <summary>
		/// 播放音效
		/// </summary>
		internal int PlayAudio(string eventPath, float volume = 1, string parameterName = null, float parameterValue = 0, bool is3D = false, Vector3 pos3D = default)
		{
			if (string.IsNullOrEmpty(eventPath)) return -1;
			
			//生成一个音频
			EventInstance eventInstance = RuntimeManager.CreateInstance(eventPath);
			//设置音量
			eventInstance.setVolume(volume * PlayerAudioVolume);
			//设置参数
			if (!string.IsNullOrEmpty(parameterName)) eventInstance.setParameterByName(parameterName, parameterValue);
			//设置3D音效
			if (is3D) eventInstance.set3DAttributes(pos3D.To3DAttributes());

			eventInstance.start();
			int serialId = m_Serial++;
			m_CurrAudioEventsDic[serialId] = eventInstance;
			return serialId;
		}

		/// <summary>
		/// 设置音效参数
		/// </summary>
		internal void SetParameterForAudio(int serialId, string parameterName, float value)
		{
			EventInstance eventInstance;
			if (m_CurrAudioEventsDic.TryGetValue(serialId, out eventInstance))
			{
				if (eventInstance.isValid()) eventInstance.setParameterByName(parameterName, value);
			}
		}

		/// <summary>
		/// 暂停某个音效
		/// </summary>
		/// <param name="serialId"></param>
		/// <param name="paused"></param>
		/// <returns></returns>
		internal bool PausedAudio(int serialId, bool paused = true)
		{
			EventInstance eventInstance;
			if (m_CurrAudioEventsDic.TryGetValue(serialId, out eventInstance))
			{
				if (eventInstance.isValid())
				{
					return eventInstance.setPaused(paused) == FMOD.RESULT.OK;
				}
			}
			return false;
		}

		/// <summary>
		/// 停止某个音效
		/// </summary>
		/// <param name="serialId"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		internal bool StopAudio(int serialId, FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
		{
			EventInstance eventInstance;
			if (m_CurrAudioEventsDic.TryGetValue(serialId, out eventInstance))
			{
				if (eventInstance.isValid())
				{
					var result = eventInstance.stop(mode);
					eventInstance.release();
					m_CurrAudioEventsDic.Remove(serialId);
					return result == FMOD.RESULT.OK;
				}
			}
			return false;
		}

		/// <summary>
		/// 停止所有音效
		/// </summary>
		internal void ShopAllAudio()
		{
			var enumerator = m_CurrAudioEventsDic.GetEnumerator();
			while (enumerator.MoveNext())
			{
				EventInstance eventInstance = enumerator.Current.Value;
				if (eventInstance.isValid())
				{
					var reselt = eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
					eventInstance.release();
				}
			}
			m_CurrAudioEventsDic.Clear();
		}
		/// <summary>
		/// 释放可释放的音效
		/// </summary>
		private void Release()
		{
			var lst = m_CurrAudioEventsDic.GetEnumerator();
			while (lst.MoveNext())
			{
				EventInstance eventInstance = lst.Current.Value;
				if (!eventInstance.isValid()) continue;

				PLAYBACK_STATE state;
				eventInstance.getPlaybackState(out state);
				if (state == PLAYBACK_STATE.STOPPED)
				{
					m_NeedRemoveList.AddLast(lst.Current.Key);
					eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
					eventInstance.release();
				}
			}

			LinkedListNode<int> currNode = m_NeedRemoveList.First;
			while (currNode != null)
			{
				LinkedListNode<int> next = currNode.Next;
				int serialId = currNode.Value;
				m_CurrAudioEventsDic.Remove(serialId);
				m_NeedRemoveList.Remove(currNode);
				currNode = next;
			}
		}


		#endregion
	}
}