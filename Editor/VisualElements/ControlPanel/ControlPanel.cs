using System;
using AnimationSequenceTool.Editor.Common;
using AnimationSequenceTool.Editor.VisualElements.PreviewPanel;
using AnimationSequenceTool.Runtime;
using Framework.AnimationSequenceEvent.Editor.Utilities;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.VisualElements.ControlPanel
{
    /// <summary>
    /// 이벤트 데이터를 조작하는 패널입니다.
    /// </summary>
    [UxmlElement]
    public partial class ControlPanel : VisualElement 
    {
        internal struct TrackRemoveBtnUserData
        {
            public int BindingDataIndex;
        }
        
        /// <summary>
        /// 바인딩에 필요한 데이터들입니다
        /// </summary>
        public class BindingData
        {
            /// <summary>
            /// <see cref="AnimationSequenceEvent"/>컴포넌트에 있는 <see cref="AnimationSequenceData"/>입니다
            /// </summary>
            public AnimationSequenceData AnimationSequenceData;
        }

        /// <summary>
        /// 프리뷰 및 이벤트 바인딩에 필요한 애니메이터 컴포넌트 입니다.
        /// </summary>
        public class AnimatorComponentData
        {
            public AnimatorController AnimatorController;
        }
        
        private sealed class MarkerBinding
        {
            public VisualElement Marker; 
        }

        public Action<AnimationClip> OnSelectedAnimationChanged
        {
            get =>ControlPanelBindingData.OnSelectedAnimationChanged;
            set => ControlPanelBindingData.OnSelectedAnimationChanged = value;
        }
        public Action<PreviewDataField, SerializedProperty> BindDataField;

        public EditorWindowStopwatch Timer { get; } = new();
        
        // VisualElement Cache
        private readonly VisualElement _rootElement;
        
        private VisualElement _markerContainer;
        private VisualElement _timelineBar;
        private ListView _dataFieldListView;
        private ListView _trackListView;
        
        //Data
        public ControlPanelBindingData ControlPanelBindingData { get; private set; }

        public ControlPanel()
        {
            Timer.MaxTime = 1f;
            var controlPanelAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                AnimationSequenceWindowConstants.UxmlPath + "ControlPanel.uxml");
            _rootElement = controlPanelAsset.CloneTree();
            Add(_rootElement);
        }

        /// <summary>
        /// VisualElement들을 캐싱하고 이벤트 및 데이터 바인딩을 진행합니다
        /// </summary>
        /// <param name="bindingData">프리뷰 루트 인스턴스에 포함되어있는 데이터로 <see cref="BindingData"/>를 초기화 해줍니다.</param>
        /// <param name="animatorComponentData"><see cref="AnimatorComponentData"/></param>
        public void InitializeVisualElements(BindingData bindingData, AnimatorComponentData animatorComponentData)
        {
            ControlPanelBindingData = new ControlPanelBindingData(bindingData, animatorComponentData)
            {
                OnUpdateCollectionItems = () =>
                {
                    _dataFieldListView.RefreshItems();
                    _trackListView.RefreshItems();
                    _markerContainer.Clear();
                }
            };

            var dataFieldAsset =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    AnimationSequenceWindowConstants.UxmlPath + "DataField.uxml");

            _timelineBar = _rootElement.Q<VisualElement>("TimelineBar");
            _markerContainer = _rootElement.Q<VisualElement>("MarkerContainer");
            
            var timelineContainer = _rootElement.Q("TimeTrack");
            timelineContainer.RegisterMouseEvent(null, OnMouseMove, null);

            var animationStates = ControlPanelBindingData.GetAnimationStateNames();

            var animationToolbar = _rootElement.Q<VisualElement>("AnimationToolbar");
            var selectStateDropdown = animationToolbar.Q<DropdownField>("SelectStateDropdown");
            selectStateDropdown.choices = animationStates;
            selectStateDropdown.value = animationStates[0];
            selectStateDropdown.RegisterValueChangedCallback(evt =>
            {
                string newValue = evt.newValue.ToString();
                ControlPanelBindingData.ChangeAnimationState(newValue);
                _dataFieldListView.RefreshItems();
                _trackListView.RefreshItems();
            });

            var toolbarContainer = _rootElement.Q<VisualElement>("ToolbarContainer");

            var playBtn = toolbarContainer.Q<Button>("Play");
            playBtn.RegisterCallback<ClickEvent>(_ => Timer.Play());
            
            var pauseBtn = toolbarContainer.Q<Button>("Pause");
            pauseBtn.RegisterCallback<ClickEvent>(_ => Timer.Pause());
            
            var stopBtn = toolbarContainer.Q<Button>("Stop");
            stopBtn.RegisterCallback<ClickEvent>(_ => Timer.Stop());
            
            _trackListView = _rootElement.Q<ListView>("TrackListView");
            _trackListView.itemsSource = ControlPanelBindingData.FilteredSequenceDataList;
            _trackListView.makeItem = () =>
            {
                var track = new TimelineTrack
                {
                    OnMouseMove = OnMouseMove
                };
                track.AddToClassList("Timeline_Track");
                track.AddToClassList("Track");
                return track;
            };
            _trackListView.bindItem = (element, i) =>
            {
                var timelineTrack = (TimelineTrack)element;
                int filteredIndex = ControlPanelBindingData.FilteredSequenceDataIndices[i];
                var bindProperty = ControlPanelBindingData.SequenceDataSerializedObject.FindProperty("SequenceData").GetArrayElementAtIndex(filteredIndex);

                timelineTrack.BindProperty(bindProperty);
                element.userData = BindTrackMarker(bindProperty);
            };
            _trackListView.unbindItem = (element, _) =>
            {
                element.Unbind();

                if (element.userData is not MarkerBinding binding) return;

                binding.Marker.RemoveFromHierarchy();
                element.userData = null;
            };
            
            _dataFieldListView = _rootElement.Q<ListView>("DataFieldListView");
            _dataFieldListView.itemsSource = ControlPanelBindingData.FilteredSequenceDataList;
            _dataFieldListView.makeItem = () =>
            {
                var dataTrack = dataFieldAsset.CloneTree();
            
                var removeDataButton = dataTrack.Q<Button>("RemoveDataButton");
                removeDataButton.clicked += () =>
                {
                    var removeBtnData = (TrackRemoveBtnUserData)removeDataButton.userData;
            
                    ControlPanelBindingData.SequenceDataSerializedObject.FindProperty("SequenceData")
                        .DeleteArrayElementAtIndex(removeBtnData.BindingDataIndex);
                    ControlPanelBindingData.UpdateCollectionItems();
                };
            
                return dataTrack;
            };
            _dataFieldListView.bindItem = (element, i) =>
            {
                int filteredIndex = ControlPanelBindingData.FilteredSequenceDataIndices[i];
                var property = ControlPanelBindingData.SequenceDataSerializedObject.FindProperty("SequenceData").GetArrayElementAtIndex(filteredIndex);
            
                var dataField = element.Q<PreviewDataField>("DataField");
                
                BindDataField.Invoke(dataField, property);
            
                var removeDataButton = element.Q<Button>("RemoveDataButton");
                var userdata = new TrackRemoveBtnUserData()
                {
                    BindingDataIndex = filteredIndex
                };
                removeDataButton.userData = userdata;
            };
            _dataFieldListView.unbindItem = (element, _) =>
            {
                var dataField = element.Q<PreviewDataField>("DataField");
                dataField.UnbindProperty();
                element.Unbind();
            };
            
            var listviewAddButton = _rootElement.Q<Button>("DataFieldAddButton");
            listviewAddButton.clicked += () =>
            {
                var newSequenceData = new SequenceData
                {
                    StateName = selectStateDropdown.value
                };
                ControlPanelBindingData.AnimationSequenceData.SequenceData.Add(newSequenceData);
                ControlPanelBindingData.UpdateCollectionItems();
            };
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            float left = evt.localMousePosition.x - _timelineBar.resolvedStyle.width * 0.5f;
            left = Mathf.Clamp(left, 0, AnimationSequenceWindowConstants.TrackWidth);
            _timelineBar.style.left = left;
            Timer.Pause();
            Timer.ChangeTime(left / AnimationSequenceWindowConstants.TrackWidth);
        }

        public void UpdateTime(float deltaTime)
        {
            Timer.UpdateTime(deltaTime);
            _timelineBar.style.left = Timer.Time * AnimationSequenceWindowConstants.TrackWidth;
        }

        private MarkerBinding BindTrackMarker(SerializedProperty property)
        {
            var eventNormalizedProperty = property.FindPropertyRelative("EventNormalizedTime");
            
            var marker = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            marker.RegisterCallback<GeometryChangedEvent>(_ => Reposition(eventNormalizedProperty));
            marker.TrackPropertyValue(eventNormalizedProperty, Reposition);
            marker.AddToClassList("track_marker");
            _markerContainer.Add(marker);

            return new MarkerBinding()
            {
                Marker = marker
            };

            void Reposition(SerializedProperty p)
            {
                marker.style.left = p.floatValue * AnimationSequenceWindowConstants.TrackWidth -
                                    marker.resolvedStyle.width * 0.5f;
            }
        }
    }
}
