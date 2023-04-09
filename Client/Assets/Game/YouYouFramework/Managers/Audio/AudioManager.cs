using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YouYou
{
    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioManager
    {
        public AudioSource BGMSource { get; private set; }

        public Sys_AudioEntity CurrBGMEntity;
        public float PlayerBGMVolume { get; private set; }
        public float PlayerAudioVolume { get; private set; }

        private AudioSource AudioSourcePrefab;

        private Queue<AudioSource> HelpPool = new Queue<AudioSource>();

        private List<AudioSource> AudioSourceList = new List<AudioSource>();


        public void Init()
        {
            AudioSourcePrefab = new GameObject("AudioItem", typeof(AudioSource)).GetComponent<AudioSource>();
            AudioSourcePrefab.transform.SetParent(GameEntry.Instance.AudioGroup);
            AudioSourcePrefab.playOnAwake = false;
            AudioSourcePrefab.maxDistance = 20;

            BGMSource = Object.Instantiate(AudioSourcePrefab, GameEntry.Instance.AudioGroup);
            BGMSource.loop = true;
            BGMSource.name = "BGMSource";

            GameEntry.Event.Common.AddEventListener(CommonEventId.PlayerBGMVolume, RefreshBGM);
            GameEntry.Event.Common.AddEventListener(CommonEventId.PlayerAudioVolume, RefreshAudio);
            GameEntry.Event.Common.AddEventListener(CommonEventId.GamePause, OnGamePause);

            RefreshBGM(null);
            RefreshAudio(null);
        }

        private void RefreshAudio(object userData)
        {
            PlayerAudioVolume = GameEntry.PlayerPrefs.GetFloat(CommonEventId.PlayerAudioVolume);
        }
        private void RefreshBGM(object userData)
        {
            PlayerBGMVolume = GameEntry.PlayerPrefs.GetFloat(CommonEventId.PlayerBGMVolume);
            if (CurrBGMEntity != null) BGMSource.volume = CurrBGMEntity.Volume * PlayerBGMVolume;
        }

        private void OnGamePause(object userData)
        {
            int GamePause = userData.ToInt();
            AudioSourceList.ForEach(x =>
            {
                if (x != null) x.mute = GamePause == 1;
            });
        }

        #region BGM
        private TimeAction timeActionIn;
        private TimeAction timeActionOut;
        public void PlayBGM(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName);
            if (sys_Audio == null)
            {
                Debug.LogError("sys_Audio==null, audioName==" + audioName);
                return;
            }
            if (sys_Audio == CurrBGMEntity)
            {
                return;
            }

            CurrBGMEntity = sys_Audio;
            PlayBGM(CurrBGMEntity.AssetPath, CurrBGMEntity.IsLoop == 1, CurrBGMEntity.IsFadeIn == 1, CurrBGMEntity.Volume);
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "PlayBGM");
        }
        public void PlayBGM(int audioId)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetDic(audioId);
            if (sys_Audio == null)
            {
                Debug.LogError("sys_Audio==null, audioId==" + audioId);
                return;
            }
            CurrBGMEntity = sys_Audio;
            PlayBGM(CurrBGMEntity.AssetPath, CurrBGMEntity.IsLoop == 1, CurrBGMEntity.IsFadeIn == 1, CurrBGMEntity.Volume);
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "PlayBGM");
        }
        public async void PlayBGM(string assetPath, bool isLoop, bool isFadeIn, float entityVolume)
        {
            AudioClip audioClip = await GameEntry.Resource.ResourceLoaderManager.LoadMainAssetAsync<AudioClip>(assetPath);
            PlayBGM(audioClip, isLoop, isFadeIn, entityVolume);
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "PlayBGM");
        }
        public void PlayBGM(AudioClip audioClip, bool isLoop, bool isFadeIn, float entityVolume)
        {
            if (audioClip == null)
            {
                Debug.LogError("audioClip==null");
                return;
            }
            StopBGM(() =>
            {
                BGMSource.clip = audioClip;
                BGMSource.Play();
                BGMSource.loop = isLoop;

                if (isFadeIn)
                {
                    //把音量逐渐变成Max
                    if (timeActionIn != null)
                    {
                        timeActionIn.Stop();
                        timeActionIn = null;
                    }
                    BGMSource.volume = 0;
                    timeActionIn = GameEntry.Time.Create(interval: 0.15f, loop: 10, unScaled: true, onUpdate: (int loop) =>
                    {
                        if (BGMSource == null) return;
                        float volume = entityVolume * PlayerBGMVolume;
                        BGMSource.volume = Mathf.Min(BGMSource.volume + volume / 10, volume);
                    }, onComplete: () =>
                    {
                        if (BGMSource == null) return;
                        timeActionIn = null;
                        BGMSource.volume = entityVolume * PlayerBGMVolume;
                    });
                }
                else
                {
                    BGMSource.volume = entityVolume * PlayerBGMVolume;
                }
            });
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "PlayBGM");
        }

        internal void StopBGM(Action volumeOut = null)
        {
            if (CurrBGMEntity == null || CurrBGMEntity.IsFadeOut == 0)
            {
                BGMSource.Stop();
                volumeOut?.Invoke();
                return;
            }

            if (timeActionOut != null)
            {
                timeActionOut.Stop();
                timeActionOut = null;
            }
            //把音量逐渐变成0 再停止
            timeActionOut = GameEntry.Time.Create(interval: 0.15f, loop: 10, unScaled: true, onUpdate: (int loop) =>
            {
                if (BGMSource == null) return;
                float volume = CurrBGMEntity.Volume * PlayerBGMVolume;
                BGMSource.volume = Mathf.Max(BGMSource.volume - volume / 10, 0);
            }, onComplete: () =>
            {
                if (BGMSource == null) return;
                timeActionOut = null;
                BGMSource.Stop();
                volumeOut?.Invoke();
            });
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "StopBGM");
        }

        public void PauseBGM(bool isPause)
        {
            if (BGMSource == null || BGMSource.clip == null) return;
            if (isPause)
            {
                BGMSource.Pause();
            }
            else
            {
                BGMSource.UnPause();
            }
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "PauseBGM");

        }
        #endregion

        #region 音效
        public void PlayAudio(AudioClip audioClip, Vector3 point, float volume = 1, bool loop = false, int priority = 128)
        {
            AudioSource audioSource = PlayAudio2(audioClip, volume, loop, priority);
            if (audioSource == null) return;
            audioSource.transform.position = point;
            audioSource.spatialBlend = 1;
        }
        public void PlayAudio(AudioClip audioClip, float volume = 1, bool loop = false, int priority = 128)
        {
            PlayAudio2(audioClip, volume, loop, priority);
        }
        public void PlayAudio(string audioName, Vector3 point)
        {
            AudioSource helper = GetAudioSource(audioName);
            if (helper == null) return;
            helper.transform.position = point;
            helper.spatialBlend = 1;
        }
        public void PlayAudio(string audioName)
        {
            GetAudioSource(audioName);
        }
        private AudioSource GetAudioSource(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName);
            if (sys_Audio == null)
            {
                GameEntry.LogError(LogCategory.Audio, "sys_Audio==null, audioName==" + audioName);
                return null;
            }

            AudioClip audioClip = GameEntry.Resource.ResourceLoaderManager.LoadMainAsset<AudioClip>(sys_Audio.AssetPath);
            if (audioClip == null)
            {
                GameEntry.LogError(LogCategory.Audio, "PlaySound找不到音效,audioName==" + audioName);
                return null;
            }

            AudioSource helper = PlayAudio2(audioClip, sys_Audio.Volume, sys_Audio.IsLoop == 1, sys_Audio.Priority);
            return helper;
        }
        private AudioSource PlayAudio2(AudioClip audioClip, float volume = 1, bool loop = false, int priority = 128)
        {
            if (PlayerAudioVolume == 0f) return null;

            AudioSource helper = Dequeue();
            AudioSourceList.Add(helper);
            helper.clip = audioClip;
            helper.mute = false;
            helper.volume = volume * PlayerAudioVolume;
            helper.loop = loop;
            helper.priority = priority;
            helper.spatialBlend = 0;
            helper.Play();

            if (!helper.loop)
            {
                GameEntry.Time.Create(delayTime: audioClip.length, onComplete: () =>
                {
                    if (helper == null) return;
                    bool isRemove = AudioSourceList.Remove(helper);
                    if (isRemove) Enqueue(helper);
                });
            }
            return helper;
        }

        private AudioSource Dequeue()
        {
            if (HelpPool.Count > 0)
            {
                return HelpPool.Dequeue();
            }
            else
            {
                return Object.Instantiate(AudioSourcePrefab, GameEntry.Instance.AudioGroup);
            }
        }
        private void Enqueue(AudioSource helper)
        {
            helper.clip = null;
            HelpPool.Enqueue(helper);
        }
        #endregion

    }
}