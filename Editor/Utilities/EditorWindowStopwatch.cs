namespace AnimationSequenceTool.Editor.Utilities
{
    public class EditorWindowStopwatch
    {
        private bool _isPlay;

        public float Time { get; private set; }
        public float MaxTime;
        
        public void Play()
        {
            _isPlay = true;
        }

        public void Pause()
        {
            _isPlay = false;
        }

        public void Stop()
        {
            _isPlay = false;
            Time = 0.0f;
        }

        public void ChangeTime(float time)
        {
            Time = time;
        }

        public void UpdateTime(float deltaTime)
        {
            if (!_isPlay)
            {
                return;
            }
            
            Time += deltaTime;

            if (Time >= MaxTime)
            {
                Time = 0.0f;
            }
        }
    }
}
