using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class GuideGroup
    {
        public TaskGroup TaskGroup { get; private set; }

        public GuideGroup()
        {
            TaskGroup = new TaskGroup();
        }

        public void AddGuide(Action task)
        {
            TaskGroup.AddTask(taskRoutine =>
            {
                GameEntry.Guide.OnNextOne = taskRoutine.Leave;
                task?.Invoke();
            });
        }

        public void Run(Action onComplete = null)
        {
            TaskGroup.OnComplete = () =>
            {
                onComplete?.Invoke();
                GameEntry.Log(LogCategory.Guide, "GroupComplete:" + GameEntry.Guide.CurrentState);
                GameEntry.Guide.OnStateEnter(GuideState.None);
            };
            TaskGroup.Run();
        }
    }
}