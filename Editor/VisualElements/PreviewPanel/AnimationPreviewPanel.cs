using AnimationSequenceTool.Editor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AnimationSequenceTool.Editor.VisualElements.PreviewPanel
{
    [UxmlElement]
    public partial class AnimationPreviewPanel : IMGUIContainer, IPreviewPanel
    {
        public AnimationClip PreviewAnimationClip;

        public float NormalizedTime
        {
            get => _currentNormalizedTime;
            set
            {
                _currentNormalizedTime = Mathf.Clamp01(value);
                _currentSamplingTime = ConvertNormalizedTimeToSamplingTime(value);
            }
        }

        public float ConvertNormalizedTimeToSamplingTime(float normalizedTime) => normalizedTime * PreviewAnimationClip.length;

        public void ChangePreviewClip(AnimationClip animationClip) => PreviewAnimationClip = animationClip;

        private Camera _previewCamera;
        private RenderTexture _previewTexture;
        
        private float _currentSamplingTime;
        private float _currentNormalizedTime;

        public void InitializeData(GameObject previewRootObjectInstance, Camera mainSceneCamera)
        {
            onGUIHandler = DrawPreview;
            
            PreviewInstanceManager.PreviewRootObject = previewRootObjectInstance;
            
            var previewCamera = new GameObject("Preview Camera")
            {
                transform =
                {
                    position = mainSceneCamera.transform.position,
                    rotation = mainSceneCamera.transform.rotation,
                    localScale = mainSceneCamera.transform.lossyScale
                }
            };
            
            PreviewInstanceManager.ConvertPreviewObject(previewCamera);
            _previewCamera = previewCamera.AddComponent<Camera>();
            _previewCamera.cullingMask = LayerMask.GetMask(AnimationSequenceWindowConstants.PreviewLayer);
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = Color.gray;
            
            _previewTexture = new RenderTexture(1920, 1080, 1, RenderTextureFormat.ARGB32);
            _previewCamera.targetTexture = _previewTexture;
            
            var previewInstanceRenderers = PreviewInstanceManager.PreviewRootObject.GetComponentsInChildren<Renderer>();
            _previewCamera.transform.position = previewInstanceRenderers[0].bounds.center + new Vector3(0, 0, 3);
            _previewCamera.transform.LookAt(previewInstanceRenderers[0].bounds.center);

            var previewCameraController = new PreviewCameraController(this, _previewCamera)
            {
                MoveSpeed = 0.01f,
                ZoomSpeed = 0.1f,
                RotationSpeed = 0.1f,
            };
        }

        public void AddParticleObject(int instanceId, GameObject previewObject, float startTime)
        {
            PreviewInstanceManager.AddObject(previewObject, startTime);
        }

        public void RemoveParticleObject(int instanceId)
        {
            PreviewInstanceManager.RemoveObject(instanceId);
        }

        public void ChangeNormalizedTime(int instanceId, float normalizedTime)
        {
            PreviewInstanceManager.ChangeSamplingTime(instanceId, normalizedTime);
        }

        public void RenderPreview()
        {
            if (_previewCamera == null) return;
            
            if (!AnimationMode.InAnimationMode())
            {
                AnimationMode.StartAnimationMode();
            }
            
            PreviewInstanceManager.RenderPreview(_currentSamplingTime);
            AnimationMode.SampleAnimationClip(PreviewInstanceManager.PreviewRootObject, PreviewAnimationClip, _currentSamplingTime);
            _previewCamera.Render();
        }

        public void ReleaseObjects()
        {
            if (_previewCamera?.gameObject != null)
            {
                Object.DestroyImmediate(_previewCamera.gameObject);    
            }

            if (_previewTexture != null)
            {
                Object.DestroyImmediate(_previewTexture);    
            }

            PreviewInstanceManager.ReleaseObjects();
        }

        private void DrawPreview()
        {
            if (_previewTexture == null)
            {
                return;
            }
            
            var rect = new Rect(0.0f, 0.0f, resolvedStyle.width, resolvedStyle.height);
            GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleAndCrop, false);
        }
    }
}
