using UnityEngine;
using System.Threading.Tasks;
using YouYou;


public class UIFormBase : UIBase
{
    [Header("是否窗口动画")]
    [SerializeField] bool isAnim = false;

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
#if UNITY_EDITOR
        transform.SetAsLastSibling();
#endif
        if (isAnim) AnimOpen();
    }
    protected override void OnClose()
    {
        base.OnClose();
        if (isAnim) AnimClose();
    }
    public void AnimOpen()
    {
    }
    public void AnimClose()
    {
    }
}