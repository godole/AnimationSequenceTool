using AnimationSequenceTool.Editor.Common;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.VisualElements
{
    [UxmlElement]
    public partial class TimeTrack : VisualElement
    {
        private const int TimeLabelCount = 10;
        private const int TimeBarCount = TimeLabelCount * 5;

        public TimeTrack()
        {
            name = "TimeTrack";
            style.flexGrow = 1;
            
            for (int i = 0; i < TimeLabelCount; i++)
            {
                var text = new Label
                {
                    text = i.ToString(),
                    style =
                    {
                        left = i * (AnimationSequenceWindowConstants.TrackWidth / TimeLabelCount)
                    },
                    pickingMode = PickingMode.Ignore
                };
                
                text.AddToClassList("Timeline_TimeLabel");
                Add(text);
            }
            
            for (int i = 0; i < TimeBarCount; i++)
            {
                var bar = new VisualElement
                {
                    style =
                    {
                        left = i * (AnimationSequenceWindowConstants.TrackWidth / TimeBarCount)
                    },
                    pickingMode = PickingMode.Ignore
                };
                
                bar.AddToClassList("Timeline_TimeBar");
                Add(bar);
            }
        }
    }
}
