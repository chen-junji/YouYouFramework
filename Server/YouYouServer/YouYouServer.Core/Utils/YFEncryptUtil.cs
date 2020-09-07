using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YouYouServer.Core
{
    /// <summary>
    /// 加密解密 MD5 SHA1
    /// </summary>
    public static class YFEncryptUtil
    {
        #region DES加密解密
        private static byte[] Keys = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        public static string KeyDES = "87654321";

        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="encryptKey">加密密钥,要求为8位</param>
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public static string EncryptDES(string encryptString, string encryptKey)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return encryptString;
            }
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        public static string DecryptDES(string decryptString, string decryptKey)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return decryptString;
            }
        }
        #endregion

        #region Sha1
        /// <summary>
        /// Sha1加密
        /// </summary>
        /// <param name="value">value</param>
        /// <returns></returns>
        public static string Sha1(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] bytResult = sha1.ComputeHash(System.Text.Encoding.Default.GetBytes(value));
            string strResult = BitConverter.ToString(bytResult);
            strResult = strResult.Replace("-", "");
            return strResult;

        }
        #endregion Sha1

        #region Md5
        /// <summary>
        /// Md5加密
        /// </summary>
        /// <param name="value">value</param>
        /// <returns></returns>
        public static string Md5(this string sDataIn)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(sDataIn);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToUpper();
        }

        /// <summary>
        /// Md5加密
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="Is16">是否16位密码</param>
        /// <returns></returns>
        public static string Md5(this string value, bool Is16)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (Is16)
            {

                return value.Md5().Substring(8, 16);
            }
            else
            {
                return value.Md5();
            }
        }
        #endregion Md5
    }
}
