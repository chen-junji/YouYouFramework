using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYouFramework
{
    /// <summary>
    /// 数据表管理基类
    /// </summary>
    /// <typeparam name="T">数据表管理子类的类型</typeparam>
    /// <typeparam name="P">数据表实体子类的类型</typeparam>
    public abstract class DataTableDBModelBase<T, P>
    where T : class, new()
    where P : DataTableEntityBase
    {
        /// <summary>
        /// Entity对象的集合
        /// </summary>
        protected List<P> m_List;
        public int Count { get { return m_List.Count; } }

        /// <summary>
        /// Key:Entity的ID
        /// Value:Entity对象
        /// </summary>
        protected Dictionary<int, P> m_Dic;

        public DataTableDBModelBase()
        {
            m_List = new List<P>();
            m_Dic = new Dictionary<int, P>();
        }

        #region 需要子类实现的属性,方法
        /// <summary>
        /// 数据表名称
        /// </summary>
        public abstract string DataTableName { get; }
        /// <summary>
        /// 加载数据列表
        /// </summary>
        protected abstract void LoadList(MMO_MemoryStream ms);
        protected virtual void OnLoadListComple() { }
        #endregion

        #region LoadData 加载数据表数据
        /// <summary>
        /// 加载数据表数据
        /// </summary>
        internal async UniTask LoadData()
        {
            //1.拿到这个表格的buffer
            byte[] buffer = await GameEntry.DataTable.GetDataTableBuffer(DataTableName);

            using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
            {
                LoadList(ms);
            }

            OnLoadListComple();
        }
        #endregion

        #region GetList 获取子类对应的数据实体List
        /// <summary>
        /// 获取子类对应的数据实体List
        /// </summary>
        /// <returns></returns>
        public List<P> GetList()
        {
            return m_List;
        }
        #endregion

        #region GetDic 根据ID获取实体
        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        public P GetDic(int id)
        {
            if (m_Dic.TryGetValue(id, out P p))
            {
                return p;
            }
            else
            {
                GameEntry.LogError(LogCategory.Normal, "该ID对应的数据实体不存在");
                return null;
            }
        }
        public bool TryGetValue(int id, out P value)
        {
            return m_Dic.TryGetValue(id, out value);
        }
        #endregion

        /// <summary>
        /// 清空数据
        /// </summary>
        internal void Clear()
        {
            m_List.Clear();
            m_Dic.Clear();
        }

    }
}