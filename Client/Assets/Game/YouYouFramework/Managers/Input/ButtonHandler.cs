using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YouYouFramework
{
    public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] InputName Name;
        public void OnPointerDown(PointerEventData eventData)
        {
            GameEntry.Input.SetButtonDown(Name);
            GameEntry.Input.Dispatch((int)Name);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            GameEntry.Input.SetButtonUp(Name);
        }
    }
}
