using System;
using UnityEngine.UIElements;

namespace Framework.AnimationSequenceEvent.Editor.Utilities
{
    public static class MouseDragHandler
    {
        /// <summary>
        /// 마우스 드래그 이벤트를 등록해줍니다
        /// </summary>
        /// <param name="element"></param>
        /// <param name="onMouseDown">드래그가 시작될때 호출됩니다</param>
        /// <param name="onMouseMove">드래그 도중 호출됩니다.\n
        /// 주의할 점은, 위치를 이동시키는 로직은 콜백에서 따로 처리를 해야합니다.</param>
        /// <param name="onMouseUp">마우스가 릴리즈되면 호출됩니다</param>
        public static MouseDragController RegisterMouseEvent(this VisualElement element, Action<MouseDownEvent> onMouseDown,
            Action<MouseMoveEvent> onMouseMove, Action<MouseUpEvent> onMouseUp)
        {
            return new MouseDragController(element, onMouseDown, onMouseMove, onMouseUp);
        }

        public class MouseDragController
        {
            public int TargetMouseIndex = 0;
            
            private readonly VisualElement _target;
            private bool _isDragging;

            public MouseDragController(VisualElement target, Action<MouseDownEvent> onMouseDown,
                Action<MouseMoveEvent> onMouseMove, Action<MouseUpEvent> onMouseUp)
            {
                _target = target;
                _target.RegisterCallback<MouseDownEvent>(evt => OnMouseDown(evt, onMouseDown));
                _target.RegisterCallback<MouseMoveEvent>(evt => OnMouseMove(evt, onMouseMove));
                _target.RegisterCallback<MouseUpEvent>(evt => OnMouseUp(evt, onMouseUp));
            }
            
            public void OnMouseDown(MouseDownEvent evt, Action<MouseDownEvent> onMouseDown)
            {
                if (evt.button != TargetMouseIndex)
                {
                    return;
                }
                
                _target.CaptureMouse();
                _isDragging = true;
                evt.StopPropagation();
                onMouseDown?.Invoke(evt);
            }

            public void OnMouseMove(MouseMoveEvent evt, Action<MouseMoveEvent> onMouseDown)
            {
                if (!_isDragging)
                {
                    return;
                }
            
                onMouseDown?.Invoke(evt);
            }

            public void OnMouseUp(MouseUpEvent evt, Action<MouseUpEvent> onMouseUp)
            {
                if (!_isDragging)
                {
                    return;
                }
            
                _isDragging = false;
                _target.ReleaseMouse();
                evt.StopPropagation();
                onMouseUp?.Invoke(evt);
            }
        }
    }
}
