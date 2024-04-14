using System;
using UnityEngine;
using UnityEngine.AI;
using YouYouFramework;

public class RoleCtrl : MonoBehaviour
{
    /// <summary>
    /// 自定义动画组件
    /// </summary>
    public RoleAnimCompoent AnimCompoent { get; private set; }
    /// <summary>
    /// 自定义导航组件
    /// </summary>
    public RoleAgentCompoent Agent { get; private set; }

    protected virtual void Awake()
    {
        //初始化导航系统
        Agent = GetComponent<RoleAgentCompoent>();

        //初始化动画系统
        AnimCompoent = GetComponent<RoleAnimCompoent>();
        AnimCompoent.InitAnim(GetComponentInChildren<Animator>());
    }

    /// <summary>
    /// 创建技能效果的Timeline
    /// </summary>
    public virtual TimelineCtrl CreateSkillTimeLine(string prefabFullPath)
    {
        TimelineCtrl timelineCtrl = GameEntry.Pool.GameObjectPool.Spawn(prefabFullPath).GetComponent<TimelineCtrl>();
        timelineCtrl.OnStopped += () =>
        {
            GameEntry.Pool.GameObjectPool.Despawn(timelineCtrl.gameObject);
        };
        timelineCtrl.CameraShake = (args) =>
        {
            //这里需要接入CameraFollow范例工程， 才可以使用震屏效果
            //CameraFollowCtrl.Instance.CameraShake();
        };
        timelineCtrl.HurtPoint = (args) =>
        {

        };
        timelineCtrl.PlayAnim = (args) =>
        {
            AnimCompoent.PlayAnim(args.AnimationClip);
        };
        timelineCtrl.PlayResource = (args, delayTime) =>
        {
            GameObject poolObj = GameEntry.Pool.GameObjectPool.Spawn(args.PrefabFullPath);
            poolObj.transform.SetPositionAndRotation(transform.position, transform.rotation);
            poolObj.gameObject.GetOrCreatComponent<AutoDespawnHandle>().SetDelayTimeDespawn(delayTime);
        };
        timelineCtrl.PlaySound = (args) =>
        {
            GameEntry.Audio.PlayAudio(args.AudioClip, transform.position);
        };

        timelineCtrl.transform.position = transform.position;
        timelineCtrl.transform.rotation = transform.rotation;

        return timelineCtrl;
    }
}