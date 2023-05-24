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
        public float MasterVolume { get; private set; }
        public float PlayerBGMVolume { get; private set; }
        public float PlayerAudioVolume { get; private set; }


        public void Init()
        {
            AudioSourcePrefab = new GameObject("AudioSource", typeof(AudioSource), typeof(PoolObj)).GetComponent<AudioSource>();
            AudioSourcePrefab.transform.SetParent(GameEntry.Instance.transform);
            AudioSourcePrefab.playOnAwake = false;
            AudioSourcePrefab.maxDistance = 20;
            AudioSourcePrefab.outputAudioMixerGroup = GameEntry.Instance.MonsterMixer.FindMatchingGroups("Audio")[0];

            BGMSource = Object.Instantiate(AudioSourcePrefab, GameEntry.Instance.transform);
            BGMSource.name = "BGMSource";
            BGMSource.outputAudioMixerGroup = GameEntry.Instance.MonsterMixer.FindMatchingGroups("BGM")[0];

            GameEntry.Data.PlayerPrefsDataMgr.AddEventListener(PlayerPrefsDataMgr.EventName.MasterVolume, RefreshMasterVolume);
            GameEntry.Data.PlayerPrefsDataMgr.AddEventListener(PlayerPrefsDataMgr.EventName.BGMVolume, RefreshBGM);
            GameEntry.Data.PlayerPrefsDataMgr.AddEventListener(PlayerPrefsDataMgr.EventName.AudioVolume, RefreshAudio);
            GameEntry.Data.PlayerPrefsDataMgr.AddEventListener(PlayerPrefsDataMgr.EventName.GamePause, OnGamePause);

            RefreshMasterVolume(null);
            RefreshBGM(null);
            RefreshAudio(null);
        }

        private void RefreshMasterVolume(object userData)
        {
            MasterVolume = GameEntry.Data.PlayerPrefsDataMgr.GetFloat(PlayerPrefsDataMgr.EventName.MasterVolume);
            SetMixerVolume(PlayerPrefsDataMgr.EventName.MasterVolume.ToString(), MasterVolume);
        }
        private void RefreshAudio(object userData)
        {
            PlayerAudioVolume = GameEntry.Data.PlayerPrefsDataMgr.GetFloat(PlayerPrefsDataMgr.EventName.AudioVolume);
            SetMixerVolume(PlayerPrefsDataMgr.EventName.AudioVolume.ToString(), PlayerAudioVolume);
        }
        private void RefreshBGM(object userData)
        {
            PlayerBGMVolume = GameEntry.Data.PlayerPrefsDataMgr.GetFloat(PlayerPrefsDataMgr.EventName.BGMVolume);
            SetMixerVolume(PlayerPrefsDataMgr.EventName.BGMVolume.ToString(), PlayerBGMVolume);
        }
        private void SetMixerVolume(string key, float volume)
        {
            //因为Mixer的音量是-80到20， 而存档里的音量是0-1， 所以这里要做转换
            volume = volume * 100;
            volume -= 80;
            GameEntry.Instance.MonsterMixer.SetFloat(key, volume);
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
        public AudioSource BGMSource { get; private set; }
        public Sys_BGMEntity CurrBGMEntity;
        private TimeAction timeActionIn;
        private TimeAction timeActionOut;

        public void PlayBGM(BGMName audioName)
        {
            Sys_BGMEntity entity = GameEntry.DataTable.Sys_BGMDBModel.GetEntity(audioName.ToString());
            if (CurrBGMEntity != null && CurrBGMEntity.AssetPath == entity.AssetPath)
            {
                return;
            }

            CurrBGMEntity = entity;
            AudioClip audioClip = GameEntry.Resource.LoadMainAsset<AudioClip>(CurrBGMEntity.AssetPath);
            PlayBGM(audioClip, CurrBGMEntity.IsLoop == 1, CurrBGMEntity.IsFadeIn == 1, CurrBGMEntity.Volume);
            GameEntry.Log(LogCategory.Audio, "PlayBGM, Volume=={0}", CurrBGMEntity.Volume);
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
                BGMSource.loop = isLoop;
                BGMSource.volume = entityVolume;
                BGMSource.Play();

                if (isFadeIn)
                {
                    //把音量逐渐变成Max
                    if (timeActionIn != null)
                    {
                        timeActionIn.Stop();
                        timeActionIn = null;
                    }
                    SetMixerVolume(PlayerPrefsDataMgr.EventName.BGMVolume.ToString(), 0);
                    int loopCount = 10;
                    timeActionIn = GameEntry.Time.Create(interval: 0.15f, loop: loopCount, unScaled: true, onUpdate: (int loop) =>
                    {
                        //得到一个0-1的音量值
                        float volume = (loopCount - loop) / loopCount;
                        SetMixerVolume(PlayerPrefsDataMgr.EventName.BGMVolume.ToString(), PlayerBGMVolume * volume);
                    }, onComplete: () =>
                    {
                        timeActionIn = null;
                    });
                }
                else
                {
                    SetMixerVolume(PlayerPrefsDataMgr.EventName.BGMVolume.ToString(), PlayerBGMVolume);
                }
            });
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "PlayBGM");
        }

        internal void StopBGM(Action volumeOut = null)
        {
            if (CurrBGMEntity.IsFadeOut == 0)
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
            int loopCount = 10;
            timeActionOut = GameEntry.Time.Create(interval: 0.15f, loop: loopCount, unScaled: true, onUpdate: (int loop) =>
            {
                //得到一个0-1的音量值
                float volume = (loopCount - loop);
                SetMixerVolume(PlayerPrefsDataMgr.EventName.BGMVolume.ToString(), PlayerBGMVolume * volume);
            }, onComplete: () =>
            {
                timeActionOut = null;
                BGMSource.Stop();
                volumeOut?.Invoke();
            });
            GameEntry.Log(LogCategory.Audio, CurrBGMEntity.Volume + "StopBGM");
        }

        public void PauseBGM(bool isPause)
        {
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
        private AudioSource AudioSourcePrefab;
        private List<AudioSource> AudioSourceList = new List<AudioSource>();

        public void PlayAudio(AudioClip audioClip, Vector3 point, float volume = 1, int priority = 128)
        {
            AudioSource audioSource = PlayAudio2(audioClip, volume, priority);
            if (audioSource == null)
            {
                Debug.LogError("audioSource==null");
                return;
            }
            audioSource.transform.position = point;
            audioSource.spatialBlend = 1;
        }
        public void PlayAudio(AudioClip audioClip, float volume = 1, int priority = 128)
        {
            PlayAudio2(audioClip, volume, priority);
        }
        public void PlayAudio(AudioName audioName, Vector3 point)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName.ToString());
            AudioClip audioClip = GameEntry.Resource.LoadMainAsset<AudioClip>(sys_Audio.AssetPath);
            AudioSource helper = PlayAudio2(audioClip, sys_Audio.Volume, sys_Audio.Priority);
            if (helper == null) return;
            helper.transform.position = point;
            helper.spatialBlend = 1;
        }
        public void PlayAudio(AudioName audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName.ToString());
            AudioClip audioClip = GameEntry.Resource.LoadMainAsset<AudioClip>(sys_Audio.AssetPath);
            PlayAudio2(audioClip, sys_Audio.Volume, sys_Audio.Priority);
        }

        private AudioSource PlayAudio2(AudioClip audioClip, float volume = 1, int priority = 128)
        {
            AudioSource helper = GameEntry.Pool.GameObjectPool.Spawn(AudioSourcePrefab.transform, poolId: 2).GetComponent<AudioSource>();
            AudioSourceList.Add(helper);
            helper.clip = audioClip;
            helper.mute = false;
            helper.volume = volume;
            helper.priority = priority;
            helper.spatialBlend = 0;
            helper.Play();

            if (!helper.loop)
            {
                PoolObj poolObj = helper.GetComponent<PoolObj>();
                poolObj.SetDelayTimeDespawn(audioClip.length);
                poolObj.OnDespawn = () =>
                {
                    AudioSourceList.Remove(helper);
                };
            }
            return helper;
        }

        #endregion

    }
}