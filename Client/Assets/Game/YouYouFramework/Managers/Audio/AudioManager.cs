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
        private float _soundVolume = 1;

        public FMODManager FMOD { get; private set; }

        public AudioManager()
        {
            FMOD = new FMODManager();
        }
        public void Init()
        {
            bgmSource = GameEntry.Instance.gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;

            GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.PlayerBGMVolume, RefreshBGM);
            GameEntry.Event.CommonEvent.AddEventListener(CommonEventId.PlayerAudioVolume, RefreshAudio);
            RefreshBGM(null);
            RefreshAudio(null);

            FMOD.Init();
        }
        internal void OnUpdate()
        {
            FMOD.OnUpdate();
        }
        private void RefreshAudio(object userData)
        {
            _soundVolume = GameEntry.Data.PlayerPrefs.GetLoggerDic(CommonEventId.PlayerAudioVolume).ToFloat(1);
        }
        private void RefreshBGM(object userData)
        {
            bgmSource.volume = GameEntry.Data.PlayerPrefs.GetLoggerDic(CommonEventId.PlayerBGMVolume).ToFloat(1);
        }

        #region BGM
        public async void PlayBGM(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase));
            AudioClip audioClip = await GameEntry.Resource.ResourceLoaderManager.LoadMainAsset<AudioClip>(string.Format("Assets/Download/Audio/{0}.mp3", sys_Audio.AssetPath));
            bgmSource.clip = audioClip;
            bgmSource.Play();
            bgmSource.loop = sys_Audio.IsLoop == 1;
        }
        internal void StopBGM()
        {
            bgmSource.Stop();
        }
        #endregion

        #region 音效
        public async void PlayAudio(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase));
            AudioClip audioClip = await GameEntry.Resource.ResourceLoaderManager.LoadMainAsset<AudioClip>(string.Format("Assets/Download/Audio/{0}.mp3", sys_Audio.AssetPath));
            if (audioClip != null)
            {
                AudioSource source = GameEntry.Instance.gameObject.AddComponent<AudioSource>();
                source.volume = _soundVolume;
                source.clip = audioClip;
                source.loop = sys_Audio.IsLoop == 1;
                source.Play();
            }
            else
            {
                Debug.LogError("PlaySound找不到音效,audioName==" + audioName);
            }
        }
        public async void PlayAudio3D(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetList().Find(x => x.AssetPath.Equals(audioName, StringComparison.CurrentCultureIgnoreCase));
            AudioClip audioClip = await GameEntry.Resource.ResourceLoaderManager.LoadMainAsset<AudioClip>(string.Format("Assets/Download/Audio/{0}.mp3", sys_Audio.AssetPath));
            if (audioClip != null)
            {
                AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position, _soundVolume);
            }
            else
            {
                Debug.LogError("PlaySound3D找不到音效,audioName==" + audioName);
            }
        }
        #endregion

    }
}