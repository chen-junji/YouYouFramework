using System.Collections.Generic;
using YouYouFramework;

public class CircleCtrl
{
    public static CircleCtrl Instance { get; private set; } = new();

    private int showCount = 0;

    public void CircleOpen(string dialogKey = null)
    {
        if (showCount < 0)
        {
            GameEntry.LogError("调用错误");
            return;
        }
        showCount++;
        if (showCount == 1)
        {
            var circleForm = GameEntry.UI.OpenUIForm<CircleForm>();
            circleForm.SetText(string.Empty); //如果不设置要设为空

            if (!string.IsNullOrEmpty(dialogKey) && GameEntry.DataTable.Sys_DialogDBModel.keyDic.TryGetValue(dialogKey, out var entity))
            {
                circleForm.SetText(entity.Content);
            }
        }
    }
    public void CircleClose()
    {
        if (showCount == 0)
        {
            GameEntry.LogError("调用错误");
            return;
        }
        showCount--;
        if (showCount == 0)
        {
            GameEntry.UI.CloseUIForm<CircleForm>();
        }
    }

    private Dictionary<string, TimeAction> dicTimeOut = new();
    public void CircleOpen(string timeoutKey, float timeout, string dialogKey = null)
    {
        if (dicTimeOut.ContainsKey(timeoutKey))
        {
            GameEntry.LogError("不允许同一个Key同时Open");
            return;
        }

        if (timeout > 0)
        {
            var timer = GameEntry.Time.CreateTimer(this, timeout, () =>
            {
                CircleClose(timeoutKey);
            }, true);
            dicTimeOut.Add(timeoutKey, timer);
            CircleOpen(dialogKey);
        }
    }
    public void CircleClose(string timeoutKey)
    {
        if (dicTimeOut.TryGetValue(timeoutKey, out var timer))
        {
            CircleClose();
            dicTimeOut.Remove(timeoutKey);
            timer.Stop();
        }
    }
}
