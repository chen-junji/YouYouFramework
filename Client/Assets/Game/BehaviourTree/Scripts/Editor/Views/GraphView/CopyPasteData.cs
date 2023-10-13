using System;
using System.Collections.Generic;

/// <summary>
/// 节点的复制粘贴数据
/// </summary>
[Serializable]
public class CopyPasteData
{
    /// <summary>
    /// 被复制的节点列表
    /// </summary>
    public List<JsonElement> CopiedNodes = new List<JsonElement>();
}