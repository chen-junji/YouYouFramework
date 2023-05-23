using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class RoleInfo
{
    public RoleView CurrRole { get; private set; }

    public int CurrHP;
    public int MaxHP;

    public List<RoleInfoSkill> SkillList = new List<RoleInfoSkill>();

    public LinkedList<RoleInfoSkill> AttackList = new LinkedList<RoleInfoSkill>();

    //最后一次释放的普攻
    public LinkedListNode<RoleInfoSkill> CurrAttack;

    /// <summary>
    /// 初始化当前角色信息
    /// </summary>
    public void InitCurrPlayerInfo(RoleView roleCtrl, int maxHP)
    {
        CurrRole = roleCtrl;

        MaxHP = maxHP;
        CurrHP = MaxHP;
    }

    internal int GetCanUsedSkillId()
    {
        //优先获取技能ID
        for (int i = 0; i < SkillList.Count; i++)
        {
            if (SkillList[i].IsActive)
            {
                return SkillList[i].SkillId;
            }
        }

        //技能ID没有了, 再拿普攻ID
        return GetCanUsedAttackId();
    }
    internal int GetCanUsedAttackId()
    {
        //2秒内连续释放普攻视为普攻连招
        if (CurrAttack.Previous != null && Time.time - CurrAttack.Previous.Value.SkillCDBegTime > 3)
        {
            CurrAttack = AttackList.First;
        }
        return CurrAttack.Value.SkillId;
    }

    public void BegSkillCD(int skillId)
    {
        RoleInfoSkill roleInfoSkill = SkillList.Find(x => x.SkillId == skillId);
        if (roleInfoSkill == null)
        {
            for (LinkedListNode<RoleInfoSkill> node = AttackList.First; node != null; node = node.Next)
            {
                if (node.Value.SkillId == skillId) roleInfoSkill = node.Value;
            }
        }
        //roleInfoSkill.BegSkillCD();

        if (skillId == CurrAttack.Value.SkillId)
        {
            CurrAttack = CurrAttack.Next;
            if (CurrAttack == null) CurrAttack = AttackList.First;
        }
    }
}

public class RoleInfoSkill
{
    //技能编号
    public int SkillId;

    //技能最近一次开始释放的时间
    public float SkillCDBegTime { get; private set; }
    //技能CD结束时间
    public float SkillCDEndTime { get; private set; }

    //当前技能是否可用,  技能冷却是否结束 && MP是否足够
    public bool IsActive { get { return Time.time > SkillCDEndTime; } }//&& CurrMP >= SkillList[i].SpendMP

    //private SkillEntity SkillEntity;


    //public void BegSkillCD()
    //{
    //    if (SkillEntity == null) SkillEntity = GameEntry.DataTable.SkillDBModel.GetDic(SkillId);

    //    SkillCDBegTime = Time.time;
    //    SkillCDEndTime = SkillCDBegTime + SkillEntity.CDTime;
    //}
}
