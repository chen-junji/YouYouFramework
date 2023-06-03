using System;
using UnityEngine;
using UnityEngine.AI;
using YouYou;

public class RoleCtrl : MonoBehaviour
{
    /// <summary>
    /// 当前角色控制器
    /// </summary>
    public RoleView RoleView { get; private set; }

    protected virtual void Awake()
    {
        RoleView = GetComponent<RoleView>();
    }

    public virtual TimelineCtrl CreateSkillTimeLine(string prefabName)
    {
        TimelineCtrl timelineCtrl = GameEntry.Pool.GameObjectPool.Spawn(prefabName).GetComponent<TimelineCtrl>();
        timelineCtrl.transform.position = transform.position;
        timelineCtrl.transform.rotation = transform.rotation;

        timelineCtrl.PlayAnim = (args) =>
        {
            RoleView.AnimCompoent.PlayAnim(args.AnimationClip);
        };
        timelineCtrl.PlayResource = (args, delayTime) =>
        {
            PoolObj poolObj = GameEntry.Pool.GameObjectPool.Spawn(args.PrefabName);
            poolObj.transform.SetPositionAndRotation(transform.position, transform.rotation);
            poolObj.SetDelayTimeDespawn(delayTime);
        };
        timelineCtrl.PlaySound = (args) =>
        {
            GameEntry.Audio.PlayAudio(args.AudioClip, transform.position);
        };

        return timelineCtrl;
    }
}