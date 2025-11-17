using System;
using System.Collections.Generic;
using System.Linq;
using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AnimationSequenceTool.Editor.VisualElements.ControlPanel
{
    public class ControlPanelBindingData
    {
        public AnimationSequenceData AnimationSequenceData { get; }
        public SerializedObject SequenceDataSerializedObject { get; }
        public List<int> FilteredSequenceDataIndices { get; } = new();
        public List<SequenceData> FilteredSequenceDataList { get; } = new();

        public Action<AnimationClip> OnSelectedAnimationChanged;
        public Action OnUpdateCollectionItems;
        
        public string SelectedAnimationStateName { get; private set; }
        public AnimationClip SelectedAnimationClip { get; private set; }
        private readonly AnimatorController _animatorController;
        
        public ControlPanelBindingData(ControlPanel.BindingData bindingData,
            ControlPanel.AnimatorComponentData animatorComponentData)
        {
            AnimationSequenceData = bindingData.AnimationSequenceData;
            _animatorController = animatorComponentData.AnimatorController;
            SequenceDataSerializedObject = new SerializedObject(AnimationSequenceData);
            string initialStateName = GetAnimationStateNames()[0];
            SelectedAnimationStateName = initialStateName;
            SelectedAnimationClip = GetAnimationClipFromController(initialStateName);
        }

        public List<string> GetAnimationStateNames()
        {
            return (from layer in _animatorController.layers from state in layer.stateMachine.states select state.state.name).ToList();
        }

        public void UpdateCollectionItems()
        {
            SequenceDataSerializedObject.ApplyModifiedProperties();
            SequenceDataSerializedObject.Update();
            RefreshCollectionItemList();
            OnUpdateCollectionItems?.Invoke();
        }

        public void RefreshCollectionItemList()
        {
            FilteredSequenceDataList.Clear();
            FilteredSequenceDataIndices.Clear();
                
            for(int i = 0; i < AnimationSequenceData.SequenceData.Count; i++)
            {
                if (!AnimationSequenceData.SequenceData[i].StateName.Equals(SelectedAnimationStateName)) continue;
                    
                FilteredSequenceDataIndices.Add(i);
                FilteredSequenceDataList.Add(AnimationSequenceData.SequenceData[i]);
            }
        }
        
        public void ChangeAnimationState(string stateName)
        {
            SelectedAnimationStateName = stateName;
            RefreshCollectionItemList();
            OnSelectedAnimationChanged?.Invoke(GetAnimationClipFromController(stateName));
        }

        private AnimationClip GetAnimationClipFromController(string stateName)
        {
            foreach (var layer in _animatorController.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (!state.state.name.Equals(stateName))
                    {
                        continue;
                    }
                    
                    SelectedAnimationClip = state.state.motion as AnimationClip;
                }
            }
            
            return SelectedAnimationClip;
        }
    }
}
