using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYouFramework;

public class TestTask : MonoBehaviour
{
    private TaskGroup taskGroup;

    private void OnDestroy()
    {
        if (taskGroup != null)
        {
            taskGroup.Dispose();
            taskGroup = null;
        }
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            CreateTaskGroup();
            if (!taskGroup.InTask)
            {
                //并行执行
                taskGroup.Run(true);
            }
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            CreateTaskGroup();
            if (!taskGroup.InTask)
            {
                //顺序执行
                taskGroup.Run(false);
            }
        }
    }


    private void CreateTaskGroup()
    {
        if (taskGroup != null)
        {
            taskGroup.Dispose();
            taskGroup = null;
        }

        taskGroup = TaskManager.Instance.CreateTaskGroup();
        taskGroup.AddTask(async (TaskRoutine taskRoutine) =>
        {
            Debug.Log(1111);
            await GameEntry.Time.Delay(this, 1);
            Debug.Log(2222);
            taskRoutine.TaskComplete();
        });
        taskGroup.AddTask(async (TaskRoutine taskRoutine) =>
        {
            Debug.Log(3333);
            await GameEntry.Time.Delay(this, 1);
            Debug.Log(4444);
            taskRoutine.TaskComplete();
        });
    }
}
