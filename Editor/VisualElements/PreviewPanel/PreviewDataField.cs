using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.VisualElements.PreviewPanel
{
    [UxmlElement]
    public partial class PreviewDataField : ObjectField
    {
        private SerializedProperty _animationSequenceDataProperty;
        private IPreviewPanel _animationPreviewPanel;

        public PreviewDataField()
        {
            
        }

        public void BindDataFieldProperty(IPreviewPanel animationPreviewPanel, SerializedProperty animationSequenceDataProperty)
        {
            _animationPreviewPanel = animationPreviewPanel;
            _animationSequenceDataProperty = animationSequenceDataProperty;
            
            this.BindProperty(animationSequenceDataProperty.FindPropertyRelative("Data"));
            this.RegisterValueChangedCallback(ChangeEvent);
        }

        public void UnbindProperty()
        {
            if (userData is int instanceId)
            {
                _animationPreviewPanel.RemoveParticleObject(instanceId);
            }
            this.Unbind();
            this.UnregisterValueChangedCallback(ChangeEvent);
        }

        private void CreatePreviewObject()
        {
            if (_animationSequenceDataProperty.FindPropertyRelative("Data").objectReferenceValue is not IAnimationSequenceEventData previewData)
            {
                return;
            }
                
            var previewInstance = previewData.CreatePreviewObject();

            if (previewInstance == null)
            {
                return;
            }
            
            var normalizedTimeProperty = _animationSequenceDataProperty.FindPropertyRelative("EventNormalizedTime");
            var bindingElement = this.Q<VisualElement>("bindingElement");
            bindingElement?.RemoveFromHierarchy();

            bindingElement = new VisualElement()
            {
                name = "bindingElement"
            };
            Add(bindingElement);
                    
            _animationPreviewPanel.AddParticleObject(previewInstance.GetInstanceID(), previewInstance, normalizedTimeProperty.floatValue);

            bindingElement.TrackPropertyValue(normalizedTimeProperty, serializedProperty =>
            {
                _animationPreviewPanel.ChangeNormalizedTime(previewInstance.GetInstanceID(), serializedProperty.floatValue);
            });
            userData = previewInstance.GetInstanceID();
        }

        private void ChangeEvent(ChangeEvent<Object> changeEvent)
        {
            if (changeEvent.previousValue == null && changeEvent.newValue == null)
            {
                return;
            }

            if (changeEvent.previousValue != null)
            {
                if (userData is int instanceId)
                {
                    _animationPreviewPanel.RemoveParticleObject(instanceId);    
                }
            }

            if (changeEvent.newValue != null)
            {
                CreatePreviewObject();
            }
        }
    }
}
