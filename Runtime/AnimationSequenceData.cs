using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationSequenceTool.Runtime
{
    [Serializable]
    public class SequenceData
    {
        public string StateName;
        public float EventNormalizedTime;
        public ScriptableObject Data;
    }
    
    [CreateAssetMenu(fileName = "Animation Sequence Data", menuName = "Animation Sequence Data")]
    public class AnimationSequenceData : ScriptableObject
    {
        public List<SequenceData> SequenceData = new List<SequenceData>();

        public List<SequenceData> GetSortedSequenceData()
        {
            var sortedSequenceData = new List<SequenceData>(SequenceData);
            
            sortedSequenceData.Sort((a, b) => a.EventNormalizedTime.CompareTo(b.EventNormalizedTime));
            
            return sortedSequenceData;
        }
    }
}
