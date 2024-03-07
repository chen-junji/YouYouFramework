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

        public void AddGuide(Action onEnter)
        {
            TaskGroup.AddTask(taskRoutine =>
            {
                onEnter?.Invoke();
            });
        }

        public void AddGuide(GuideRoutine guideRoutine)
        {
            TaskGroup.AddTask(taskRoutine =>
            {
                taskRoutine.OnComplete += () =>
                {
                    guideRoutine.OnExit?.Invoke();
                };
                guideRoutine.OnEnter?.Invoke();
            });
        }

        public void Run(Action onComplete = null)
        {
            TaskGroup.OnComplete = () =>
            {
                onComplete?.Invoke();
                GameEntry.Log(LogCategory.Guide, "GroupComplete:" + GuideCtrl.Instance.CurrentState);
                GuideCtrl.Instance.OnStateEnter(GuideState.None);
            };
            TaskGroup.Run();
        }
    }
}