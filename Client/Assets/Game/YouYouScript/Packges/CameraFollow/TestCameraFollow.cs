using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestCameraFollow : MonoBehaviour
{
    void Update()
    {
        //Camera跟随
        CameraFollowCtrl.Instance.transform.position = transform.position;

        //Camera旋转
        if (Input.GetKey(KeyCode.W))
        {
            //这个测试用例不存在GameEntry,  不然也可以使用GameEntry.Input.GetAxis(InputConst.MouseY)
            CameraFollowCtrl.Instance.SetCameraRotateY(Time.deltaTime * 1000);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            CameraFollowCtrl.Instance.SetCameraRotateY(-Time.deltaTime * 1000);
        }
        if (Input.GetKey(KeyCode.A))
        {
            CameraFollowCtrl.Instance.SetCameraRotateX(Time.deltaTime * 1000);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            CameraFollowCtrl.Instance.SetCameraRotateX(-Time.deltaTime * 1000);
        }

        //Camera缩放
        if (Input.GetKey(KeyCode.Q))
        {
            CameraFollowCtrl.Instance.SetCameraDistance(Time.deltaTime * 10);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            CameraFollowCtrl.Instance.SetCameraDistance(-Time.deltaTime * 10);
        }
    }
}
