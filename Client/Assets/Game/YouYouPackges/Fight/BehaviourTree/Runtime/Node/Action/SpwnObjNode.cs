using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YouYouFramework;

/// <summary>
/// 取对象
/// </summary>
[NodeInfo(Name = "内置/取对象")]
public class SpwnObjNode : BaseActionNode
{
    private static FieldInfo[] fieldInfos = typeof(SpwnObjNode).GetFields(BindingFlags.Public | BindingFlags.Instance);

    /// <inheritdoc />
    public override FieldInfo[] FieldInfos => fieldInfos;

    /// <summary>
    /// 要取的对象名称
    /// </summary>
    [BBParamInfo(Name = "要取的对象名称")]
    public BBParamString SpwnObjName;

    protected override void OnStart()
    {
        TimelineCtrl timelineCtrl = Owner.RoleCtrl.CreateSkillTimeLine(SpwnObjName.Value);
        GameEntry.Log(LogCategory.Skill, "开始释放技能 Skill1");
        timelineCtrl.OnStopped = () =>
        {
            Finish(true);
            GameEntry.Log(LogCategory.Skill, "技能释放完毕 Skill1");

            Owner.Restart();
            GameEntry.Log(LogCategory.Skill, "重启行为树");
        };
    }

    protected override void OnCancel()
    {

    }

}
