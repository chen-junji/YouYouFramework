
using UnityEngine;
using UnityEngine.UI;

public class UIScrollIndex : MonoBehaviour
{
    public int Index { get; private set; }

    public void SetUI(int value, Vector3 pos)
    {
        Index = value;
        transform.localPosition = pos;

        //这里性能不好, 注释掉
        //gameObject.name = "Scroll" + (Index < 10 ? "0" + Index : Index.ToString());
    }
}