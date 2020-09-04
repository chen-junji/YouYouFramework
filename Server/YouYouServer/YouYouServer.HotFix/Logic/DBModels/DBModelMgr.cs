using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Model.Logic.DBModels
{
    /// <summary>
    /// 数据模型管理器
    /// </summary>
    public sealed class DBModelMgr
    {
        private static object m_lock = new object();

        //====================账号服=========================

        #region UniqueIDAccount
        private static UniqueIDAccount m_UniqueIDAccount;
        public static UniqueIDAccount UniqueIDAccount
        {
            get
            {
                if (m_UniqueIDAccount == null)
                {
                    lock (m_lock)
                    {
                        if (m_UniqueIDAccount == null)
                        {
                            m_UniqueIDAccount = new UniqueIDAccount();
                        }
                    }
                }
                return m_UniqueIDAccount;
            }
        }
        #endregion

        #region AccountDBModel 账号DBModel
        private static AccountDBModel m_AccountDBModel;
        public static AccountDBModel AccountDBModel
        {
            get
            {
                if (m_AccountDBModel == null)
                {
                    lock (m_lock)
                    {
                        if (m_AccountDBModel == null)
                        {
                            m_AccountDBModel = new AccountDBModel();
                        }
                    }
                }
                return m_AccountDBModel;
            }
        }
        #endregion

        //====================区服=========================

        #region UniqueIDGameServer
        private static UniqueIDGameServer m_UniqueIDGameServer;
        public static UniqueIDGameServer UniqueIDGameServer
        {
            get
            {
                if (m_UniqueIDGameServer == null)
                {
                    lock (m_lock)
                    {
                        if (m_UniqueIDGameServer == null)
                        {
                            m_UniqueIDGameServer = new UniqueIDGameServer();
                        }
                    }
                }
                return m_UniqueIDGameServer;
            }
        }
        #endregion

        #region RoleDBModel 角色DBModel
        private static RoleDBModel m_RoleDBModel;
        public static RoleDBModel RoleDBModel
        {
            get
            {
                if (m_RoleDBModel == null)
                {
                    lock (m_lock)
                    {
                        if (m_RoleDBModel == null)
                        {
                            m_RoleDBModel = new RoleDBModel();
                        }
                    }
                }
                return m_RoleDBModel;
            }
        }
        #endregion
    }
}
