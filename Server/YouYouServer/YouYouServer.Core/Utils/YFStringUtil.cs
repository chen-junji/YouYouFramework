using System;
using System.Collections.Generic;
using System.Text;

namespace YouYouServer.Core.Utils
{
    public static class YFStringUtil
    {
        #region IsNullOrEmpty 验证值是否为null

        /// <summary>
        /// 判断对象是否为Null、DBNull、Empty或空白字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object value)
        {
            bool retVal = false;
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()) || (value.GetType().Equals(DBNull.Value.GetType())))
            {
                retVal = true;
            }
            return retVal;
        }

        #endregion

        #region ObjectToString object转换Str 自动Trim
        /// <summary>
        ///  ObjectToString object转换Str 自动Trim
        /// </summary>
        /// <param name="canNullStr"></param>
        /// <returns></returns>
        public static string ObjectToString(this object canNullStr)
        {
            return canNullStr.ObjectToString("");
        }

        /// <summary>
        /// ObjectToString object转换Str 自动Trim
        /// </summary>
        /// <param name="canNullStr"></param>
        /// <param name="defaultStr"></param>
        /// <returns></returns>
        public static string ObjectToString(this object canNullStr, string defaultStr)
        {
            try
            {
                if ((canNullStr == null) || (canNullStr is DBNull))
                {
                    if (defaultStr != null)
                    {
                        return defaultStr;
                    }
                    return string.Empty;
                }
                return Convert.ToString(canNullStr).Trim();
            }
            catch
            {
                return defaultStr;
            }
        }
        #endregion

        #region ToShort
        /// <summary>
        /// ToShort
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>retValue</returns>
        public static short ToShort(this string value)
        {
            short ret = 0;
            short.TryParse(value, out ret);
            return ret;
        }

        public static short ToShort(this object value)
        {
            short ret = 0;
            if (value != null)
            {
                short.TryParse(value.ToString(), out ret);
            }
            return ret;
        }
        #endregion

        #region ToInt
        /// <summary>
        /// ToInt
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>retValue</returns>
        public static int ToInt(this string value)
        {
            int ret = 0;
            int.TryParse(value, out ret);
            return ret;
        }

        public static int ToInt(this object value)
        {
            int ret = 0;
            if (value != null)
            {
                int.TryParse(value.ToString(), out ret);
            }
            return ret;
        }
        #endregion

        #region ToLong
        /// <summary>
        /// ToLong
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>retValue</returns>
        public static long ToLong(this string value)
        {
            long ret = 0;
            long.TryParse(value, out ret);
            return ret;
        }

        public static long ToLong(this object value)
        {
            long ret = 0;
            if (value != null)
            {
                long.TryParse(value.ToString(), out ret);
            }
            return ret;
        }
        #endregion

        #region ToFloat
        /// <summary>
        /// ToFloat
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>retValue</returns>
        public static float ToFloat(this string value)
        {
            float ret = 0;
            float.TryParse(value, out ret);
            return ret;
        }
        #endregion
    }
}