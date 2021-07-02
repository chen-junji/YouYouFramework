using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

[DisallowMultipleComponent]
[RequireComponent(typeof(Toggle))]//脚本依赖
public class YouYouToggle : MonoBehaviour
{
    [SerializeField] private bool IsOffPlay;

    [SerializeField] private int[] AudioId = new int[] { };
    private int id;
    void Start()
    {
        if (AudioId.Length == 0)
        {
            id = CommonConst.SoundClick;
        }
        else
        {
            id = AudioId[Random.Range(0, AudioId.Length)];
            if (GameEntry.DataTable.Sys_AudioDBModel.GetDic(id) == null) id = CommonConst.SoundClick;
        }
        GetComponent<Toggle>().onValueChanged.AddListener((isOn) =>
        {
            if (IsOffPlay)
            {
                GameEntry.Audio.PlayAudio(id);
            }
            else
            {
                if (isOn) GameEntry.Audio.PlayAudio(id);
            }
        });
    }
}
