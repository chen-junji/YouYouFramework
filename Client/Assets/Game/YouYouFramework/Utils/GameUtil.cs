using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


namespace YouYou
{
    public class GameUtil
    {
        /// <summary>
        /// 获取UI资源的路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetUIResPath(string path)
        {
            return string.Format("UI/UIRes/UITexture/{0}.png", path);
        }
    }
}