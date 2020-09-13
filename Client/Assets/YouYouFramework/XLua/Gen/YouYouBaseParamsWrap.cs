#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class YouYouBaseParamsWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(YouYou.BaseParams);
			Utils.BeginObjectRegister(type, L, translator, 0, 1, 21, 21);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Reset", _m_Reset);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "IntParam1", _g_get_IntParam1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IntParam2", _g_get_IntParam2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IntParam3", _g_get_IntParam3);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IntParam4", _g_get_IntParam4);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IntParam5", _g_get_IntParam5);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ULongParam1", _g_get_ULongParam1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ULongParam2", _g_get_ULongParam2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ULongParam3", _g_get_ULongParam3);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ULongParam4", _g_get_ULongParam4);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ULongParam5", _g_get_ULongParam5);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "FloatParam1", _g_get_FloatParam1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "FloatParam2", _g_get_FloatParam2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "FloatParam3", _g_get_FloatParam3);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "FloatParam4", _g_get_FloatParam4);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "FloatParam5", _g_get_FloatParam5);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "StringParam1", _g_get_StringParam1);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "StringParam2", _g_get_StringParam2);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "StringParam3", _g_get_StringParam3);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "StringParam4", _g_get_StringParam4);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "StringParam5", _g_get_StringParam5);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Vector3Param1", _g_get_Vector3Param1);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "IntParam1", _s_set_IntParam1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IntParam2", _s_set_IntParam2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IntParam3", _s_set_IntParam3);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IntParam4", _s_set_IntParam4);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IntParam5", _s_set_IntParam5);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ULongParam1", _s_set_ULongParam1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ULongParam2", _s_set_ULongParam2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ULongParam3", _s_set_ULongParam3);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ULongParam4", _s_set_ULongParam4);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ULongParam5", _s_set_ULongParam5);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "FloatParam1", _s_set_FloatParam1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "FloatParam2", _s_set_FloatParam2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "FloatParam3", _s_set_FloatParam3);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "FloatParam4", _s_set_FloatParam4);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "FloatParam5", _s_set_FloatParam5);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "StringParam1", _s_set_StringParam1);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "StringParam2", _s_set_StringParam2);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "StringParam3", _s_set_StringParam3);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "StringParam4", _s_set_StringParam4);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "StringParam5", _s_set_StringParam5);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Vector3Param1", _s_set_Vector3Param1);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					YouYou.BaseParams gen_ret = new YouYou.BaseParams();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to YouYou.BaseParams constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Reset(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Reset(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IntParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.IntParam1);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IntParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.IntParam2);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IntParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.IntParam3);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IntParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.IntParam4);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IntParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.IntParam5);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ULongParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushuint64(L, gen_to_be_invoked.ULongParam1);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ULongParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushuint64(L, gen_to_be_invoked.ULongParam2);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ULongParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushuint64(L, gen_to_be_invoked.ULongParam3);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ULongParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushuint64(L, gen_to_be_invoked.ULongParam4);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ULongParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushuint64(L, gen_to_be_invoked.ULongParam5);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_FloatParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.FloatParam1);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_FloatParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.FloatParam2);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_FloatParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.FloatParam3);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_FloatParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.FloatParam4);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_FloatParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.FloatParam5);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_StringParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.StringParam1);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_StringParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.StringParam2);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_StringParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.StringParam3);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_StringParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.StringParam4);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_StringParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.StringParam5);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Vector3Param1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineVector3(L, gen_to_be_invoked.Vector3Param1);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IntParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IntParam1 = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IntParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IntParam2 = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IntParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IntParam3 = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IntParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IntParam4 = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IntParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IntParam5 = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ULongParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ULongParam1 = LuaAPI.lua_touint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ULongParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ULongParam2 = LuaAPI.lua_touint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ULongParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ULongParam3 = LuaAPI.lua_touint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ULongParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ULongParam4 = LuaAPI.lua_touint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ULongParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ULongParam5 = LuaAPI.lua_touint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_FloatParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.FloatParam1 = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_FloatParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.FloatParam2 = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_FloatParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.FloatParam3 = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_FloatParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.FloatParam4 = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_FloatParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.FloatParam5 = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_StringParam1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.StringParam1 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_StringParam2(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.StringParam2 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_StringParam3(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.StringParam3 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_StringParam4(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.StringParam4 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_StringParam5(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.StringParam5 = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Vector3Param1(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                YouYou.BaseParams gen_to_be_invoked = (YouYou.BaseParams)translator.FastGetCSObj(L, 1);
                UnityEngine.Vector3 gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.Vector3Param1 = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
