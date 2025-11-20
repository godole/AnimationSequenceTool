using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnimationSequenceTool.Runtime
{
    [Serializable]
    public class SequenceElement
    {
        public string StateName;
        public float EventNormalizedTime;
        public ScriptableObject Data;
    }
    
    [CreateAssetMenu(fileName = "Animation Sequence Data", menuName = "Animation Sequence Data")]
    public class AnimationSequenceData : ScriptableObject
    {
        public SerializableDictionary<string, SequenceElement> SequenceData = new SerializableDictionary<string, SequenceElement>();

        public Dictionary<int, List<SequenceElement>> GetRuntimeSequenceData()
        {
            var runtimeSequenceData = new Dictionary<int, List<SequenceElement>>();

            foreach (var dictionaryElement in SequenceData.Dictionary)   
            {
                var sortedSequenceElements = new List<SequenceElement>(dictionaryElement.Value.Values);
                sortedSequenceElements.Sort((a, b) => a.EventNormalizedTime.CompareTo(b.EventNormalizedTime));
                runtimeSequenceData.Add(Animator.StringToHash(dictionaryElement.Key), sortedSequenceElements);
            }
            
            return runtimeSequenceData;
        }
    }
}
