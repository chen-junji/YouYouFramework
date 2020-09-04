// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace YouYou.DataTable
{

using global::System;
using global::FlatBuffers;

public struct Sys_UIForm : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static Sys_UIForm GetRootAsSys_UIForm(ByteBuffer _bb) { return GetRootAsSys_UIForm(_bb, new Sys_UIForm()); }
  public static Sys_UIForm GetRootAsSys_UIForm(ByteBuffer _bb, Sys_UIForm obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public Sys_UIForm __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Id { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public string Desc { get { int o = __p.__offset(6); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetDescBytes() { return __p.__vector_as_span(6); }
#else
  public ArraySegment<byte>? GetDescBytes() { return __p.__vector_as_arraysegment(6); }
#endif
  public byte[] GetDescArray() { return __p.__vector_as_array<byte>(6); }
  public string Name { get { int o = __p.__offset(8); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span(8); }
#else
  public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(8); }
#endif
  public byte[] GetNameArray() { return __p.__vector_as_array<byte>(8); }
  public byte UIGroupId { get { int o = __p.__offset(10); return o != 0 ? __p.bb.Get(o + __p.bb_pos) : (byte)0; } }
  public int DisableUILayer { get { int o = __p.__offset(12); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int IsLock { get { int o = __p.__offset(14); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public string AssetPathChinese { get { int o = __p.__offset(16); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetAssetPathChineseBytes() { return __p.__vector_as_span(16); }
#else
  public ArraySegment<byte>? GetAssetPathChineseBytes() { return __p.__vector_as_arraysegment(16); }
#endif
  public byte[] GetAssetPathChineseArray() { return __p.__vector_as_array<byte>(16); }
  public string AssetPathEnglish { get { int o = __p.__offset(18); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetAssetPathEnglishBytes() { return __p.__vector_as_span(18); }
#else
  public ArraySegment<byte>? GetAssetPathEnglishBytes() { return __p.__vector_as_arraysegment(18); }
#endif
  public byte[] GetAssetPathEnglishArray() { return __p.__vector_as_array<byte>(18); }
  public bool CanMulit { get { int o = __p.__offset(20); return o != 0 ? 0!=__p.bb.Get(o + __p.bb_pos) : (bool)false; } }
  public byte ShowMode { get { int o = __p.__offset(22); return o != 0 ? __p.bb.Get(o + __p.bb_pos) : (byte)0; } }
  public byte FreezeMode { get { int o = __p.__offset(24); return o != 0 ? __p.bb.Get(o + __p.bb_pos) : (byte)0; } }

  public static Offset<Sys_UIForm> CreateSys_UIForm(FlatBufferBuilder builder,
      int Id = 0,
      StringOffset DescOffset = default(StringOffset),
      StringOffset NameOffset = default(StringOffset),
      byte UIGroupId = 0,
      int DisableUILayer = 0,
      int IsLock = 0,
      StringOffset AssetPath_ChineseOffset = default(StringOffset),
      StringOffset AssetPath_EnglishOffset = default(StringOffset),
      bool CanMulit = false,
      byte ShowMode = 0,
      byte FreezeMode = 0) {
    builder.StartObject(11);
    Sys_UIForm.AddAssetPathEnglish(builder, AssetPath_EnglishOffset);
    Sys_UIForm.AddAssetPathChinese(builder, AssetPath_ChineseOffset);
    Sys_UIForm.AddIsLock(builder, IsLock);
    Sys_UIForm.AddDisableUILayer(builder, DisableUILayer);
    Sys_UIForm.AddName(builder, NameOffset);
    Sys_UIForm.AddDesc(builder, DescOffset);
    Sys_UIForm.AddId(builder, Id);
    Sys_UIForm.AddFreezeMode(builder, FreezeMode);
    Sys_UIForm.AddShowMode(builder, ShowMode);
    Sys_UIForm.AddCanMulit(builder, CanMulit);
    Sys_UIForm.AddUIGroupId(builder, UIGroupId);
    return Sys_UIForm.EndSys_UIForm(builder);
  }

  public static void StartSys_UIForm(FlatBufferBuilder builder) { builder.StartObject(11); }
  public static void AddId(FlatBufferBuilder builder, int Id) { builder.AddInt(0, Id, 0); }
  public static void AddDesc(FlatBufferBuilder builder, StringOffset DescOffset) { builder.AddOffset(1, DescOffset.Value, 0); }
  public static void AddName(FlatBufferBuilder builder, StringOffset NameOffset) { builder.AddOffset(2, NameOffset.Value, 0); }
  public static void AddUIGroupId(FlatBufferBuilder builder, byte UIGroupId) { builder.AddByte(3, UIGroupId, 0); }
  public static void AddDisableUILayer(FlatBufferBuilder builder, int DisableUILayer) { builder.AddInt(4, DisableUILayer, 0); }
  public static void AddIsLock(FlatBufferBuilder builder, int IsLock) { builder.AddInt(5, IsLock, 0); }
  public static void AddAssetPathChinese(FlatBufferBuilder builder, StringOffset AssetPathChineseOffset) { builder.AddOffset(6, AssetPathChineseOffset.Value, 0); }
  public static void AddAssetPathEnglish(FlatBufferBuilder builder, StringOffset AssetPathEnglishOffset) { builder.AddOffset(7, AssetPathEnglishOffset.Value, 0); }
  public static void AddCanMulit(FlatBufferBuilder builder, bool CanMulit) { builder.AddBool(8, CanMulit, false); }
  public static void AddShowMode(FlatBufferBuilder builder, byte ShowMode) { builder.AddByte(9, ShowMode, 0); }
  public static void AddFreezeMode(FlatBufferBuilder builder, byte FreezeMode) { builder.AddByte(10, FreezeMode, 0); }
  public static Offset<Sys_UIForm> EndSys_UIForm(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Sys_UIForm>(o);
  }
};


}