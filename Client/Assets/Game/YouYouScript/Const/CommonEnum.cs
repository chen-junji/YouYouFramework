using System;

/// <summary>
/// 获取条件类型 1准备打卡;2积分;3打boss;4订阅;5去旅行;6其他;7探索打卡
/// </summary>
public enum ItemDataConditionType
{
    Prepara = 1,
    PowerPoint = 2,
    Determina = 3,
    Subscribe = 4,
    Trip = 5,
    Other = 6,
    Explora = 7,
}
/// <summary>
/// 物品类型:角色1,伙伴2,技能3,成就4,称号5
/// </summary>
public enum ItemDataItemType
{
    JueSe = 1,
    HuoBan = 2,
    JiNeng = 3,
    ChengJiu = 4,
    ChengHao = 5,
}

/// <summary>
/// 成长界面物品类型:成长1,伙伴2,技能3,人物4
/// </summary>
public enum UIGrawthItemType
{
    Grawth = 1,
    HuoBan = 2,
    JiNeng = 3,
    RenWu = 4,
}

/// <summary>
/// 玩法模式
/// </summary>
public enum MainWindowMenuType
{
    None = 0,
    /// <summary>
    /// 准备
    /// </summary>
    PREPARATION = 1,
    /// <summary>
    /// 探索
    /// </summary>
    EXPLORATION = 2,
    /// <summary>
    /// 决心
    /// </summary>
    DETERMINATION = 3,
    /// <summary>
    /// 陪伴
    /// </summary>
    COMPANION = 4,
    /// <summary>
    /// 单词本
    /// </summary>
    WORDBOOK = 5,
}

/// <summary>
/// 题库分类(难度等级)
/// </summary>
[Serializable]
public enum QuestionBank
{
    None = 0,
    Primary = 1,
    Medium = 2,
    Advaced = 3,
    Ultimate = 4
}
/// <summary>
/// 词性分类
/// </summary>
[Serializable]
public enum PartOfSpeech
{
    None = 0,
    n = 1,
    v = 2,
    adj = 3,
    adv = 4,
    num = 5,
    prep = 6,
    pron = 7,
    conj = 8
}
/// <summary>
/// 字母数分类
/// </summary>
[Serializable]
public enum Letters
{
    None = 0,
    Letters3To4 = 1,
    Letters5To6 = 2,
    Letters7To8 = 3,
    Letters9To10 = 4,
    Letters11Up = 5
}

/// <summary>
/// 角色动画状态名称
/// </summary>
public enum RoleAnimatorState
{
    OrdStby = 1,
    AtkStby = 2,
    Walk = 3,
    Beaten = 4,
    Die = 5,
    Select = 6,
    XiuXian = 7,
    Died = 8,
}
/// <summary>
/// 角色动画状态切换条件
/// </summary>
public enum ToAnimatorCondition
{
    OrdStby,
    AtkStby,
    Walk,
    Beaten,
    Fail,
    ToSelect,
    Skill,
    Wrong
}

public enum SkillRoleType
{
    None = 0,
    Uji_Nanami = 1,
    Color = 2,
    Clione = 3,
}


/// <summary>
/// Camera镜头组(新增枚举, 只能填在最下面, 否则会导致Prefab那边绑定枚举错位)
/// </summary>
public enum CameraAnimationType
{
    AtkStby,
    FailRight,
    EnemyABC_Wrong,
    UltimateBoss_Start,//普通Boss和终极Boss出场表演
    Clione_Warmup,
    StartScene,
    User_Closeup,
    Enemy_Closeup,
    UjiNanami_AdvSki,
    Color_AdvSki,
    Clione_OrdSki1,
    Clione_OrdSki2,
    Clione_OrdSki3,
    Clione_OrdSki4,
    Clione_OrdSki5,
    Clione_OrdSki6,
    Clione_OrdSki7,
    Clione_OrdSki8,
    Clione_OrdSki9,
    Clione_OrdSki10,
    Clione_OrdSki11,
    Clione_OrdSki12,
    Clione_AdvSki1,
    Clione_AdvSki2,
    Clione_AdvSki3,
    Clione_AdvSki4,
    Clione_AdvSki5,
    Clione_AdvSki6,
    Color_OrdSki,
    UjiNanami_OrdSki,
    Enemy_C_Atk,
    Enemy_Cn_3_AtkLhand2,
    Enemy_B_Atk,
    Enemy_A1_AtkShoot,
    Enemy_A2_AtkLhand,
    Enemy_A2_AtkLhand2,
    Enemy_A3_AtkStap,
    Enemy_A4_AtkShoot,
    NPCn_AtkOrd,
    NPCn_AtkOrd2,
    NPCn_AtkAdv,
    Truth_AtkOrd,
    Truth_AtkAdv,
    Boxman_AtkOrd,
    Boxman_AtkOrd2,
    Boxman_AtkAdv,
    CloneOfClione_AtkOrd,
    CloneOfClione_AtkOrd2,
    CloneOfClione_AtkAdv,
    CloneOfClione_AtkAdv2,
    RippleOfStar_AtkOrd,
    RippleOfStar_AtkAdv,
    BoneOfClione_AtkOrd,
    BoneOfClione_AtkOrd2,
    BoneOfClione_AtkOrd3,
    BoneOfClione_AtkAdv,
    BoneOfClione_AtkAdv2,
    OldGoddess_AtkOrd,
    OldGoddess_AtkOrd2,
    OldGoddess_AtkAdv,
    OldGoddess_AtkAdv2,
    TripStart,
}

public enum MonthType
{
    One = 20001,
    Three = 20002,
    Six = 20003,
    Twelve = 20004
}

/// <summary>
/// 翻页选项
/// </summary>
public enum Dir
{
    Left,
    Right
}