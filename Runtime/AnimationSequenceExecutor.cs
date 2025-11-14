using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationSequenceTool.Runtime
{
    public interface IAnimationSequenceExecutable
    {
        public Type GetDataType();
        public void Execute(ScriptableObject data);
    }
    
    public class AnimationSequenceExecutor : MonoBehaviour
    {
        private readonly Dictionary<Type, IAnimationSequenceExecutable> _animationSequenceExecutors = new();

        private void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var childExecutor = transform.GetChild(i).GetComponent<IAnimationSequenceExecutable>();

                if (childExecutor == null)
                {
                    continue;
                }
                
                _animationSequenceExecutors.Add(childExecutor.GetDataType(), childExecutor);
            }
        }
        
        public void Execute(Type objectType, ScriptableObject data)
        {
            _animationSequenceExecutors[objectType].Execute(data);
        }
    }
}
