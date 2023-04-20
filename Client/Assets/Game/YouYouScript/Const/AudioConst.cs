using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public struct BGMEntity
{
    public BGMName BGMName;

    /// <summary>
    /// 路径
    /// </summary>
    public string AssetPath;

    /// <summary>
    /// 音量（0-1）
    /// </summary>
    public float Volume;

    /// <summary>
    /// 是否循环
    /// </summary>
    public bool IsLoop;

    /// <summary>
    /// 是否淡入
    /// </summary>
    public bool IsFadeIn;

    /// <summary>
    /// 是否淡出
    /// </summary>
    public bool IsFadeOut;

    /// <summary>
    /// 优先级(默认128)
    /// </summary>
    public byte Priority;

    public BGMEntity(BGMName bgmName, string assetPath, float volume = 0.2f, bool isLoop = true, bool isFadeIn = true, bool isFadeOut = true, byte priorty = 128)
    {
        BGMName = bgmName;
        AssetPath = assetPath;
        Volume = volume;
        IsLoop = isLoop;
        IsFadeIn = isFadeIn;
        IsFadeOut = isFadeOut;
        Priority = priorty;
    }
}
public struct AudioEnity
{
    public AudioName AudioName;

    /// <summary>
    /// 路径
    /// </summary>
    public string AssetPath;

    /// <summary>
    /// 音量（0-1）
    /// </summary>
    public float Volume;

    /// <summary>
    /// 是否循环
    /// </summary>
    public bool IsLoop;

    /// <summary>
    /// 优先级(默认128)
    /// </summary>
    public byte Priority;

    public AudioEnity(AudioName audioName, string assetPath, float volume = 1, bool isLoop = false, byte priorty = 128)
    {
        AudioName = audioName;
        AssetPath = assetPath;
        Volume = volume;
        IsLoop = isLoop;
        Priority = priorty;
    }
}

public enum BGMName : uint
{
    None,
    BGM,
}
public enum AudioName : uint
{
    button_sound,
    UIOpen,
}
public class AudioConst
{
    public static Dictionary<BGMName, BGMEntity> BGMDic = new Dictionary<BGMName, BGMEntity>();
    public static Dictionary<AudioName, AudioEnity> AudioDic = new Dictionary<AudioName, AudioEnity>();

    public AudioConst()
    {
        void AddBGMDic(BGMEntity audioEnity)
        {
            BGMDic.Add(audioEnity.BGMName, audioEnity);
        }
        void AddAudioDic(AudioEnity audioEnity)
        {
            AudioDic.Add(audioEnity.AudioName, audioEnity);
        }

        //配置背景音乐
        AddBGMDic(new BGMEntity(BGMName.BGM, "BGM"));

        //配置音效
        AddAudioDic(new AudioEnity(AudioName.button_sound, "UI/button_sound.mp3"));
        AddAudioDic(new AudioEnity(AudioName.UIOpen, "UI/UIOpen.mp3"));
    }

    public static BGMEntity GetDic(BGMName audioName)
    {
        return BGMDic[audioName];
    }
    public static AudioEnity GetAudio(AudioName audioName)
    {
        return AudioDic[audioName];
    }
}
