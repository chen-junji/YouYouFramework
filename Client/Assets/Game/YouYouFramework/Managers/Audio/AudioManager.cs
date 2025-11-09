using YouYouMain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using YooAsset;

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


        public AudioManager()
        {
            audioRoutinePrefab = new GameObject("AudioSource", typeof(AudioRoutine), typeof(GameObjectDespawnHandle), typeof(AudioSource)).GetComponent<AudioRoutine>();
            audioRoutinePrefab.Init();
            audioRoutinePrefab.transform.SetParent(GameEntry.Instance.transform);
            audioRoutinePrefab.AudioSource.playOnAwake = false;
            audioRoutinePrefab.AudioSource.maxDistance = 20;
            audioRoutinePrefab.AudioSource.outputAudioMixerGroup = GameEntry.Instance.MasterMixer.FindMatchingGroups("Audio")[0];

            BGMSource = new GameObject("BGMSource", typeof(AudioSource)).GetComponent<AudioSource>();
            BGMSource.transform.SetParent(GameEntry.Instance.transform);
            BGMSource.outputAudioMixerGroup = GameEntry.Instance.MasterMixer.FindMatchingGroups("BGM")[0];
        }
        public void Init()
        {
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
            OnBGMUpdate();
        }

        #region BGM
        public AudioSource BGMSource { get; private set; }

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

        private AssetHandle currBGMHandle;

        public async void PlayBGM(string audioName)
        {
            Sys_BGMEntity entity = GameEntry.DataTable.Sys_BGMDBModel.GetEntity(audioName);
            if (entity == null)
            {
                GameEntry.LogError(LogCategory.Audio, "CurrBGMEntity==null, audioName==" + audioName);
                return;
            }

            var operation = GameEntry.Loader.DefaultPackage.LoadAssetAsync(entity.AssetFullPath);
            await operation.Task;
            PlayBGM(operation.AssetObject as AudioClip, entity.IsLoop == 1, entity.Volume, entity.IsFadeIn == 1, entity.IsFadeOut == 1, operation);
        }
        public void PlayBGM(AudioClip audioClip, bool isLoop, float volume, bool isFadeIn, bool isFadeOut, AssetHandle assetHandle)
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

            //这里要保证BGM过渡时间小于Asset池释放时间, 否则会在过渡到一半的时候AudioClip就变成null了, 但一般情况下是不会出问题的
            if (currBGMHandle != null)
            {
                currBGMHandle.Release();
                currBGMHandle = null;
            }
            currBGMHandle = assetHandle;

            GameEntry.Log(LogCategory.Audio, string.Format("PlayBGM, audioClip=={0}, volume=={1}", audioClip, volume));
        }

        public void StopBGM(bool isFadeOut)
        {
            //把音量逐渐变成0 再停止
            targetBGMAudioClip = null;
            this.isFadeOut = isFadeOut;

            //这里要保证BGM过渡时间小于Asset池释放时间, 否则会在过渡到一半的时候AudioClip就变成null了, 但一般情况下是不会出问题的
            if (currBGMHandle != null)
            {
                currBGMHandle.Release();
                currBGMHandle = null;
            }

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

        private void OnBGMUpdate()
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
        #endregion

        #region 音效
        private AudioRoutine audioRoutinePrefab;

        public async void PlayAudio(string audioName, Vector3 point)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName);
            var operation = GameEntry.Loader.DefaultPackage.LoadAssetAsync(sys_Audio.AssetFullPath);
            await operation.Task;

            AudioRoutine routine = CreateAudioRoutine(operation.AssetObject as AudioClip, sys_Audio.Volume, sys_Audio.Priority);
            routine.AudioSource.spatialBlend = 1;
            routine.transform.position = point;

            //音效资源做引用计数
            routine.AutoDespawnHandle.OnDespawn += () =>
            {
                operation.Release();
            };
        }
        public async void PlayAudio(string audioName)
        {
            Sys_AudioEntity sys_Audio = GameEntry.DataTable.Sys_AudioDBModel.GetEntity(audioName);
            var operation = GameEntry.Loader.DefaultPackage.LoadAssetAsync(sys_Audio.AssetFullPath);
            await operation.Task;

            AudioRoutine routine = CreateAudioRoutine(operation.AssetObject as AudioClip, sys_Audio.Volume, sys_Audio.Priority);
            routine.AudioSource.spatialBlend = 0;

            //音效资源做引用计数
            routine.AutoDespawnHandle.OnDespawn += () =>
            {
                operation.Release();
            };
        }
        public void PlayAudio(AudioClip audioClip, Vector3 point, float volume = 1, int priority = 128)
        {
            AudioRoutine routine = CreateAudioRoutine(audioClip, volume, priority);
            routine.AudioSource.spatialBlend = 1;
            routine.transform.position = point;
        }
        public void PlayAudio(AudioClip audioClip, float volume = 1, int priority = 128)
        {
            AudioRoutine routine = CreateAudioRoutine(audioClip, volume, priority);
            routine.AudioSource.spatialBlend = 0;
        }

        private AudioRoutine CreateAudioRoutine(AudioClip audioClip, float volume, int priority)
        {
            AudioRoutine routine = GameEntry.Pool.GameObjectPool.Spawn(audioRoutinePrefab.gameObject, SpawnPoolId.Audio).GetComponent<AudioRoutine>();
            routine.AudioSource.clip = audioClip;
            routine.AudioSource.volume = volume;
            routine.AudioSource.priority = priority;
            routine.AudioSource.Play();

            //这里需要注意: 如果AudioSource.loop==true则会有问题, 循环播放的音效不需要使用AudioManager, 自己挂AudioSource去播放就行了
            routine.AutoDespawnHandle.SetDelayTimeDespawn(audioClip.length);
            routine.AutoDespawnHandle.OnDespawn += () =>
            {
                routine.AudioSource.clip = null;
            };
            return routine;
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
            //如果传进来的值就是0 那么直接转成-80
            if (volume < 0.01f)
            {
                volume = -80;
                //Debug.LogError($"key=={key}, volume=={volume}");
                GameEntry.Instance.MasterMixer.SetFloat(key, volume);
            }
            else
            {
                //因为Mixer内我们要修改的音量是db， 而存档里的音量是0-1形式的百分比， 所以这里要做转换
                volume = (float)(20 * Math.Log10(volume / 1));
                //Debug.LogError($"key=={key}, volume=={volume}");
                GameEntry.Instance.MasterMixer.SetFloat(key, volume);
            }
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