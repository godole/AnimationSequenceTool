using AnimationSequenceTool.Editor.Common;
using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.Inspectors
{
    [CustomEditor(typeof(AnimationSequenceEvent))]
    public class AnimationSequenceEventEditor : UnityEditor.Editor
    {
        private ObjectField _animationSequenceDataField;
        private Button _createNewDataBtn;
        private VisualElement _dataInspectorContainer;
        private VisualElement _dataInspector;
        private Label _dataNameLabel;
        
        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AnimationSequenceWindowConstants.UxmlPath + "AnimationSequenceEventEditor.uxml");
            visualTree.CloneTree(rootElement);
            
            _animationSequenceDataField = rootElement.Q<ObjectField>("AnimationSequenceDataField");
            _createNewDataBtn = rootElement.Q<Button>("CreateNewDataBtn");
            _dataInspectorContainer = rootElement.Q<VisualElement>("SequenceDataInspectorContainer");
            _dataInspector = rootElement.Q<VisualElement>("SequenceDataInspector");
            _dataNameLabel = rootElement.Q<Label>("DataNameLabel");
            
            _animationSequenceDataField.BindProperty(serializedObject.FindProperty("_animationSequenceData"));
            _animationSequenceDataField.RegisterValueChangedCallback(_ =>
            {
                RefreshVisualElement();
            });

            RefreshVisualElement();
            
            return rootElement;
        }

        private void RefreshVisualElement()
        {
            if (_animationSequenceDataField.value == null)
            {
                _createNewDataBtn.visible = true;
                _dataInspectorContainer.visible = false;
                return;
            }

            _createNewDataBtn.visible = false;
            _dataInspectorContainer.visible = true;
            
            _dataNameLabel.text = $"{_animationSequenceDataField.value.name} Inspector";

            _dataInspector.Clear();

            var inspector = CreateEditor(_animationSequenceDataField.value);
            _dataInspector.Add(inspector.CreateInspectorGUI());
        }
    }
}
