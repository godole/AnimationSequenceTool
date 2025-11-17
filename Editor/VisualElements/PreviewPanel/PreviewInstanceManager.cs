using System.Collections.Generic;
using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEngine;

namespace AnimationSequenceTool.Editor.VisualElements.PreviewPanel
{
    public static class PreviewInstanceManager
    {
        public class SimulateData
        {
            public float SamplingTime;
            public BasePreviewObject BasePreviewObjectInstance;
        }
        
        public static Dictionary<int, SimulateData> Objects { get; } = new();

        public static GameObject PreviewRootObject
        {
            get => _previewRootObject;
            set
            {
                if (_previewRootObject != null)
                {
                    Object.DestroyImmediate(_previewRootObject);
                }
                
                _previewRootObject = value;
                ConvertPreviewObject(_previewRootObject);
            }
        }
        
        private static GameObject _previewRootObject;

        static PreviewInstanceManager()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ReleaseObjects;
        }
        
        public static void ConvertPreviewObject(GameObject instance)
        {
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.layer = LayerMask.NameToLayer("Preview");
            var sampleObjectRenderers = instance.GetComponentsInChildren<Renderer>();
            foreach (var sampleObjectRenderer in sampleObjectRenderers)
            {
                sampleObjectRenderer.gameObject.layer = LayerMask.NameToLayer("Preview");
            }
            AnimatorUtility.DeoptimizeTransformHierarchy(instance);
        }

        public static void AddObject(GameObject gameObject, float simulateTime)
        {
            var previewObject = gameObject.GetComponent<BasePreviewObject>();

            if (previewObject == null)
            {
                return;
            }
            
            ConvertPreviewObject(gameObject);
            
            Objects.Add(gameObject.GetInstanceID(), new SimulateData()
            {
                SamplingTime = simulateTime,
                BasePreviewObjectInstance = previewObject
            });
        }

        public static void ReleaseObjects()
        {
            foreach (KeyValuePair<int,SimulateData> objectPair in Objects)
            {
                Object.DestroyImmediate(objectPair.Value.BasePreviewObjectInstance.gameObject);
            }
            
            Object.DestroyImmediate(_previewRootObject);
            
            Objects.Clear();
        }

        public static void RemoveObject(int instanceId)
        {
            if (!Objects.Remove(instanceId, out var releaseObject))
            {
                return;
            }

            Object.DestroyImmediate(releaseObject.BasePreviewObjectInstance.gameObject);
        }

        public static void ChangeSamplingTime(int instanceId, float time)
        {
            if (!Objects.TryGetValue(instanceId, out var simulateData))
            {
                return;
            }

            simulateData.SamplingTime = time; 
        }

        public static void RenderPreview(float time)
        {
            foreach (var simulateData in Objects.Values)
            {
                if (simulateData == null)
                {
                    continue;
                }
                
                var simulateObject = simulateData.BasePreviewObjectInstance;

                if (simulateObject == null)
                {
                    continue;
                }
                
                simulateObject.Simulate(time - simulateData.SamplingTime);
            }
        }
    }
}
