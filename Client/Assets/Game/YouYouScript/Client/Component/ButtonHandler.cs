using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] InputKeyCode Name;
    public void OnPointerDown(PointerEventData eventData)
    {
        InputManager.Instance.SetButtonDown(Name);
        InputManager.Instance.ActionInput?.Invoke(Name);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        InputManager.Instance.SetButtonUp(Name);
    }
}
