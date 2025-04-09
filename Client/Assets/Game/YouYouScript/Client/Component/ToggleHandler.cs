using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYouFramework;


[RequireComponent(typeof(Toggle))]
public class ToggleHandler : MonoBehaviour
{
    [SerializeField] InputKeyCode Name;

    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                InputManager.Instance.SetButtonDown(Name);
                InputManager.Instance.ActionInput?.Invoke(Name);
            }
            else
            {
                InputManager.Instance.SetButtonUp(Name);
            }
        });
    }
}