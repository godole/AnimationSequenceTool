using UnityEngine;

namespace AnimationSequenceTool.Runtime
{
    public abstract class BasePreviewObject : MonoBehaviour
    {
        public abstract void Simulate(float time);
    }
}
