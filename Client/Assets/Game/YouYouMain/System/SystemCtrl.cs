using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class SystemCtrl
    {
        private static SystemCtrl instance;
        public static SystemCtrl Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SystemCtrl();
                }
                return instance;
            }
        }
    }
}