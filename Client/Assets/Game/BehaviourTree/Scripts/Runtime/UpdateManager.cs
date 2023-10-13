using System;
using System.Collections.Generic;

/// <summary>
/// 定时器管理器
/// </summary>
public static class UpdateManager
{
    private static List<Action<float>> timers = new List<Action<float>>();

    public static void AddUpdateTimer(Action<float> timer)
    {
        timers.Add(timer);
    }
        
    public static void RemoveUpdateTimer(Action<float> timer)
    {
        timers.Remove(timer);
    }
        
    public static void OnUpdate(float deltaTime)
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            var timer = timers[i];
            timer?.Invoke(deltaTime);
        }
    }
}