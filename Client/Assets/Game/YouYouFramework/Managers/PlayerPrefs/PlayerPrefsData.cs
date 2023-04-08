using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class PlayerPrefsData
    {
        public JGuideEntity GuideEntity = new JGuideEntity();
    }

    public class JGuideEntity
    {
        //当前已完成的新手引导
        public GuideState CurrGuide;

        /// <summary>
        /// 新手引导 完成一步
        /// </summary>
        public void GuideCompleteOne(GuideState guideState)
        {
            CurrGuide = guideState;
        }
    }
}