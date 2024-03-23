
using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// 黑板参数
/// </summary>
[Serializable]
public abstract class BBParam
{
    /// <summary>
    /// 参数key
    /// </summary>
    public string Key;
        
    /// <summary>
    /// 参数值obj
    /// </summary>
    public abstract object ValueObj { get; set; }
    
    /// <summary>
    /// 获取黑板参数的类型名
    /// </summary>
    public static string GetBBParamTypeName(Type type)
    {
        string name = type.Name;
        name = name.Replace("BBParam", "");
        return name;
    }
}

public class BBParam<T> : BBParam
{
    /// <summary>
    /// 参数值
    /// </summary>
    public T Value;

    /// <summary>
    /// 参数值obj
    /// </summary>
    public override object ValueObj
    {
        get => Value;
        set => Value = value == null ? Value = default : (T)value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

[Serializable]
public class BBParamBool : BBParam<bool>
{

}
    
[Serializable]
public class BBParamInt : BBParam<int>
{

}
    
[Serializable]
public class BBParamFloat : BBParam<float>
{

}
    
[Serializable]
public class BBParamString : BBParam<string>
{

}
    
[Serializable]
public class BBParamVector2 : BBParam<Vector2>
{

}
    
[Serializable]
public class BBParamVector3 : BBParam<Vector3>
{

}
    
[Serializable]
public class BBParamVector4 : BBParam<Vector4>
{

}

[Serializable]
public class BBParamBoolList : BBParam<List<bool>>
{
        
}
    
[Serializable]
public class BBParamIntList : BBParam<List<int>>
{
        
}
    
[Serializable]
public class BBParamFloatList : BBParam<List<float>>
{
        
}
    
[Serializable]
public class BBParamStringList : BBParam<List<string>>
{
        
}
    
[Serializable]
public class BBParamVector2List : BBParam<List<Vector2>>
{
        
}
    
[Serializable]
public class BBParamVector3List : BBParam<List<Vector3>>
{
}
    
[Serializable]
public class BBParamVector4List : BBParam<List<Vector4>>
{
        
}