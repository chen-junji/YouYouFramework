using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

[RequireComponent(typeof(Toggle))]
public class ToggleHandler : MonoBehaviour
{
    [SerializeField] InputName Name;

    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                GameEntry.Input.SetButtonDown(Name);
                GameEntry.Event.Common.Dispatch((int)Name);//ToggleIsOn
            }
            else
            {
                GameEntry.Input.SetButtonUp(Name);
            }
        });
    }
}