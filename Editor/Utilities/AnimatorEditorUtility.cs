using UnityEditor.Animations;
using UnityEngine;

namespace Framework.AnimationSequenceEvent.Editor.Utilities
{
    public static class AnimatorEditorUtility
    {
        public static AnimatorController GetOriginAnimatorController(Animator animator)
        {
            var baseController = animator.runtimeAnimatorController as AnimatorController;
            if (baseController != null) return baseController;
            
            var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController != null)
            {
                baseController = overrideController.runtimeAnimatorController as AnimatorController;
            }

            return baseController;
        }
        
        public static Animator FindTargetAnimator(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<Animator>(out var rootAnimator))
            {
                return rootAnimator;
            }
            
            var childAnimator = gameObject.GetComponentInChildren<Animator>(true);

            if (childAnimator != null)
            {
                return childAnimator;
            }
            
            Debug.LogError("GameObject에 애니메이터가 없습니다");

            return null;
        }
    }
}
