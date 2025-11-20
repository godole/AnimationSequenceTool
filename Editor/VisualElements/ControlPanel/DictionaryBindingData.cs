using System;
using System.Collections.Generic;
using AnimationSequenceTool.Runtime;
using UnityEditor;

namespace AnimationSequenceTool.Editor.VisualElements.ControlPanel
{
    public class DictionaryBindingData<TKey, TValue>
    {
        public SerializedObject DataSerializedObject { get; }
        public List<TValue> FilteredDataList { get; } = new();
        
        public SerializableDictionary<TKey, TValue> SerializedDictionary { get; private set; }

        public Action OnUpdateCollectionItems;
        
        public TKey SelectedKey { get; private set; }
        
        public DictionaryBindingData(SerializedObject origin, SerializableDictionary<TKey, TValue> serializableDictionary)
        {
            SerializedDictionary = serializableDictionary;
            DataSerializedObject = origin;
        }

        /// <summary>
        /// SerializedDictionary가 변경되었을때 변경된 데이터를 반영합니다.
        /// </summary>
        public void UpdateCollectionItems()
        {
            DataSerializedObject.ApplyModifiedProperties();
            DataSerializedObject.Update();
            RefreshCollectionItemList();
        }

        /// <summary>
        /// 현재 Key에 맞게 List를 업데이트 해줍니다
        /// </summary>
        public void RefreshCollectionItemList()
        {
            FilteredDataList.Clear();
            
            if(SelectedKey == null)
            {
                return;
            }

            if (!SerializedDictionary.Dictionary.TryGetValue(SelectedKey, out var value))
            {
                return;
            }
                
            FilteredDataList.AddRange(value.Values);
            
            OnUpdateCollectionItems?.Invoke();
        }
        
        public void ChangeKey(TKey stateName)
        {
            SelectedKey = stateName;
            RefreshCollectionItemList();
        }

        public SerializedProperty GetValueListProperty(string dataName)
        {
            int index = SerializedDictionary.Keys.IndexOf(SelectedKey);
            
            return DataSerializedObject.FindProperty(dataName)
                .FindPropertyRelative("_values")
                .GetArrayElementAtIndex(index)
                .FindPropertyRelative("Values");
        }
    }
}
