using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class MainForm : UIFormBase
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
                    GameEntry.Procedure.ChangeState(ProcedureState.None);
                    GameEntry.UI.OpenUIForm<LoadingForm>();
                    GameEntry.Scene.LoadSceneAction(button.GetComponentInChildren<Text>().text);
                });
            }
        }
    }
}
