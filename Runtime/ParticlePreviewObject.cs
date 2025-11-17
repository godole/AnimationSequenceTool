using UnityEngine;

namespace AnimationSequenceTool.Runtime
{
    public class ParticlePreviewObject : BasePreviewObject
    {
        private ParticleSystem _particleSystem;

        public override void Simulate(float time)
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
                
                if (_particleSystem == null)
                {
                    return;
                }
            }

            _particleSystem.Simulate(time, true, true);
        }
    }
}
