//===================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestAudio : MonoBehaviour
{
    public Transform target;

    public List<int> aaa;

    void Start()
    {

    }

    int serialId;
    float value = 0;
    bool begin = false;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            //serialId = GameEntry.Audio.FMOD.PlayAudio(601, pos3D: target.position);
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            begin = true;

        }

        if (begin)
        {
            value += Time.deltaTime * 10;
            value = Mathf.Min(value, 300);
            Debug.LogError("value==" + value);
            //GameEntry.Audio.FMOD.SetParameterForAudio(serialId, "Speed", value);
        }

    }
}