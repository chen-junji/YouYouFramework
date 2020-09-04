//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
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
            serialId = GameEntry.Audio.PlayAudio(601, pos3D: target.position);
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
            GameEntry.Audio.SetParameterForAudio(serialId, "Speed", value);
        }

    }
}