using System;

public class ShareUserData : IDisposable
{
    private LuaArrAccess m_CurrArrAccess;

    /// <summary>
    /// 初始化c#访问器
    /// </summary>
    /// <param name="arrAccess"></param>
    public void InitLuaArrAccess(LuaArrAccess arrAccess)
    {
        m_CurrArrAccess = arrAccess;
    }

    /// <summary>
    /// 玩家账号
    /// </summary>
    public long AccountId { get { return m_CurrArrAccess.GetLong(1); } set { m_CurrArrAccess.SetLong(1, value); } }

    /// <summary>
    /// 角色编号
    /// </summary>
    public int CurrRoleId { get { return m_CurrArrAccess.GetInt(2); } set { m_CurrArrAccess.SetInt(2, value); } }

    /// <summary>
    /// 测试double
    /// </summary>
    public double TestD { get { return m_CurrArrAccess.GetDouble(3); } set { m_CurrArrAccess.SetDouble(3, value); } }

    public void Dispose()
    {
        AccountId = 0;
        CurrRoleId = 0;
        TestD = 0;
    }
}