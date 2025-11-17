using System;
using AnimationSequenceTool.Editor.Common;
using Framework.AnimationSequenceEvent.Editor.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.VisualElements
{
    public class TimelineTrack : VisualElement
    {
        private SerializedProperty _eventNormalizedProperty;
        private VisualElement _marker;
        
        public Action<MouseMoveEvent> OnMouseMove;
        
        public TimelineTrack()
        {
            this.RegisterMouseEvent(null, UpdateProperty, null);
        }
        
        public void BindProperty(SerializedProperty property)
        {
            _eventNormalizedProperty = property.FindPropertyRelative("EventNormalizedTime");

            _marker?.RemoveFromHierarchy();
            _marker = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            _marker.AddToClassList("track_marker");

            _marker.RegisterCallback<GeometryChangedEvent>(_ => RepositionCallback(_eventNormalizedProperty));
            _marker.TrackPropertyValue(_eventNormalizedProperty, RepositionCallback);
            
            Add(_marker);
            return;

            void RepositionCallback(SerializedProperty p)
            {
                _marker.style.left = p.floatValue * AnimationSequenceWindowConstants.TrackWidth - _marker.resolvedStyle.width * 0.5f - resolvedStyle.borderLeftWidth;
            }
        }
        
        private void UpdateProperty(MouseMoveEvent evt)
        {
            var dragCurrentPosition = evt.localMousePosition;
            _eventNormalizedProperty.floatValue = Mathf.Clamp01(dragCurrentPosition.x / AnimationSequenceWindowConstants.TrackWidth);
            _eventNormalizedProperty.serializedObject.ApplyModifiedProperties();
            OnMouseMove?.Invoke(evt);
        }
    }
}
