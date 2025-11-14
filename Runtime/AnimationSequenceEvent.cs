using System.Collections.Generic;
using UnityEngine;

namespace AnimationSequenceTool.Runtime
{
    public class AnimationSequenceEvent : MonoBehaviour
    {
        [SerializeField] private AnimationSequenceExecutor _animationSequenceExecutor;
        
        [SerializeField] private AnimationSequenceData _animationSequenceData;
        [SerializeField] private Animator _animator;
        
        private readonly Dictionary<int, List<SequenceData>> _cachedSequenceData = new();
        
        private int _currentStateNameHash;
        private float _currentAnimationTime;
        private float _currentAnimationLength;
        private int _currentSequenceDataIndex;
        
        public AnimationSequenceData AnimationSequenceData => _animationSequenceData;
        
        private void Start()
        {
            var sortedSequenceData = _animationSequenceData.GetSortedSequenceData();

            foreach (var sequenceData in sortedSequenceData)
            {
                int stateNameHash = Animator.StringToHash(sequenceData.StateName);
                
                if (_cachedSequenceData.TryGetValue(stateNameHash, out List<SequenceData> sequenceDataList))
                {
                    sequenceDataList.Add(sequenceData);
                }
                else
                {
                    _cachedSequenceData.Add(stateNameHash, new List<SequenceData>() { sequenceData });
                }
            }
        }

        private void Update()
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            
            int currentStateNameHash = stateInfo.shortNameHash;
            
            if (_currentStateNameHash != currentStateNameHash)
            {
                _currentStateNameHash = currentStateNameHash;
                _currentAnimationLength = stateInfo.length;
                _currentAnimationTime = 0.0f;
                _currentSequenceDataIndex = 0;
            }

            if (!_cachedSequenceData.TryGetValue(currentStateNameHash, out List<SequenceData> currentState))
            {
                return;
            }
            
            float nextTime = _currentAnimationTime + Time.deltaTime;

            if (_currentSequenceDataIndex >= currentState.Count)
            {
                _currentSequenceDataIndex = 0;
            }

            for (; _currentSequenceDataIndex < currentState.Count; _currentSequenceDataIndex++)
            {
                var currentSequenceData = currentState[_currentSequenceDataIndex];
                float eventTime = _currentAnimationLength * currentSequenceData.EventNormalizedTime;
                
                if (_currentAnimationTime <= eventTime && eventTime <= nextTime)
                {
                    _animationSequenceExecutor.Execute(currentSequenceData.Data.GetType(), currentSequenceData.Data);
                }
                else
                {
                    break;
                }
            }
            
            _currentAnimationTime = nextTime % _currentAnimationLength;
        }

        private void Reset()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                _animator = gameObject.GetComponentInChildren<Animator>();
            }
        }
    }
}
