using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;

[RequireComponent(typeof(Toggle))]
public class ToggleHandler : MonoBehaviour
{
    [SerializeField] InputKey Name;

    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                GameEntry.Input.SetButtonDown(Name);
                GameEntry.Input.Dispatch((int)Name);
            }
            else
            {
                GameEntry.Input.SetButtonUp(Name);
            }
        });
    }
}