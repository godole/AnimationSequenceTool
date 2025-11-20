using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [Serializable]
    public class DictionaryElement
    {
        public List<TValue> Values = new();
    }
    
    [SerializeField] private List<TKey> _keys = new();
    [SerializeField] private List<DictionaryElement> _values = new();
    
    private Dictionary<TKey, DictionaryElement> _data = new();
    
    public List<TKey> Keys => _keys;
    public List<DictionaryElement> Values => _values;

    public Dictionary<TKey, DictionaryElement> Dictionary
    {
        get
        {
            if (_data == null)
            {
                BuildDictionary();
            }
            
            return _data;
        }
    }

    public void Add(TKey key, TValue value)
    {
        if (!_keys.Contains(key))
        {
            _keys.Add(key);
        }
        
        int keyIndex = _keys.IndexOf(key);
        _values[keyIndex].Values.Add(value);
    }

    public bool TryAddKey(TKey key)
    {
        if (_keys.Contains(key))
        {
            return false;
        }
        
        _keys.Add(key);
        _values.Add(new DictionaryElement());

        BuildDictionary();
        
        return true;
    }
    
    public void OnBeforeSerialize()
    {
        
    }

    public void OnAfterDeserialize()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        _data = new Dictionary<TKey, DictionaryElement>();
        
        int indexCount = Math.Min(_keys.Count, _values.Count);

        for (int i = 0; i < indexCount; i++)
        {
            _data.TryAdd(_keys[i], _values[i]);
        }
    }
}
