using UnityEngine;
using System.Threading.Tasks;
using YouYou;


public class UIFormBase : UIBase
{
    [Header("是否窗口动画")]
    [SerializeField] bool isAnim = false;

    protected override void OnEnable()
    {
        base.OnEnable();
#if UNITY_EDITOR
        transform.SetAsLastSibling();
#endif
        if (isAnim) AnimOpen();
    }
    public void AnimOpen()
    {
        transform.DoShowScale(0.3f, 1);
    }
}