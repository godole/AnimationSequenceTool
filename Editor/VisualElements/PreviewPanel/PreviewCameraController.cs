using Framework.AnimationSequenceEvent.Editor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.VisualElements.PreviewPanel
{
    public class PreviewCameraController
    {
        public float MoveSpeed;
        public float ZoomSpeed;
        public float RotationSpeed;
        
        private readonly Camera _camera;
        
        public PreviewCameraController(VisualElement target, Camera camera)
        {
            _camera = camera;

            var rightHandler = target.RegisterMouseEvent(null, OnMouseRightMove, null);
            rightHandler.TargetMouseIndex = 1;

            var wheelHandler = target.RegisterMouseEvent(null, OnMouseMiddleMove, null);
            wheelHandler.TargetMouseIndex = 2;
            
            target.RegisterCallback<WheelEvent>(OnMouseWheel);
        }
        
        private void OnMouseWheel(WheelEvent evt)
        {
            _camera.transform.position += _camera.transform.forward * ZoomSpeed * -evt.delta.y;
        }
        
        private void OnMouseMiddleMove(MouseMoveEvent evt)
        {
            Vector3 xMove = -_camera.transform.right * evt.mouseDelta.x * MoveSpeed;
            Vector3 yMove = _camera.transform.up * evt.mouseDelta.y * MoveSpeed;
            _camera.transform.position += xMove + yMove;
        }
        
        private void OnMouseRightMove(MouseMoveEvent evt)
        {
            _camera.transform.eulerAngles += new Vector3(evt.mouseDelta.y, evt.mouseDelta.x, 0.0f) * RotationSpeed;
        }
    }
}
