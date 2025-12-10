using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnimationSequenceTool.Runtime
{
    [RequireComponent(typeof(Animator))]
    public class AnimationSequenceEvent : MonoBehaviour
    {
        public AnimationSequenceExecutor AnimationSequenceExecutor;
        
        [SerializeField] private AnimationSequenceData _animationSequenceData;
        
        private Animator _animator;
        
        private Dictionary<int, List<SequenceElement>> _cachedSequenceData = new();
        
        private int _currentStateNameHash;
        private float _currentAnimationTime;
        private float _currentAnimationLength;
        private int _currentSequenceDataIndex;
        
        public AnimationSequenceData AnimationSequenceData => _animationSequenceData;
        
        private void Start()
        {
            if (_animationSequenceData == null)
            {
                enabled = false;
                return;
            }
            
            _animator = GetComponent<Animator>();
            _cachedSequenceData = _animationSequenceData.GetRuntimeSequenceData();
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

            if (!_cachedSequenceData.TryGetValue(currentStateNameHash, out List<SequenceElement> currentState))
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
                    AnimationSequenceExecutor.Execute(currentSequenceData.Data.GetType(), currentSequenceData.Data);
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
