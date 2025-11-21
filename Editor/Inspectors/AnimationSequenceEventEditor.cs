using AnimationSequenceTool.Editor.Common;
using AnimationSequenceTool.Editor.Utilities;
using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.Inspectors
{
    [CustomEditor(typeof(AnimationSequenceEvent))]
    public class AnimationSequenceEventEditor : UnityEditor.Editor
    {
        private AnimationSequenceEvent _target;
        
        private ObjectField _animationSequenceDataField;
        private Button _createNewDataBtn;
        private VisualElement _dataInspectorContainer;
        private VisualElement _dataInspector;
        private Label _dataNameLabel;
        
        public override VisualElement CreateInspectorGUI()
        {
            _target = (AnimationSequenceEvent)target;
            
            var rootElement = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AnimationSequenceWindowConstants.UxmlPath + "AnimationSequenceEventEditor.uxml");
            visualTree.CloneTree(rootElement);
            
            _animationSequenceDataField = rootElement.Q<ObjectField>("AnimationSequenceDataField");
            _createNewDataBtn = rootElement.Q<Button>("CreateNewDataBtn");
            _dataInspectorContainer = rootElement.Q<VisualElement>("SequenceDataInspectorContainer");
            _dataInspector = rootElement.Q<VisualElement>("SequenceDataInspector");
            _dataNameLabel = rootElement.Q<Label>("DataNameLabel");

            _createNewDataBtn.clicked += CreateNewData;
            
            _animationSequenceDataField.BindProperty(serializedObject.FindProperty("_animationSequenceData"));
            _animationSequenceDataField.RegisterValueChangedCallback(_ =>
            {
                RefreshVisualElement();
            });

            RefreshVisualElement();
            
            return rootElement;
        }

        private void CreateNewData()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Create Animation Sequence Data", "AnimationSequenceData", "asset",
                "");

            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }
            
            var animationSequenceData = CreateInstance<AnimationSequenceData>();
            var animatorController = AnimatorEditorUtility.GetOriginAnimatorController(AnimatorEditorUtility.FindTargetAnimator(_target.gameObject));

            foreach (var layer in animatorController.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    animationSequenceData.SequenceData.TryAddKey(state.state.name);
                }
            }
            
            AssetDatabase.CreateAsset(animationSequenceData, savePath);
            AssetDatabase.SaveAssets();
            
            var loadedAsset = AssetDatabase.LoadAssetAtPath<AnimationSequenceData>(savePath);
            _animationSequenceDataField.value = loadedAsset;
        }

        private void RefreshVisualElement()
        {
            if (_animationSequenceDataField.value == null)
            {
                _dataInspectorContainer.visible = false;
                return;
            }

            _dataInspectorContainer.visible = true;
            
            _dataNameLabel.text = $"{_animationSequenceDataField.value.name} Inspector";

            _dataInspector.Clear();

            var inspector = CreateEditor(_animationSequenceDataField.value);
            _dataInspector.Add(inspector.CreateInspectorGUI());
        }
    }
}
