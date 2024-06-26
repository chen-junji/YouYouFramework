using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace YouYouFramework
{
    public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] InputKeyCode Name;
        public void OnPointerDown(PointerEventData eventData)
        {
            GameEntry.Input.SetButtonDown(Name);
            GameEntry.Input.ActionInput?.Invoke(Name);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            GameEntry.Input.SetButtonUp(Name);
        }
    }
}
