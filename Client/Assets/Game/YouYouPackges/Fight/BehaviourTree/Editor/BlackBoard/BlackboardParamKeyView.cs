using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

/// <summary>
/// 黑板参数key视图
/// </summary>
public class BlackboardParamKeyView : BlackboardField
{
    private BehaviourTreeGraphView graphView;

    private TextField keyField;
        
    public BlackboardParamKeyView(BehaviourTreeGraphView graphView,BBParam param,string key,string typeName) :base(null,key,typeName)
    {
        this.graphView = graphView;
        this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));  //增加右键菜单
        var icon = this.Q("icon");
        icon.AddToClassList("parameter-" + typeName);
        icon.visible = true;
        icon.style.width = 4;
        icon.style.height = 4;
        keyField = this.Q<TextField>("textField");
        keyField.RegisterValueChangedCallback((e) =>
        {
            //重命名了黑板key
            string oldKey = e.previousValue;
            string newKey = e.newValue;
            if (graphView.RenameBlackBoardParam(oldKey,newKey,param))
            {
                text = e.newValue;
            }
        });
            
    }

    /// <summary>
    /// 构建右键菜单
    /// </summary>
    private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.RemoveItemAt(0);
        evt.menu.AppendAction("重命名", (a) => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
        evt.menu.AppendAction("删除", (a) => graphView.RemoveBlackBoardParam(text), DropdownMenuAction.AlwaysEnabled);

        evt.StopPropagation();
    }
}