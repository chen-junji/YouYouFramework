using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class HollowOutMask : Graphic, ICanvasRaycastFilter
{
	private RectTransform outer_trans;//背景区域

	[Header("镂空区域")]
	[Space(25)]
	public RectTransform inner_trans;

	[Header("是否实时刷新")]
	[Space(25)]
	public bool realtimeRefresh = false;

	/// <summary>
	/// 是否穿透点击
	/// </summary>
	protected bool IsAcross = true;

	private Vector2 inner_rt;//镂空区域的右上角坐标
	private Vector2 inner_lb;//镂空区域的左下角坐标
	private Vector2 outer_rt;//背景区域的右上角坐标
	private Vector2 outer_lb;//背景区域的左下角坐标

	protected override void Awake()
	{
		base.Awake();
		color = new Color(0, 0, 0, 0.85f);
		outer_trans = GetComponent<RectTransform>();

#if UNITY_EDITOR
		realtimeRefresh = true;
#endif

		//计算边界
		CalcBounds();
	}
	private void Update()
	{
		if (realtimeRefresh == false) return;

		//计算边界
		CalcBounds();
		//刷新
		SetAllDirty();
	}
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		if (inner_trans == null)
		{
			base.OnPopulateMesh(vh);
			return;
		}

		vh.Clear();

		UIVertex vertex = UIVertex.simpleVert;
		vertex.color = color;

		//0 outer左下角
		vertex.position = new Vector3(outer_lb.x, outer_lb.y);
		vh.AddVert(vertex);
		//1 outer左上角
		vertex.position = new Vector3(outer_lb.x, outer_rt.y);
		vh.AddVert(vertex);
		//2 outer右上角
		vertex.position = new Vector3(outer_rt.x, outer_rt.y);
		vh.AddVert(vertex);
		//3 outer右下角
		vertex.position = new Vector3(outer_rt.x, outer_lb.y);
		vh.AddVert(vertex);
		//4 inner左下角
		vertex.position = new Vector3(inner_lb.x, inner_lb.y);
		vh.AddVert(vertex);
		//5 inner左上角
		vertex.position = new Vector3(inner_lb.x, inner_rt.y);
		vh.AddVert(vertex);
		//6 inner右上角
		vertex.position = new Vector3(inner_rt.x, inner_rt.y);
		vh.AddVert(vertex);
		//7 inner右下角
		vertex.position = new Vector3(inner_rt.x, inner_lb.y);
		vh.AddVert(vertex);

		//绘制三角形
		vh.AddTriangle(0, 1, 4);
		vh.AddTriangle(1, 4, 5);
		vh.AddTriangle(1, 5, 2);
		vh.AddTriangle(2, 5, 6);
		vh.AddTriangle(2, 6, 3);
		vh.AddTriangle(6, 3, 7);
		vh.AddTriangle(4, 7, 3);
		vh.AddTriangle(0, 4, 3);
	}
	/// <summary>
	/// 过滤掉射线检测
	/// </summary>
	bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
	{
		if (inner_trans == null || !IsAcross)
		{
			return true;
		}

		return !RectTransformUtility.RectangleContainsScreenPoint(inner_trans, screenPos, eventCamera);
	}

	/// <summary>
	/// 计算边界
	/// </summary>
	private void CalcBounds()
	{
		if (inner_trans == null)
		{
			return;
		}

		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(outer_trans, inner_trans);
		inner_rt = bounds.max;
		inner_lb = bounds.min;
		outer_rt = outer_trans.rect.max;
		outer_lb = outer_trans.rect.min;
	}
}
