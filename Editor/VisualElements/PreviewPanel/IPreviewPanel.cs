using UnityEngine;

namespace AnimationSequenceTool.Editor.VisualElements.PreviewPanel
{
    public interface IPreviewPanel
    {
        void AddParticleObject(int instanceId, GameObject previewObject, float startTime);
        void RemoveParticleObject(int instanceId);
        void ChangeNormalizedTime(int instanceId, float normalizedTime);
    }
}
