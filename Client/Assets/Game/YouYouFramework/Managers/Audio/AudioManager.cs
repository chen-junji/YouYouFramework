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
    public class AudioManager
    {
        private AudioSource bgmSource;

        public Sys_AudioEntity CurrBGMEntity;
        public float PlayerBGMVolume { get; private set; }
        public float PlayerAudioVolume { get; private set; }

        public FMODManager FMOD { get; private set; }

        public AudioManager()
        {
            FMOD = new FMODManager();
        }
        public void Init()
        {
            ReleaseInterval = 10;

            bgmSource = GameEntry.Instance.gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;

            GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.PlayerBGMVolume, RefreshBGM);
            GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.PlayerAudioVolume, RefreshAudio);
            RefreshBGM(null);
            RefreshAudio(null);

            GameEntry.Time.Create(delayTime: ReleaseInterval, loop: -1, interval: ReleaseInterval, onUpdate: (updateValue) => Release());

            FMOD.Init();
        }
        private void RefreshAudio(object userData)
        {
            PlayerAudioVolume = GameEntry.PlayerPrefs.GetLoggerDic(CommonEventId.PlayerAudioVolume).ToFloat(1);
        }
        private void RefreshBGM(object userData)
        {
            PlayerBGMVolume = GameEntry.PlayerPrefs.GetLoggerDic(CommonEventId.PlayerBGMVolume).ToFloat(1);
            if (CurrBGMEntity != null) bgmSource.volume = CurrBGMEntity.Volume * PlayerBGMVolume;
        }

        #region BGM
        public async void PlayBGM(string audioName)
        {
            CurrBGMEntity = GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase));
            AudioClip audioClip = await GameEntry.Resource.ResourceLoaderManager.LoadMainAsset<AudioClip>(string.Format("Audio/{0}.mp3", CurrBGMEntity.AssetPath));
            bgmSource.clip = audioClip;
            bgmSource.Play();
            bgmSource.loop = CurrBGMEntity.IsLoop == 1;
            bgmSource.volume = CurrBGMEntity.Volume * PlayerBGMVolume;
        }
        internal void StopBGM()
        {
            bgmSource.Stop();
        }
        #endregion

        #region 音效
        /// <summary>
        /// 释放间隔
        /// </summary>
        public int ReleaseInterval { get; private set; }

        /// <summary>
        /// 音效字典
        /// </summary>
        private Dictionary<string, AudioSource> m_CurrAudioEventsDic = new Dictionary<string, AudioSource>();

        /// <summary>
        /// 需要释放的音效
        /// </summary>
        private LinkedList<string> m_NeedRemoveList = new LinkedList<string>();

        public async void PlayAudio(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase));
            AudioClip audioClip = await GameEntry.Resource.ResourceLoaderManager.LoadMainAsset<AudioClip>(string.Format("Audio/{0}.mp3", sys_Audio.AssetPath));
            if (audioClip != null)
            {
                AudioSource source = null;
                if (!m_CurrAudioEventsDic.TryGetValue(sys_Audio.AssetPath, out source))
                {
                    source = GameEntry.Instance.gameObject.AddComponent<AudioSource>();
                    m_CurrAudioEventsDic.Add(sys_Audio.AssetPath, source);
                }
                source.volume = sys_Audio.Volume * PlayerAudioVolume;
                source.clip = audioClip;
                source.loop = sys_Audio.IsLoop == 1;
                source.Play();
            }
            else
            {
                Debug.LogError("PlaySound找不到音效,audioName==" + audioName);
            }
        }

        /// <summary>
        /// 暂停某个音效
        /// </summary>
        internal bool PausedAudio(string audioName, bool paused = true)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase));
            AudioSource source = null;
            if (m_CurrAudioEventsDic.TryGetValue(sys_Audio.AssetPath, out source))
            {
                if (paused)
                {
                    source.Pause();
                }
                else
                {
                    source.Play();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 释放可释放的音效
        /// </summary>
        private void Release()
        {
            var enumerator = m_CurrAudioEventsDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AudioSource source = enumerator.Current.Value;
                if (source.isPlaying) continue;
                m_NeedRemoveList.AddLast(enumerator.Current.Key);
                UnityEngine.Object.Destroy(source);
            }

            LinkedListNode<string> currNode = m_NeedRemoveList.First;
            while (currNode != null)
            {
                LinkedListNode<string> next = currNode.Next;
                string serialId = currNode.Value;
                m_CurrAudioEventsDic.Remove(serialId);
                m_NeedRemoveList.Remove(currNode);
                currNode = next;
            }
        }
        #endregion

    }
}