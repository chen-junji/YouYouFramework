using UnityEngine.UIElements;

/// <summary>
/// 黑板参数值视图
/// </summary>
public class BlackboardParamValueView : VisualElement
{
    /// <summary>
    /// 绘制黑板值
    /// </summary>
    public void DrawValue(BBParam bbParam)
    {
        Clear();
            
        if (BaseBBParamDrawer.BBParamDrawerDict.TryGetValue(bbParam.GetType(),out var drawer))
        {
            IMGUIContainer imguiContainer = new IMGUIContainer(){};
            imguiContainer.onGUIHandler =  (() =>
            {
                drawer.Target = bbParam;
                drawer.OnGUI(null,false);
            });
            Add(imguiContainer);
        }
    }
}