using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using XLua;



public class LuaJitType
{
    //public const uint LJ_TISNUM = 0xfffeffffu; // this is for LJ_64 && !LJ_GC64
    public static uint LJ_TISNUM = (~13u); // this is not correct on LJ_64 && !LJ_GC64, 64bit arch without GC64
    public const uint LJ_TNUMX = (~13u);

#if UNITY_IPHONE || UNITY_TVOS
    public static bool LJ_DUALNUM = true;
    public static bool GC64 = true;// arm64 always on in beta3
#elif UNITY_ANDROID
    public static bool LJ_DUALNUM = true;
    public static bool GC64 = false;// false on arm32 and true on arm64
#else
    public static bool LJ_DUALNUM = false;
    public static bool GC64 = true;// only true if LUAJIT_ENABLE_GC64 is used
#endif

    static LuaJitType()
    {
    }

    // see tvisint and tvisnum in lj_obj.h
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIntType(ref LuaJitTValue tv)
    {
#if UNITY_IPHONE || UNITY_TVOS
        if(GC64)
            return (((UInt32)(tv.it64 >> 47)) == LJ_TISNUM);
        else
            return (tv.it == LJ_TISNUM);
#elif UNITY_ANDROID
        if (!GC64)
            return (LJ_DUALNUM && tv.it == LJ_TISNUM);
        else
            return (((UInt32)(tv.it64 >> 47)) == LJ_TISNUM);
#else
        return false;
#endif

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetDouble(ref LuaJitTValue tv)
    {
#if UNITY_IPHONE || UNITY_TVOS || UNITY_ANDROID
        if (IsIntType(ref tv))
            return tv.i;
        else
            return tv.n;
#else
        return tv.n;
#endif

    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetInt(ref LuaJitTValue tv)
    {
#if UNITY_IPHONE || UNITY_TVOS || UNITY_ANDROID
        if (IsIntType(ref tv))
            return tv.i;
        else
            return (int)tv.n;        
#else
        return (int)tv.n;
#endif
    }
}


/// <summary>
/// see TValue in lj_obj.h
/// we make sure to handle both GC_64 and 32, also be careful on i/it for endian issue
/// LJ_ARCH_ENDIAN = LUAJIT_LE in x86/x64/arm/arm64
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct LuaJitTValue
{
    // uint64
    [FieldOffset(0)]
    public UInt64 u64;

    // number
    [FieldOffset(0)]
    public double n;

    // integer value
    [FieldOffset(0)]
    public int i;

    // internal object tag for GC64
    [FieldOffset(0)]
    public Int64 it64;

    // internal object tag
    [FieldOffset(4)]
    public UInt32 it;
}

/// <summary>
/// see Node in lj_obj.h
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 24)]
public struct LuaJitNode
{
    [FieldOffset(0)]
    public LuaJitTValue val;

    [FieldOffset(8)]
    public LuaJitTValue key;

    [FieldOffset(16)]
    public UInt64 next64;

    [FieldOffset(16)]
    public UInt32 next32;

    [FieldOffset(20)]
    public UInt32 freetop;
}

// see GCtab in lj_obj.h
public unsafe struct LuaJitGCtab32
{
    public IntPtr nextgc;

    public UInt32 masks;

    public LuaJitTValue* array;

    public IntPtr gclist;

    public IntPtr metatable;

    public IntPtr node;

    public UInt32 asize;

    public UInt32 hmask;
}

// GC64 version
public struct LuaJitGCtabGC64
{
    IntPtr nextgc;

    UInt32 masks;

    IntPtr array;

    IntPtr gclist;

    IntPtr metatable;

    IntPtr node;

    UInt32 asize;

    UInt32 hmask;

    IntPtr freetop; // only valid for LJ_GC64
}







/////////////////////////////////////////////////////////////


[LuaCallCSharp]
public unsafe class LuaJitArrAccess : LuaArrAccess
{
    LuaJitGCtab32* TableRawPtr; // its ok to always use 32bit version since it has the same structure before GCTab.hmask

    public override string ToString()
    {
        IntPtr Ptr = (IntPtr)TableRawPtr;
        return "LuaJitTablePin " + Ptr.ToString();
    }

    public override void OnPin(IntPtr TablePtr)
    {
        TableRawPtr = (LuaJitGCtab32*)TablePtr;
    }

    public override void OnGC()
    {
        TableRawPtr = null;
    }

    private bool IsArchCorrect()
    {
        if (GetInt(1) == 32167 && double.IsNaN(GetDouble(2)) == false &&
            GetInt(2) == 9527 && Math.Abs(GetDouble(2) - 9527.5) < 0.00001 &&
            GetInt(3) == -2000000)
        {
            return true;
        }
        return false;
    }

    private void LogDebug(string msg)
    {
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_IOS || UNITY_TVOS ||UNITY_ANDROID || UNITY_STANDALONE_WIN
        UnityEngine.Debug.Log(msg);
#endif
    }

    public override void AutoDetectArch()
    {
        // first, use default value to test
        if (IsArchCorrect())
        {
            LogDebug("luajit with default arch");
            return;
        }

        // LJ64 + !GC64
        if(LuaJitType.GC64 == false)
        {
            LuaJitType.LJ_TISNUM = 0xfffeffffu;
            if (IsArchCorrect())
            {
                LogDebug("luajit with LJ64 + !GC64");
                return;
            }
        }

        // LJ64 + GC64
        LuaJitType.GC64 = true;
        LuaJitType.LJ_TISNUM = ~13u;
        if (IsArchCorrect())
        {
            LogDebug("luajit with LJ64 + GC64");
            return;
        }

        // LJ32 + !GC64
        LuaJitType.GC64 = false;
        LuaJitType.LJ_TISNUM = ~13u;
        if (IsArchCorrect())
        {
            LogDebug("luajit with LJ32 + !GC64");
            return;
        }

        // disable dualnum, this is needed for x86 in android/ios simulator, and GC64/LJ_TISNUM is not needed
        LuaJitType.LJ_DUALNUM = false;
        if (IsArchCorrect())
        {
            LogDebug("luajit with DUALNUM == false");
            return;
        }


        throw new LuaAdapterException("unknown arch for lua jit, try to modify this function to support it");
    }

    public override bool IsValid()
    {
        return (TableRawPtr != null);
    }

    public override uint GetArrayCapacity()
    {
        if(TableRawPtr != null)
        {
            // luajit have array[0], so asize is power of 2 + 1
            return TableRawPtr->asize > 0 ? TableRawPtr->asize - 1 : 0;
        }
        return 0;
    }

    public double GetDoubleFast(int index)
    {
        return TableRawPtr->array[index].n;
    }

    public int GetIntFast(int index)
    {
        return TableRawPtr->array[index].i;
    }

    public override double GetDouble(int index)
    {
        if (TableRawPtr != null && index >= 0 && index <= TableRawPtr->asize)
        {
            return LuaJitType.GetDouble(ref TableRawPtr->array[index]);
        }
        else
        {
            LuaAdapterException.ThrowIfNeeded(TableRawPtr == null, index, GetArrayCapacity());
            return 0;
        }
    }


    public override void SetDouble(int index, double val)
    {
        if (TableRawPtr != null && index >= 0 && index <= TableRawPtr->asize)
        {
            TableRawPtr->array[index].n = val;
        }
        else
        {
            LuaAdapterException.ThrowIfNeeded(TableRawPtr == null, index, GetArrayCapacity());
        }
        
    }

    public override int GetInt(int index)
    {
        if (TableRawPtr != null && index >= 0 && index <= TableRawPtr->asize)
        {
            return LuaJitType.GetInt(ref TableRawPtr->array[index]);
        }
        else
        {
            LuaAdapterException.ThrowIfNeeded(TableRawPtr == null, index, GetArrayCapacity());
            return 0;
        }
    }

    public override void SetInt(int index, int val)
    {
        if (TableRawPtr != null && index >= 0 && index <= TableRawPtr->asize)
        {
            TableRawPtr->array[index].n = val;
        }
        else
        {
            LuaAdapterException.ThrowIfNeeded(TableRawPtr == null, index, GetArrayCapacity());
        }
    }
    
}