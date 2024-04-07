using YouYouMain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YouYouFramework
{
    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioManager
    {
        public float MasterVolume { get; private set; }
        public float BGMVolume { get; private set; }
        public float AudioVolume { get; private set; }

        //目标BGM剪辑
        private AudioClip targetBGMAudioClip;
        //目标BGM是否循环
        private bool targetBGMIsLoop;
        //目标BGM音量
        private float targetBGMClipVolume;

        //BGM淡入淡出过渡音量
        private float interimBGMVolume;

        //是否需要淡入淡出
        private bool isFadeIn;
        private bool isFadeOut;

        public void Init()
        {
            AudioSourcePrefab = new GameObject("AudioSource", typeof(AudioSource), typeof(AutoDespawnHandle)).GetComponent<AudioSource>();
            AudioSourcePrefab.transform.SetParent(GameEntry.Instance.transform);
            AudioSourcePrefab.playOnAwake = false;
            AudioSourcePrefab.maxDistance = 20;
            AudioSourcePrefab.outputAudioMixerGroup = GameEntry.Instance.MonsterMixer.FindMatchingGroups("Audio")[0];

            BGMSource = Object.Instantiate(AudioSourcePrefab, GameEntry.Instance.transform);
            BGMSource.name = "BGMSource";
            BGMSource.outputAudioMixerGroup = GameEntry.Instance.MonsterMixer.FindMatchingGroups("BGM")[0];

            GameEntry.PlayerPrefs.SetFloatHas(PlayerPrefsConstKey.MasterVolume, 1);
            GameEntry.PlayerPrefs.SetFloatHas(PlayerPrefsConstKey.AudioVolume, 1);
            GameEntry.PlayerPrefs.SetFloatHas(PlayerPrefsConstKey.BGMVolume, 1);
            GameEntry.PlayerPrefs.SetBoolHas(PlayerPrefsConstKey.MasterMute, false);

            SetMasterVolume(GameEntry.PlayerPrefs.GetFloat(PlayerPrefsConstKey.MasterVolume));
            SetAudioVolume(GameEntry.PlayerPrefs.GetFloat(PlayerPrefsConstKey.AudioVolume));
            SetBGMVolume(GameEntry.PlayerPrefs.GetFloat(PlayerPrefsConstKey.BGMVolume));

            SetMasterMute(GameEntry.PlayerPrefs.GetBool(PlayerPrefsConstKey.MasterMute));
        }
        public void OnUpdate()
        {
            if (BGMSource.clip != targetBGMAudioClip)
            {
                //淡出
                if (isFadeOut && interimBGMVolume > 0)
                {
                    interimBGMVolume -= Time.unscaledDeltaTime * 2;//0.5秒淡出时间
                    SetBGMVolume(BGMVolume);//这里是为了刷新音量
                }
                else
                {
                    //淡出完毕, 或者不需要淡出
                    if (targetBGMAudioClip != null)
                    {
                        //播放新的背景音乐
                        BGMSource.clip = targetBGMAudioClip;
                        BGMSource.loop = targetBGMIsLoop;
                        BGMSource.volume = targetBGMClipVolume;
                        BGMSource.Play();

                        interimBGMVolume = 0;
                        SetBGMVolume(BGMVolume);//这里是为了刷新音量

                    }
                    else
                    {
                        //没新的背景音乐 停止播放
                        BGMSource.Stop();
                        BGMSource.clip = null;
                    }
                }
            }
            else
            {
                //淡入
                if (isFadeIn && interimBGMVolume < 1)
                {
                    interimBGMVolume += Time.unscaledDeltaTime * 2;//0.5秒淡入时间
                    SetBGMVolume(BGMVolume);//这里是为了刷新音量
                }
                else if (interimBGMVolume != 1)
                {
                    interimBGMVolume = 1;
                    SetBGMVolume(BGMVolume);//这里是为了刷新音量
                }
            }

        }

        #region BGM
        public AudioSource BGMSource { get; private set; }
        public Sys_BGMEntity CurrBGMEntity;

        public void PlayBGM(string audioName)
        {
            Sys_BGMEntity entity = GameEntry.DataTable.Sys_BGMDBModel.GetEntity(audioName);
            if (entity == null)
            {
                GameEntry.LogError(LogCategory.Audio, "CurrBGMEntity==null, audioName==" + audioName);
                return;
            }

            CurrBGMEntity = entity;
            AudioClip audioClip = GameEntry.Loader.LoadMainAsset<AudioClip>(CurrBGMEntity.AssetFullPath, BGMSource.gameObject);
            PlayBGM(audioClip, CurrBGMEntity.IsLoop == 1, CurrBGMEntity.Volume, CurrBGMEntity.IsFadeIn == 1, CurrBGMEntity.IsFadeOut == 1);
        }
        public void PlayBGM(AudioClip audioClip, bool isLoop, float volume, bool isFadeIn, bool isFadeOut)
        {
            if (audioClip == null)
            {
                GameEntry.LogError(LogCategory.Audio, "audioClip==null");
                return;
            }
            targetBGMAudioClip = audioClip;
            targetBGMIsLoop = isLoop;
            targetBGMClipVolume = volume;
            this.isFadeIn = isFadeIn;
            this.isFadeOut = isFadeOut;

            GameEntry.Log(LogCategory.Audio, string.Format("PlayBGM, audioClip=={0}, volume=={1}", audioClip, volume));
        }

        internal void StopBGM(bool isFadeOut)
        {
            //把音量逐渐变成0 再停止
            targetBGMAudioClip = null;
            this.isFadeOut = isFadeOut;
            GameEntry.Log(LogCategory.Audio, "StopBGM");
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
            GameEntry.Log(LogCategory.Audio, BGMSource.clip + "PauseBGM");

        }
        #endregion

        #region 音效
        private AudioSource AudioSourcePrefab;
        private List<AudioSource> AudioSourceList = new List<AudioSource>();

        public void PlayAudio(AudioClip audioClip, float volume = 1, int priority = 128)
        {
            PlayAudio2(audioClip, volume, priority);
        }
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
        public void PlayAudio(string audioName, Vector3 point)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName);
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(sys_Audio.AssetFullPath);
            AudioSource helper = PlayAudio2(referenceEntity.Target as AudioClip, sys_Audio.Volume, sys_Audio.Priority);
            if (helper != null)
            {
                AutoReleaseHandle.Add(referenceEntity, helper.gameObject);
                helper.transform.position = point;
                helper.spatialBlend = 1;
            }
        }
        public void PlayAudio(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName);
            AssetReferenceEntity referenceEntity = GameEntry.Loader.LoadMainAsset(sys_Audio.AssetFullPath);
            AudioSource helper = PlayAudio2(referenceEntity.Target as AudioClip, sys_Audio.Volume, sys_Audio.Priority);
            if (helper != null)
            {
                AutoReleaseHandle.Add(referenceEntity, helper.gameObject);
            }
        }

        private AudioSource PlayAudio2(AudioClip audioClip, float volume = 1, int priority = 128)
        {
            AudioSource helper = GameEntry.Pool.GameObjectPool.Spawn(AudioSourcePrefab.gameObject, poolId: 2).GetComponent<AudioSource>();
            AudioSourceList.Add(helper);
            helper.clip = audioClip;
            helper.mute = false;
            helper.volume = volume;
            helper.priority = priority;
            helper.spatialBlend = 0;
            helper.Play();

            if (!helper.loop)
            {
                AutoDespawnHandle poolObj = helper.gameObject.GetOrCreatComponent<AutoDespawnHandle>();
                poolObj.SetDelayTimeDespawn(audioClip.length);
                poolObj.OnDespawn = () =>
                {
                    AudioSourceList.Remove(helper);
                };
            }
            return helper;
        }

        #endregion

        public void SetMasterVolume(float volume)
        {
            MasterVolume = volume;
            SetMixerVolume(PlayerPrefsConstKey.MasterVolume, MasterVolume);
            GameEntry.PlayerPrefs.SetFloat(PlayerPrefsConstKey.MasterVolume, MasterVolume);
        }
        public void SetAudioVolume(float volume)
        {
            AudioVolume = volume;
            SetMixerVolume(PlayerPrefsConstKey.AudioVolume, AudioVolume);
            GameEntry.PlayerPrefs.SetFloat(PlayerPrefsConstKey.AudioVolume, AudioVolume);
        }
        public void SetBGMVolume(float volume)
        {
            BGMVolume = volume;
            SetMixerVolume(PlayerPrefsConstKey.BGMVolume, BGMVolume * interimBGMVolume);
            GameEntry.PlayerPrefs.SetFloat(PlayerPrefsConstKey.BGMVolume, BGMVolume);
        }
        private void SetMixerVolume(string key, float volume)
        {
            //因为Mixer内我们要修改的音量是db， 而存档里的音量是0-1形式的百分比， 所以这里要做转换
            volume = (float)(20 * Math.Log10(volume / 1));
            GameEntry.Instance.MonsterMixer.SetFloat(key, volume);
        }

        /// <summary>
        /// 设置根节点(全部)静音
        /// </summary>
        public void SetMasterMute(bool mute)
        {
            SetMasterVolume(mute ? 0 : GameEntry.PlayerPrefs.GetFloat(PlayerPrefsConstKey.MasterVolume));
            GameEntry.PlayerPrefs.SetBool(PlayerPrefsConstKey.MasterMute, mute);
        }
    }
}