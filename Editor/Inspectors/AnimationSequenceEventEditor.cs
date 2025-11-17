using System.Collections.Generic;
using System.Linq;
using AnimationSequenceTool.Editor.Common;
using AnimationSequenceTool.Editor.Utilities;
using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Slider = UnityEngine.UIElements.Slider;

namespace AnimationSequenceTool.Editor.Inspectors
{
    [CustomEditor(typeof(AnimationSequenceEvent))]
    public class AnimationSequenceEventEditor : UnityEditor.Editor
    {
        private VisualElement _cachedRootElement;
        private AnimationSequenceEvent _animationSequenceEvent;
        private Animator _targetAnimator;
        
        private readonly List<SequenceData> _selectedSequenceDataList = new();
        private readonly List<int> _selectedSequenceDataIndexList = new();
        private string _selectedAnimationState;

        public override VisualElement CreateInspectorGUI()
        {
            if (_cachedRootElement != null)
            {
                return _cachedRootElement;
            }
            
            var rootElement = new VisualElement();
            _cachedRootElement = rootElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AnimationSequenceWindowConstants.UxmlPath + "AnimationSequenceEventEditor.uxml");
            visualTree.CloneTree(rootElement);
            
            var sequenceDataInspector = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AnimationSequenceWindowConstants.UxmlPath + "AnimationSequenceDataEditor.uxml");
            
            _animationSequenceEvent = (AnimationSequenceEvent)target;
            if (_animationSequenceEvent == null || _animationSequenceEvent.AnimationSequenceData == null)
            {
                rootElement.Add(new Label("No AnimationSequenceData assigned."));
                return rootElement;
            }

            _targetAnimator = AnimatorEditorUtility.FindTargetAnimator(_animationSequenceEvent.gameObject);

            if (_targetAnimator == null)
            {
                return rootElement;
            }

            List<string> animationStates = new();
            var stateNameDropdownField = rootElement.Q<DropdownField>("AnimationStateDropdownField");

            var baseController = AnimatorEditorUtility.GetOriginAnimatorController(_targetAnimator);
            
            if (baseController != null)
            {
                animationStates.AddRange(from layer in baseController.layers from state in layer.stateMachine.states select state.state.name);
            }
            
            stateNameDropdownField.choices = animationStates;
            _selectedAnimationState = animationStates.First();
            stateNameDropdownField.value = _selectedAnimationState;
            
            RefreshSelectedAnimationState(stateNameDropdownField.value);
            
            // ScriptableObject를 SerializedObject로 래핑
            var sequenceDataList = new SerializedObject(_animationSequenceEvent.AnimationSequenceData);
            var sequenceDataListProperty = sequenceDataList.FindProperty("SequenceData");
            
            var sequenceDataField = rootElement.Q<ObjectField>("SequenceDataField");
            sequenceDataField.objectType = typeof(AnimationSequenceData);
            sequenceDataField.BindProperty(serializedObject.FindProperty("_animationSequenceData"));

            // ListView 생성
            var listView = rootElement.Q<ListView>("SequenceDataList");
            listView.reorderable = true;
            listView.reorderMode = ListViewReorderMode.Animated;

            listView.itemsSource = _selectedSequenceDataList;
            
            listView.makeItem = () => sequenceDataInspector.Instantiate();

            listView.bindItem = (element, i) =>
            {
                var itemProp = sequenceDataListProperty.GetArrayElementAtIndex(_selectedSequenceDataIndexList[i]);
                
                var stateNameField = element.Q<TextField>("StateNameField");
                stateNameField.BindProperty(itemProp.FindPropertyRelative("StateName"));
                
                var eventTimeField = element.Q<Slider>("EventTimeField");
                eventTimeField.BindProperty(itemProp.FindPropertyRelative("EventNormalizedTime"));

                var dataField = element.Q<ObjectField>("DataField");
                dataField.BindProperty(itemProp.FindPropertyRelative("Data"));
                
                var inspectorContainer = element.Q<VisualElement>("InspectorContainer");
                inspectorContainer.Clear();
                
                var dataProp = itemProp.FindPropertyRelative("Data");
                if (dataProp.objectReferenceValue is ScriptableObject so)
                {
                    var editor = CreateEditor(so);
                    if (editor != null)
                    {
                        inspectorContainer.Add(new InspectorElement(editor));
                    }
                }

                dataField.RegisterValueChangedCallback((evt) =>
                {
                    inspectorContainer.Clear();
                    var editor = CreateEditor(evt.newValue);
                    if (editor != null)
                    {
                        inspectorContainer.Add(new InspectorElement(editor));
                    }
                });
            };
            
            listView.itemsAdded += indices =>
            {
                foreach (int idx in indices)
                {
                    sequenceDataListProperty.InsertArrayElementAtIndex(idx);
                    
                    var newElement = sequenceDataListProperty.GetArrayElementAtIndex(idx);
                    
                    var stateNameField = newElement.FindPropertyRelative("StateName");
                    stateNameField.stringValue = stateNameDropdownField.value;

                    var dataField = newElement.FindPropertyRelative("Data");
                    dataField.objectReferenceValue = null;
                    
                    var eventTimeField = newElement.FindPropertyRelative("EventNormalizedTime");
                    eventTimeField.floatValue = 0.0f;
                    
                    _selectedSequenceDataIndexList.Add(idx);
                }
                
                sequenceDataList.ApplyModifiedProperties();
            };
            
            listView.itemsRemoved += indices =>
            {
                foreach (int idx in indices.OrderByDescending(i => i))
                {
                    sequenceDataListProperty.DeleteArrayElementAtIndex(idx);
                    _selectedSequenceDataIndexList.Remove(idx);
                }
                
                sequenceDataList.ApplyModifiedProperties();
            };
            
            stateNameDropdownField.RegisterValueChangedCallback((changeEvent) =>
            {
                _selectedAnimationState = changeEvent.newValue;

                RefreshSelectedAnimationState(_selectedAnimationState);
                
                listView.RefreshItems();
            });

            rootElement.Add(listView);
            
            return rootElement;
        }

        private void RefreshSelectedAnimationState(string animationState)
        {
            _selectedSequenceDataList.Clear();
            _selectedSequenceDataIndexList.Clear();
            
            for (int i = 0; i < _animationSequenceEvent.AnimationSequenceData.SequenceData.Count; i++)
            {
                if (!_animationSequenceEvent.AnimationSequenceData.SequenceData[i].StateName
                        .Equals(animationState)) continue;
                
                _selectedSequenceDataList.Add(_animationSequenceEvent.AnimationSequenceData.SequenceData[i]);
                _selectedSequenceDataIndexList.Add(i);
            }
        }
    }
}
