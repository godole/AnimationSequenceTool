using AnimationSequenceTool.Editor.Common;
using AnimationSequenceTool.Editor.Utilities;
using AnimationSequenceTool.Editor.VisualElements.ControlPanel;
using AnimationSequenceTool.Editor.VisualElements.PreviewPanel;
using AnimationSequenceTool.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimationSequenceTool.Editor
{
    /// <summary>
    /// 리컴파일, 프로젝트를 열었을때 선택된 오브젝트가 날아가는 현상을 없애기 위해 선택된 대상의 GUID를 캐싱하는 역할
    /// </summary>
    [FilePath(AnimationSequenceWindowConstants.CacheAssetPath, FilePathAttribute.Location.PreferencesFolder)]
    public class SelectionCache : ScriptableSingleton<SelectionCache>
    {
        public GUID PrefabGuid;

        public void SavePrefabGuid(GameObject prefab)
        {
            PrefabGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(prefab));
            Save(true);
        }

        public GameObject LoadPrefab()
        {
            return AssetDatabase.LoadAssetByGUID<GameObject>(PrefabGuid);
        }
    }
    
    public class AnimationSequenceWindow : EditorWindow
    {
        //프리뷰 씬 오브젝트
        private GameObject _sampleObject;
        private AnimationPreviewPanel _animationPreviewPanel;
        private AnimationSequenceEvent _animationSequenceEventComponent;
        
        private readonly EditorWindowTimer _editorWindowTimer = new();
        
        private ControlPanel _controlPanel;
        
        [MenuItem(AnimationSequenceWindowConstants.OpenWindowOption, true)]
        private static bool ValidateOpen()
        {
            if (Selection.activeObject is GameObject go)
            {
                return go.GetComponent<AnimationSequenceEvent>() != null;
            }
            return false;
        }

        [MenuItem(AnimationSequenceWindowConstants.OpenWindowOption, false)]
        private static void OpenWindow()
        {
            SelectionCache.instance.SavePrefabGuid(Selection.activeObject as GameObject);
            
            var wnd = GetWindow<AnimationSequenceWindow>();
            wnd.titleContent = new GUIContent(AnimationSequenceWindowConstants.WindowTitle);
        }

        public void CreateGUI()
        {
            if (SelectionCache.instance.PrefabGuid.Empty())
            {
                Close();
                return;
            }
            
            var cachedSelectedPrefab = SelectionCache.instance.LoadPrefab();
            _sampleObject = Instantiate(cachedSelectedPrefab);
            _animationSequenceEventComponent = _sampleObject.GetComponent<AnimationSequenceEvent>();
            InitializeVisualElements();
            InitializeSceneView();
        }

        private void InitializeVisualElements()
        {
            var splitView = new TwoPaneSplitView(0, 800, TwoPaneSplitViewOrientation.Vertical);

            //프리뷰 윈도우
            _animationPreviewPanel = new AnimationPreviewPanel();
            _animationPreviewPanel.InitializeData(_sampleObject, Camera.main);
            splitView.Add(_animationPreviewPanel);

            //데이터 영역 윈도우
            _controlPanel = new ControlPanel();
            _controlPanel.InitializeVisualElements(
                new ControlPanel.BindingData()
                {
                    AnimationSequenceData = _animationSequenceEventComponent.AnimationSequenceData
                },
                new ControlPanel.AnimatorComponentData()
                {
                    AnimatorController = AnimatorEditorUtility.GetOriginAnimatorController(AnimatorEditorUtility.FindTargetAnimator(_sampleObject))
                });
            
            _controlPanel.OnSelectedAnimationChanged = clip =>
            {
                _animationPreviewPanel.ChangePreviewClip(clip);
            };
            _controlPanel.BindDataField = (dataField, serializedProperty) => dataField.BindDataFieldProperty(_animationPreviewPanel, serializedProperty);
            
            _animationPreviewPanel.ChangePreviewClip(_controlPanel.CurrentClip);
            
            splitView.Add(_controlPanel);
            
            rootVisualElement.Add(splitView);
        }

        private void InitializeSceneView()
        {
            //씬 뷰에서 Preview 레이어를 끔
            var sceneView = SceneView.lastActiveSceneView;
            
            // cullingMask == -1이면 모두 켜져있는거
            if (sceneView.camera.cullingMask == -1)
            {
                Tools.visibleLayers = ~LayerMask.GetMask(AnimationSequenceWindowConstants.PreviewLayer);
            }
            else
            {
                int currentSceneViewLayerMask = sceneView.camera.cullingMask;
                currentSceneViewLayerMask &= ~(1 << LayerMask.NameToLayer(AnimationSequenceWindowConstants.PreviewLayer));

                sceneView.camera.cullingMask = currentSceneViewLayerMask;  
            }

            sceneView.Repaint();
        }

        private void OnDestroy()
        {
            _animationPreviewPanel.ReleaseObjects();
        }
        
        private void Update()
        {
            _editorWindowTimer.UpdateTimer();
            _controlPanel.UpdateTime(_editorWindowTimer.DeltaTime);
            _animationPreviewPanel.NormalizedTime = _controlPanel.Timer.Time;
            _animationPreviewPanel?.RenderPreview();
            
            Repaint();
        }
    }
}
