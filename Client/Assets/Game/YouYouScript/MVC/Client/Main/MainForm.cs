using YouYouMain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

public partial class MainForm : UIFormBase
{
    [SerializeField] Transform BtnGroup;

    protected override void Awake()
    {
        base.Awake();
        foreach (Transform child in BtnGroup)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    if (MainEntry.IsAssetBundleMode)
                    {
                        GameEntry.LogError(LogCategory.Framework, "当前是AssetBundle加载模式，不支持加载YouYouTest文件夹内的测试场景， 因为它们没有放在Download文件夹内打包，所以不支持热更新和AssetBundle加载");
                        return;
                    }
                    GameEntry.UI.CloseAllDefaultUIForm();
                    GameEntry.UI.OpenUIForm<LoadingForm>();
                    GameEntry.Scene.LoadSceneAction(button.GetComponentInChildren<Text>().text);
                });
            }
        }
    }
}
