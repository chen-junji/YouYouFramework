using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class MainForm : UIFormBase
{
    [SerializeField] List<Button> btnList;

    protected override void Awake()
    {
        base.Awake();
        for (int i = 0; i < btnList.Count; i++)
        {
            Button button = btnList[i];
            button.onClick.AddListener(() =>
            {
                GameEntry.Procedure.ChangeState(ProcedureState.None);
                GameEntry.UI.OpenUIForm<LoadingForm>();
                GameEntry.Scene.LoadSceneAction(button.GetComponentInChildren<Text>().text);
            });
        }
    }
}
