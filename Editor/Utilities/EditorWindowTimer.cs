using UnityEditor;

namespace Framework.AnimationSequenceEvent.Editor.Utilities
{
    public class EditorWindowTimer
    {
        public float DeltaTime { get; private set; }
        private float _lastEditorStartTime;

        public void UpdateTimer()
        {
            float currentTime = (float)EditorApplication.timeSinceStartup;
            float deltaTime = currentTime - _lastEditorStartTime;
            _lastEditorStartTime = currentTime;
            DeltaTime = deltaTime;
        }
    }
}
