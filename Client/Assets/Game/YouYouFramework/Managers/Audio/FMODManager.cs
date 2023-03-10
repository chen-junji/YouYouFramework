using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System;
using System.IO;

namespace YouYou
{
    public class FMODManager
    {
        internal void Init()
        {
            GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.PlayerAudioVolume, RefreshAudio);
            GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.PlayerBGMVolume, RefreshBGM);
            RefreshAudio(null);
            RefreshBGM(null);

            GameEntry.Time.Create(delayTime: GameEntry.Audio.ReleaseInterval, loop: -1, interval: GameEntry.Audio.ReleaseInterval, onUpdate: (updateValue) => Release());
        }
        private void RefreshBGM(object userData)
        {
            SetBGMVolume();
        }
        private void RefreshAudio(object userData)
        {
        }

        internal void LoadBanks(Action onComplete)
        {
#if UNITY_EDITOR && EDITORLOAD
            string[] arr = Directory.GetFiles(Application.dataPath + "/Download/FMOD/", "*.bytes");
            for (int i = 0; i < arr.Length; i++)
            {
                FileInfo file = new FileInfo(arr[i]);
                TextAsset asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Download/FMOD/" + file.Name);
                RuntimeManager.LoadBank(asset);
            }
            if (onComplete != null) onComplete();
#elif ASSETBUNDLE
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.AudioAssetBundlePath, onComplete: (AssetBundle bundle) =>
			{
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
            TextAsset[] assets = Resources.LoadAll<TextAsset>("FMOD");
            for (int i = 0; i < assets.Length; i++)
            {
                RuntimeManager.LoadBank(assets[i]);
            }
            if (onComplete != null) onComplete();
#endif

        }

        #region BGM
        private float m_CurrBGMVolume;

        private float m_CurrBGMMaxVolume;

        private string m_CurrBGMPath;
        private EventInstance BGMEvent;

        public void PlayBGM(string audioName)
        {
            PlayBGM(GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase)));
        }
        public void PlayBGM(Sys_AudioEntity sys_Audio)
        {
            if (sys_Audio != null)
            {
                PlayBGM(sys_Audio.AssetPath, sys_Audio.Volume);
            }
            else
            {
                GameEntry.LogError("BGM?????????");
            }
        }
        public void PlayBGM(string bgmPath, float volume = 1)
        {
            m_CurrBGMPath = bgmPath;
            m_CurrBGMMaxVolume = volume;
            CheckBGMEventInstance();
        }
        /// <summary>
        /// BGM????????????
        /// </summary>
        private void CheckBGMEventInstance(string switchName, float value)
        {
            BGMEvent.setParameterByName(switchName, value);
        }
        /// <summary>
        /// ??????BGM??????
        /// </summary>
        /// <param name="value"></param>
        private void SetBGMVolume()
        {
            if (BGMEvent.isValid()) BGMEvent.setVolume(m_CurrBGMVolume * GameEntry.Audio.PlayerBGMVolume);
        }
        /// <summary>
        /// ??????BGM
        /// </summary>
        /// <param name="pause"></param>
        public void PauseBGM(bool pause)
        {
            if (BGMEvent.isValid()) BGMEvent.setPaused(pause);
        }
        /// <summary>
        /// ??????BGM???????????????????????????
        /// </summary>
        private void CheckBGMEventInstance()
        {
            if (!string.IsNullOrEmpty(m_CurrBGMPath))
            {
                StopBGM();

                BGMEvent = RuntimeManager.CreateInstance("event:/" + m_CurrBGMPath);

                m_CurrBGMVolume = 0;
                SetBGMVolume();

                BGMEvent.start();

                //?????????????????????Max
                GameEntry.Time.Create(null, 0, 0.05f, 100, true, null, (int loop) =>
                {
                    m_CurrBGMVolume = Mathf.Min(m_CurrBGMVolume + m_CurrBGMMaxVolume / 10, m_CurrBGMMaxVolume);
                    SetBGMVolume();
                }, null);
            }
        }

        /// <summary>
        /// ????????????BGM
        /// </summary>
        internal void StopBGM()
        {
            if (BGMEvent.isValid())
            {
                BGMEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                BGMEvent.release();
            }
        }



        /// <summary>
        /// ????????????BGM
        /// </summary>
        internal void StartBGM()
        {
            BGMEvent.start();
        }

        #endregion

        #region ??????
        /// <summary>
        /// ??????
        /// </summary>
        private int m_Serial = 0;

        /// <summary>
        /// ????????????
        /// </summary>
        private Dictionary<int, EventInstance> m_CurrAudioEventsDic = new Dictionary<int, EventInstance>();

        /// <summary>
        /// ?????????????????????
        /// </summary>
        private LinkedList<int> m_NeedRemoveList = new LinkedList<int>();

        public int PlayAudio(string audioName, string parameterName = null, float parameterValue = 0, Vector3 pos3D = default)
        {
            return PlayAudio(GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase)), parameterName, parameterValue, pos3D);
        }
        /// <summary>
        /// ????????????
        /// </summary>
        public int PlayAudio(Sys_AudioEntity sys_Audio, string parameterName = null, float parameterValue = 0, Vector3 pos3D = default)
        {
            if (GameEntry.Procedure != null && (int)GameEntry.Procedure.CurrProcedureState <= 2) return -1;

            if (sys_Audio != null)
            {
                return PlayAudio(sys_Audio.AssetPath, sys_Audio.Volume, parameterName, parameterValue, pos3D);
            }
            else
            {
                GameEntry.LogError("Audio?????????");
                return -1;
            }
        }
        /// <summary>
        /// ????????????
        /// </summary>
        internal int PlayAudio(string eventPath, float volume = 1, string parameterName = null, float parameterValue = 0, Vector3 pos3D = default)
        {
            if (string.IsNullOrEmpty(eventPath)) return -1;

            //??????????????????
            EventInstance eventInstance = RuntimeManager.CreateInstance("event:/" + eventPath);
            //????????????
            eventInstance.setVolume(volume * GameEntry.Audio.PlayerAudioVolume);
            //????????????
            if (!string.IsNullOrEmpty(parameterName)) eventInstance.setParameterByName(parameterName, parameterValue);
            //??????3D??????
            if (pos3D != default) eventInstance.set3DAttributes(pos3D.To3DAttributes());

            eventInstance.start();
            int serialId = m_Serial++;
            m_CurrAudioEventsDic[serialId] = eventInstance;
            return serialId;
        }

        /// <summary>
        /// ??????????????????
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
        /// ??????????????????
        /// </summary>
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
        /// ??????????????????
        /// </summary>
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
        /// ??????????????????
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
        /// ????????????????????????
        /// </summary>
        private void Release()
        {
            var enumerator = m_CurrAudioEventsDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                EventInstance eventInstance = enumerator.Current.Value;
                if (!eventInstance.isValid()) continue;

                PLAYBACK_STATE state;
                eventInstance.getPlaybackState(out state);
                if (state == PLAYBACK_STATE.STOPPED)
                {
                    m_NeedRemoveList.AddLast(enumerator.Current.Key);
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