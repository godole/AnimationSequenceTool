using System.Linq;
using AnimationSequenceTool.Editor.Common;
using AnimationSequenceTool.Editor.VisualElements.ControlPanel;
using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor.Inspectors
{
    [CustomEditor(typeof(AnimationSequenceData))]
    public class AnimationSequenceDataEditor : UnityEditor.Editor
    {
        private AnimationSequenceData _animationSequenceData;
        
        private ListView _sequenceElementListView;

        private DictionaryBindingData<string, SequenceElement> _bindingData;
        
        public override VisualElement CreateInspectorGUI()
        {
            var rootElement = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AnimationSequenceWindowConstants.UxmlPath + "AnimationSequenceDataEditor.uxml");
            visualTree.CloneTree(rootElement);
            
            var sequenceDataInspector = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AnimationSequenceWindowConstants.UxmlPath + "SequenceElementEditor.uxml");

            _animationSequenceData = (AnimationSequenceData)target;
            
            var stateNameDropdownField = rootElement.Q<DropdownField>("AnimationStateDropdownField");
            _sequenceElementListView = rootElement.Q<ListView>("SequenceDataList");
            var addKeyInputField = rootElement.Q<TextField>("AddKeyInputField");
            var addKeyBtn = rootElement.Q<Button>("AddKeyBtn");
            
            var stateNames = _animationSequenceData.SequenceData.Keys.ToList();
            string initialStateName = stateNames.Count == 0 ? "None" : stateNames.First();
            stateNameDropdownField.choices = stateNames;
            stateNameDropdownField.value = initialStateName;
            stateNameDropdownField.RegisterValueChangedCallback((evt) =>
            {
                _bindingData.ChangeKey(evt.newValue);
            });
            
            _bindingData = new DictionaryBindingData<string, SequenceElement>(
                serializedObject,
                _animationSequenceData.SequenceData)
            {
                OnUpdateCollectionItems = () =>
                {
                    _sequenceElementListView.RefreshItems();
                }
            };

            _bindingData.ChangeKey(initialStateName);
            
            _sequenceElementListView.reorderable = true;
            _sequenceElementListView.reorderMode = ListViewReorderMode.Animated;

            _sequenceElementListView.itemsSource = _bindingData.FilteredDataList;
            
            _sequenceElementListView.makeItem = () => sequenceDataInspector.Instantiate();

            _sequenceElementListView.bindItem = (element, i) =>
            {
                var itemProp = _bindingData.GetValueListProperty("SequenceData").GetArrayElementAtIndex(i);
                
                var stateNameField = element.Q<TextField>("StateNameField");
                stateNameField.BindProperty(itemProp.FindPropertyRelative("StateName"));
                
                var eventTimeField = element.Q<Slider>("EventTimeField");
                eventTimeField.BindProperty(itemProp.FindPropertyRelative("EventNormalizedTime"));

                var dataField = element.Q<ObjectField>("DataField");
                dataField.BindProperty(itemProp.FindPropertyRelative("Data"));
                
                var inspectorContainer = element.Q<VisualElement>("InspectorContainer");
                inspectorContainer.Clear();
                
                var dataProp = itemProp.FindPropertyRelative("Data");
                if (dataProp.objectReferenceValue is ScriptableObject so)
                {
                    var editor = CreateEditor(so);
                    if (editor != null)
                    {
                        inspectorContainer.Add(new InspectorElement(editor));
                    }
                }

                dataField.RegisterValueChangedCallback((evt) =>
                {
                    inspectorContainer.Clear();
                    var editor = CreateEditor(evt.newValue);
                    if (editor != null)
                    {
                        inspectorContainer.Add(new InspectorElement(editor));
                    }
                });
            };

            _sequenceElementListView.unbindItem = (element, i) =>
            {
                element.Unbind();
            };
            
            _sequenceElementListView.itemsAdded += indices =>
            {
                var valuesProperty = _bindingData.GetValueListProperty("SequenceData");
                
                foreach (int idx in indices)
                {
                    valuesProperty.InsertArrayElementAtIndex(idx);
                    
                    var newElement = valuesProperty.GetArrayElementAtIndex(idx);
                    
                    var stateNameField = newElement.FindPropertyRelative("StateName");
                    stateNameField.stringValue = stateNameDropdownField.value;

                    var dataField = newElement.FindPropertyRelative("Data");
                    dataField.objectReferenceValue = null;
                    
                    var eventTimeField = newElement.FindPropertyRelative("EventNormalizedTime");
                    eventTimeField.floatValue = 0.0f;
                }
                
                serializedObject.ApplyModifiedProperties();
            };
            
            _sequenceElementListView.itemsRemoved += indices =>
            {
                var valuesProperty = _bindingData.GetValueListProperty("SequenceData");
                
                foreach (int idx in indices.OrderByDescending(i => i))
                {
                    valuesProperty.DeleteArrayElementAtIndex(idx);
                }
                
                serializedObject.ApplyModifiedProperties();
            };

            addKeyBtn.clicked += () =>
            {
                string newKey = addKeyInputField.value;
                
                if (string.IsNullOrEmpty(newKey))
                {
                    return;
                }

                addKeyInputField.value = "";
                
                _bindingData.SerializedDictionary.TryAddKey(newKey);
                _bindingData.UpdateCollectionItems();
                
                stateNameDropdownField.choices = _animationSequenceData.SequenceData.Keys;
                stateNameDropdownField.value = newKey;
            };
            
            _bindingData.RefreshCollectionItemList();
            
            return rootElement;
        }
    }
}
