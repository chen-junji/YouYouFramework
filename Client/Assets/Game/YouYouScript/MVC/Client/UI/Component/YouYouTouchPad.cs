using UnityEngine;
using System.Linq;
using YouYou;
using UnityEngine.EventSystems;

public class YouYouTouchPad : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector2 input;

    public void OnDrag(PointerEventData eventData)
    {
        input = eventData.delta;
        GameEntry.Input.SetAxis(InputConst.MouseX, input.x);
        GameEntry.Input.SetAxis(InputConst.MouseY, input.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        input = Vector2.zero;
        GameEntry.Input.SetAxis(InputConst.MouseX, input.x);
        GameEntry.Input.SetAxis(InputConst.MouseY, input.y);
    }

    public void SetEnabled(bool enabled)
    {
        if (!enabled)
        {
            input = Vector2.zero;
            GameEntry.Input.SetAxis(InputConst.MouseX, input.x);
            GameEntry.Input.SetAxis(InputConst.MouseY, input.y);
        }
        this.enabled = enabled;
    }
}