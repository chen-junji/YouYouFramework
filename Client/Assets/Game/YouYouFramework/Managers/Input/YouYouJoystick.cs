using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace YouYou
{
    public class YouYouJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float maxRadius = 100; //Handle 移动最大半径
        public Direction activatedAxis = (Direction)(-1); //选择激活的轴向
        [SerializeField] bool dynamic = true; // 动态摇杆
        [SerializeField] Transform handle; //摇杆
        [SerializeField] Transform backGround; //背景
        public Action<Vector2> OnChanged; //事件 ： 摇杆被 拖拽时
        public Action<Vector2> OnDown; // 事件： 摇杆被按下时
        public Action<Vector2> OnUp; //事件 ： 摇杆上抬起时
        public bool IsDraging { get { return fingerId != int.MinValue; } } //摇杆拖拽状态


        private void Awake()
        {
            backGround.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            backGroundOriginLocalPostion = backGround.localPosition;
        }
        void OnDisable()
        {
            RestJoystick(); //意外被 Disable 各单位需要被重置
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerId < -1 || IsDraging) return; //适配 Touch：只响应一个Touch；适配鼠标：只响应左键
            fingerId = eventData.pointerId;
            pointerDownPosition = eventData.position;
            if (dynamic)
            {
                //pointerDownPosition[2] = eventData.pressEventCamera?.WorldToScreenPoint(backGround.position).z ?? backGround.position.z;
                backGround.position = eventData.pressEventCamera?.ScreenToWorldPoint(pointerDownPosition) ?? pointerDownPosition;
            }
            OnDown?.Invoke(eventData.position);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (fingerId != eventData.pointerId) return;
            Vector2 direction = eventData.position - (Vector2)pointerDownPosition; //得到pointerDownPos 指向 OnDragPos 的向量
            float radius = Mathf.Clamp(Vector3.Magnitude(direction), 0, maxRadius); //获取并锁定向量的长度 以控制 Handle 半径
            Vector2 localPosition = new Vector2()
            {
                x = (0 != (activatedAxis & Direction.Horizontal)) ? (direction.normalized * radius).x : 0, //确认是否激活水平轴向
                y = (0 != (activatedAxis & Direction.Vertical)) ? (direction.normalized * radius).y : 0    //确认是否激活垂直轴向
            };
            handle.localPosition = localPosition;      //更新 Handle 位置
            UpdateVirtualAxes();
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (fingerId != eventData.pointerId) return;//正确的手指抬起时才会重置摇杆；
            RestJoystick();
            OnUp?.Invoke(eventData.position);
        }

        void UpdateVirtualAxes()
        {
            Vector2 delta = handle.localPosition / maxRadius;
            if (GameEntry.Input != null)
            {
                GameEntry.Input.SetAxis(ConstInput.Horizontal, delta.x);
                GameEntry.Input.SetAxis(ConstInput.Vertical, delta.y);
            }
            OnChanged?.Invoke(delta);
        }

        public void RestJoystick()
        {
            backGround.localPosition = backGroundOriginLocalPostion;
            handle.localPosition = Vector3.zero;
            fingerId = int.MinValue;
            UpdateVirtualAxes();
        }

        private Vector3 backGroundOriginLocalPostion, pointerDownPosition;
        private int fingerId = int.MinValue; //当前触发摇杆的 pointerId ，预设一个永远无法企及的值
        [Flags]
        public enum Direction
        {
            Horizontal = 1 << 0,
            Vertical = 1 << 1
        }
    }
}