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

    //角色实时信息
    public RoleInfo RoleInfo;

    internal virtual void CheckHurt(HurtPointEventArgs args, Vector3 position) { }

    protected virtual void Awake()
    {
        RoleView = GetComponent<RoleView>();
    }

    public void PlayAnim(string animName, bool isLoop = false, Action onComplete = null)
    {
        RoleView.AnimCompoent.PlayAnim(animName, isLoop, onComplete);
    }
    public void PlayAnim(AnimationClip animClip, bool isLoop = false, Action onComplete = null)
    {
        RoleView.AnimCompoent.PlayAnim(animClip, isLoop, onComplete);
    }

    public void SetPostionAndRotation(Vector3 pos, Quaternion rot)
    {
        RoleView.Agent.SetPostionAndRotation(pos, rot);
    }
    public NavMeshPathStatus ClickMove(Vector3 targetPos)
    {
       return RoleView.Agent.ClickMove(targetPos);
    }
    public void ClickMoveStop()
    {
        RoleView.Agent.ClickMoveStop();
    }

    public T GetLockEnemy<T>(float radius) where T : RoleCtrl
    {
        //环形射线寻找目标 找离当前攻击者 最近的 就是锁定敌人
        T lockEnemy = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, 1 << LayerMask.NameToLayer("Role"));
        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                T enemy = colliders[i].GetComponent<T>();
                if (enemy != null)
                {
                    if (lockEnemy == null) lockEnemy = enemy;
                    if (Vector3.Distance(enemy.transform.position, transform.position) < Vector3.Distance(lockEnemy.transform.position, transform.position))
                    {
                        //获取最近的目标
                        lockEnemy = enemy;
                    }
                }
            }
        }
        return lockEnemy;
    }

    public virtual void AddBuff(BuffCategory buffCategory, float buffValue)
    {
    }
}