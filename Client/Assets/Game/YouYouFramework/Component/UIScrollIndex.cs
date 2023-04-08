
using UnityEngine;
using UnityEngine.UI;

public class UIScrollIndex : MonoBehaviour
{
    public int Index { get; private set; }

    /// <summary>
    /// ¸üÐÂItemµÄ×ø±ê
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pos"></param>
    public void SetUI(int value, Vector3 pos)
    {
        Index = value;
        transform.localPosition = pos;
        //gameObject.name = "Scroll" + (Index < 10 ? "0" + Index : Index.ToString());
    }
}