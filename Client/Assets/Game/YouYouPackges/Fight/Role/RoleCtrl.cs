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

    /// <summary>
    /// 行为树SO
    /// </summary>
    [SerializeField]
    private BehaviourTreeSO BTSO;
    private BehaviourTree behaviourTree;

    protected virtual void Awake()
    {
        //初始化导航系统
        Agent = GetComponent<RoleAgentCompoent>();

        //初始化动画系统
        AnimCompoent = GetComponent<RoleAnimCompoent>();
        AnimCompoent.InitAnim(GetComponentInChildren<Animator>());

        //初始化行为树
        behaviourTree = BTSO.CloneBehaviourTree();
        behaviourTree.RoleCtrl = this;
        behaviourTree.Start("Debugger Test");
    }
    private void OnDestroy()
    {
        behaviourTree.Cancel();
    }

    /// <summary>
    /// 创建技能效果的Timeline
    /// </summary>
    public virtual TimelineCtrl CreateSkillTimeLine(string prefabName)
    {
        TimelineCtrl timelineCtrl = GameEntry.Pool.GameObjectPool.Spawn(prefabName).GetComponent<TimelineCtrl>();
        timelineCtrl.transform.position = transform.position;
        timelineCtrl.transform.rotation = transform.rotation;

        timelineCtrl.CameraShake = (args) =>
        {
            CameraFollowCtrl.Instance.CameraShake();
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
            Transform poolObj = GameEntry.Pool.GameObjectPool.Spawn(args.PrefabName);
            poolObj.SetPositionAndRotation(transform.position, transform.rotation);
            poolObj.gameObject.GetOrCreatComponent<AutoDespawnHandle>().SetDelayTimeDespawn(delayTime);
        };
        timelineCtrl.PlaySound = (args) =>
        {
            GameEntry.Audio.PlayAudio(args.AudioClip, transform.position);
        };
        return timelineCtrl;
    }
}