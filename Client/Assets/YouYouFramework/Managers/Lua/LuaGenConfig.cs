//===================================================
//作    者：边涯  http://www.u3dol.com
//创建时间：
//备    注：
//===================================================
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace YouYou
{
    public static class LuaGenConfig
    {
        //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp = new List<Type>() {
                typeof(GameEntry),
                typeof(SocketEvent),
                typeof(CommonEvent),
                typeof(BaseParams),
                typeof(BaseParams),
                typeof(HttpCallBackArgs),
                typeof(RetValue),
                typeof(StringUtil),

                //typeof(Chapter),
                //typeof(Chapter?),
                //typeof(ChapterList),
                //typeof(ChapterListExt),
            };

        //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
        [CSharpCallLua]
        public static List<Type> CSharpCallLua = new List<Type>()
        {
                typeof(HttpSendDataCallBack),
        };
    }
}