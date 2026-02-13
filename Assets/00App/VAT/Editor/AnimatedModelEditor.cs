using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using System.IO;
using DBG.Utils.IO;

namespace AnimatedModel
{
    [CustomEditor(typeof(AnimatedModelData))]
    public class AnimatedModelEditor : Editor
    {
        enum STATE
        {
            Edit,
            Bake,
            BakeReady,
            ToPrefab,
            ToPrefabReady,
            Save
        }

        AnimatedModelData m_target;
        ReorderableList m_clipsList;
        ReorderableList m_renderersdataList;

        List<string> m_buttonLabels = new List<string>();
        List<Action> m_buttonActions = new List<Action>();

        STATE m_state = STATE.Edit;
        string m_message = "";
        int m_messageDuration = 3 * 1000;
        string m_messageMissingSkinnedRenderer = "There must be at leat one GameObject in the hierarchy with a SkinnedMeshRenderer component.";
        string m_messageMissingClip = "There is a missing animation clip.";
        string m_messageDuplicatedClip = "There is a duplicated animation clip.";
        GUIStyle m_messageStyle = new GUIStyle(EditorStyles.label);

        GUIStyle m_headerStyle = new GUIStyle(EditorStyles.label);

        Shader m_shader;
        //MeshPreview m_meshPreview;
        private SkinnedMeshRenderer m_selectedRenderer;
        private AnimatedClipData m_selectedClip;
        Editor m_gameObjectEditor;

        private void Awake()
        {
            m_target = (AnimatedModelData)target;
            InitClipsList();
            SetEditionButtons();

            m_messageStyle.normal.textColor = Color.red;
            m_messageStyle.wordWrap = true;

            m_headerStyle.fontStyle = FontStyle.Bold;

            string[] guis = AssetDatabase.FindAssets("AnimatedModel t:shader");
            if (guis != null)
            {
                string path = AssetDatabase.GUIDToAssetPath(guis[0]);
                m_shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            }
        }

        private void OnDestroy()
        {
            /*
            if(m_meshPreview == null)
            {
                m_meshPreview.Dispose();
            }
            */
        }

        void InitClipsList()
        {
            m_clipsList = new ReorderableList(m_target.clipsData, typeof(AnimationClip), true, true, true, true);
            m_clipsList.drawHeaderCallback = DrawClipsHeader;
            m_clipsList.drawElementCallback = DrawClip;
            m_clipsList.onAddCallback = OnAddClip;
            m_clipsList.elementHeightCallback = ClipListHeight;
            m_clipsList.onRemoveCallback = OnRemoveClip;
            m_clipsList.onReorderCallbackWithDetails = OnReorderClipWithDetails;
            m_clipsList.onSelectCallback = OnSelectClip;

            m_renderersdataList = new ReorderableList(m_target.renderers, typeof(SkinnedMeshData), false, true, false, false);
            m_renderersdataList.drawHeaderCallback = DrawRenderersdataHeader;
            m_renderersdataList.drawElementCallback = DrawRendererdata;
            m_renderersdataList.onSelectCallback = OnSelectRenderer;
        }

        public override void OnInspectorGUI()
        {
            if (m_state == STATE.BakeReady)
            {
                BakeEnd();
                DrawEdition();
            }

            switch (m_state)
            {
                case STATE.Edit:
                    DrawEdition();
                    break;
                case STATE.Bake:
                    DrawBake();
                    break;
                case STATE.ToPrefab:
                    DrawBake();
                    break;
            }
        }

        /*
        public override void OnPreviewSettings()
        {
            if (m_meshPreview != null)
            {
                m_meshPreview.OnPreviewSettings();
            }
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (m_meshPreview != null)
            {
                m_meshPreview.OnPreviewGUI(r, background);
            }
        }
        */


        void DrawEdition()
        {
            GUILayout.BeginVertical();
            GameObject newGobj = (GameObject)EditorGUILayout.ObjectField("GameObject", m_target.gameObject, typeof(GameObject), false);
            if (newGobj != null)
            {
                if (newGobj != m_target.gameObject)
                {
                    if (newGobj.GetComponent<SkinnedMeshRenderer>() != null || newGobj.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                    {
                        m_target.gameObject = newGobj;
                        Restore();
                    }
                    else
                    {
                        WaitForMesssageEnd(m_messageDuration, m_messageMissingSkinnedRenderer);
                        //PopupWindow.Show(new Rect((position.width - 250) * 0.5f, (position.height - 250) * 0.5f, 250, 250), m_messagePopup);
                    }
                    SetEditionButtons();
                }
            }
            else if (m_target.gameObject != null)
            {
                DeleteMaterials();
                m_target.gameObject = null;
                m_target.clipsData.Clear();
                m_target.renderers.Clear();
                UpdateSerializedObject();
                SetEditionButtons();
            }

            //SetEditionButtons();

            GUILayout.Space(15);
            m_renderersdataList.DoLayoutList();
            m_clipsList.DoLayoutList();
            DrawBoxButtons(m_buttonLabels, m_buttonActions);

            if (m_message != "")
            {
                DrawMessageBox(m_message);
            }

            if(m_target.gameObject != null && m_selectedRenderer != null && m_gameObjectEditor != null)
            {
                m_gameObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(400, 400), EditorStyles.whiteLabel);
            }

            GUILayout.EndVertical();
        }

        void DrawBake()
        {
            GUILayout.BeginVertical("GroupBox", GUILayout.Height(120));
            EditorGUILayout.LabelField("Baking", m_headerStyle);
            EditorGUILayout.LabelField(ModelBaker.info);
            GUILayout.EndVertical();
            float buttomSpaceWidth = EditorGUIUtility.currentViewWidth - 37;
            DrawBoxButtons(m_buttonLabels, m_buttonActions);
            EditorGUI.ProgressBar(new Rect(25, 200, buttomSpaceWidth, 25), ModelBaker.Progress, "");
        }

        void DrawClipsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Clips", m_headerStyle);
        }

        void DrawClip(Rect rect, int index, bool isActive, bool isFocused)
        {
            float fpsWidth = 60;
            AnimatedClipData clipData = m_target.clipsData[index];
            AnimationClip newClip = (AnimationClip)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - fpsWidth, 20), clipData.clip, typeof(AnimationClip), false);
            if (newClip != clipData.clip)
            {
                clipData.clip = newClip;
                UpdateSerializedObject();
            }

            float lebelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = fpsWidth - 30;
            int newFps = EditorGUI.IntField(new Rect(rect.x + rect.width - fpsWidth, rect.y, fpsWidth, 20), "FPS", clipData.fps);
            EditorGUIUtility.labelWidth = lebelWidth;
            if (newFps != clipData.fps)
            {
                clipData.fps = newFps;
            }
        }

        float ClipListHeight(int index)
        {
            return 22;
        }

        void OnAddClip(ReorderableList list)
        {
            m_target.clipsData.Add(null);
            UpdateSerializedObject();
        }

        void OnRemoveClip(ReorderableList list)
        {
            m_target.clipsData.RemoveAt(list.index);
            UpdateSerializedObject();
        }

        void OnReorderClipWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            UpdateSerializedObject();
        }
        void OnSelectClip(ReorderableList list)
        {
            if(m_target.clipsData != null)
                m_selectedClip = m_target.clipsData[list.index];
        }

        void DrawRenderersdataHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Renderers Materials", m_headerStyle);
        }
        
        void DrawRendererdata(Rect rect, int index, bool isActive, bool isFocused)
        {
            SkinnedMeshData rendererData = m_target.renderers[index];
            Material newMaterial = (Material)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, 20), rendererData.renderer.name+" material", rendererData.material, typeof(Material), false);
            if (newMaterial != rendererData.material)
            {
                rendererData.material = newMaterial;
                UpdateSerializedObject();
            }
        }
        void OnSelectRenderer(ReorderableList list)
        {
            if (m_target.renderers != null)
            {
                m_selectedRenderer = m_target.renderers[list.index].renderer;
                UpdateMeshPreview();
            }
        }

        void UpdateMeshPreview()
        {
            /*
            if (m_meshPreview != null)
                m_meshPreview.Dispose();
            Debug.Log("UpdateMeshPreview");
            m_meshPreview = new MeshPreview(m_selectedRenderer.sharedMesh);
            */

            if (m_target.gameObject)
            {
                if (m_selectedRenderer)
                    m_gameObjectEditor = Editor.CreateEditor(m_selectedRenderer.gameObject);
            }
        }

        void BakeCancel()
        {
            ModelBaker.Cancel();
        }

        bool ExistNullClip()
        {
            foreach (var clip in m_target.clipsData)
            {
                if (clip == null)
                    return true;
            }
            return false;
        }

        Vector2Int ExistDuplicatedClip()
        {
            for (int i = 0; i < m_target.clipsData.Count; i++)
            {
                for (int j = i + 1; j < m_target.clipsData.Count; j++)
                {
                    if (m_target.clipsData[i] == m_target.clipsData[j])
                        return new Vector2Int(i, j);
                }
            }
            return new Vector2Int(-1, -1);
        }

        void Bake()
        {
            m_target.baked = false;
            _Bake();
        }

        async void _Bake()
        {

            Debug.Log("_Bake");
            if (ExistNullClip())
            {
                Debug.Log(1);
                WaitForMesssageEnd(m_messageDuration, m_messageMissingClip);
                return;
            }

            Vector2Int duplication = ExistDuplicatedClip();
            if (duplication.x != -1)
            {
                Debug.Log(2);
                WaitForMesssageEnd(m_messageDuration, m_messageDuplicatedClip + "\nClips: " + duplication.x + " and " + duplication.y + " are the same clip.");
                return;
            }

            SetBakeButtons();

            if (m_state == STATE.Edit)
            {
                m_state = STATE.Bake;
            }

            ModelBaker.ModelBakeData bakeData = await ModelBaker.Bake(m_target.gameObject, m_target.clipsData, Repaint);
            m_target.animatedMeshes = bakeData.animatedMeshes;
            m_target.bonesCount = bakeData.bonesCount;
            m_target.clipsFramesStart = bakeData.clipsFramesStart;
            m_target.boneKey = bakeData.boneKey;



            m_target.baked = true;
            if (m_state == STATE.Bake)
            {
                m_state = STATE.BakeReady;
            }
        }

        void BakeEnd()
        {
            if (!ModelBaker.cancel)
            {
                UpdateSerializedObject();
            }
            SetEditionButtons();
            m_state = STATE.Edit;
        }

        void UpdateSerializedObject()
        {
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(m_target);
        }

        void SetEditionButtons()
        {
            m_buttonLabels.Clear();
            m_buttonActions.Clear();
            if (m_target.gameObject != null)
            {
                m_buttonLabels.Add("Bake");
                m_buttonLabels.Add("Restore");
                m_buttonLabels.Add("To Asset");
                m_buttonActions.Add(Bake);
                m_buttonActions.Add(Restore);
                m_buttonActions.Add(ToAsset);
            }
        }

        void SetBakeButtons()
        {
            m_buttonLabels.Clear();
            m_buttonActions.Clear();
            m_buttonLabels.Add("Cancel");
            m_buttonActions.Add(BakeCancel);
        }

        void DrawBoxButtons(List<string> buttonLabels, List<Action> callbacks)
        {
            float minButtonSize = 150;
            int buttonsCount = buttonLabels.Count;

            float buttomSpaceWidth = EditorGUIUtility.currentViewWidth - 70;
            float buttonWidth = Mathf.Max(minButtonSize, buttomSpaceWidth / buttonsCount);
            int cols = Mathf.CeilToInt(buttomSpaceWidth / buttonWidth);
            float rows = Mathf.CeilToInt(((float)buttonsCount) / cols);
            //Debug.Log(cols + "  " + rows);

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            for (int row = 0; row < rows; row++)
            {
                GUILayout.BeginHorizontal("GroupBox", GUILayout.Height(50));
                GUILayout.FlexibleSpace();
                for (int col = 0; col < cols; col++)
                {
                    int i = row * cols + col;
                    if (i < buttonLabels.Count && GUILayout.Button(buttonLabels[i], GUILayout.Width(buttonWidth), GUILayout.Height(30)))
                    {
                        callbacks[i]();
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        void DrawMessageBox(string message)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal("GroupBox", GUILayout.Height(50));
            GUILayout.FlexibleSpace();
            GUILayout.Label(message, m_messageStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        void Restore()
        {
            DeleteMaterials();
            m_target.clipsData.Clear();
            List<AnimationClip> clips = UtilsGameObject.GetAnimationClips<AnimationClip>(m_target.gameObject);
            m_target.clipsData = new List<AnimatedClipData>();
            for (int i = 0; i < clips.Count; i++)
            {
                m_target.clipsData.Add(new AnimatedClipData { clip = clips[i], fps = 30 });
            }

            m_target.renderers.Clear();
            SkinnedMeshRenderer renderer = m_target.gameObject.GetComponent<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                m_target.renderers.Add(new SkinnedMeshData
                {
                    renderer = renderer,
                    material = GetMaterial(renderer)
                });
            }

            SkinnedMeshRenderer[] renderers = m_target.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = GetMaterial(renderers[i]);
                m_target.renderers.Add(new SkinnedMeshData
                {
                    renderer = renderers[i],
                    material = material
                });


                AssetDatabase.AddObjectToAsset(material, m_target);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(material));
            }

            UpdateSerializedObject();
            InitClipsList();
        }

        void DeleteMaterials()
        {
            for (int i = 0; i < m_target.renderers.Count; i++)
            {
                Material material = m_target.renderers[i].material;
                if (material)
                {
                    DestroyImmediate(material, true);
                    material = null;
                }
            }
        }

        async void WaitForMesssageEnd(int milliseconds, string message)
        {
            bool end = await _WaitForMesssageEnd(milliseconds, message);
        }

        async Task<bool> _WaitForMesssageEnd(int milliseconds, string message)
        {
            m_message = message;
            await Task.Delay(milliseconds);
            m_message = "";
            return true;
        }

        Material GetMaterial(SkinnedMeshRenderer renderer)
        {
            Material newMaterial = new Material(m_shader);
            newMaterial.name = "Mat" + renderer.name;
            Material oldMaterial = renderer.sharedMaterial;
            if (m_shader != null && oldMaterial != null)
            {
                if(oldMaterial.HasProperty("_Color"))
                {
                    newMaterial.SetColor("_Color", oldMaterial.GetColor("_Color"));
                }
                else if(oldMaterial.HasProperty("Color"))
                {
                    newMaterial.SetColor("Color", oldMaterial.GetColor("Color"));
                }

                if (oldMaterial.HasProperty("_BaseMap"))
                {
                    newMaterial.SetTexture("_Albedo", oldMaterial.GetTexture("_BaseMap"));
                }

                if (oldMaterial.HasProperty("_NormalMap"))
                {
                    newMaterial.SetTexture("_NormalMap", oldMaterial.GetTexture("_NormalMap"));
                }
                return newMaterial;
            }
            return newMaterial;
        }

        void ToAsset()
        {
            m_state = STATE.ToPrefab;
            if (!m_target.baked)
            {
                _Bake();
            }

            string path = EditorUtility.SaveFolderPanel("Save Animated Model",
                Application.dataPath,
                m_target.gameObject.name);

            if (path == "")
            {
                m_state = STATE.BakeReady;
                return;
            }

            int i = Application.dataPath.Length - "Assets".Length;
            path = path.Substring(i) + "/";

//            try
            {
                ModelAnimatorController animator = GetModelAnimatorController(m_target);
                string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + m_target.gameObject.name + ".asset");
                AssetDatabase.CreateAsset(animator, assetPathAndName);

                GameObject gameObject = m_target.GetGameObject();

                assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + m_target.gameObject.name + ".prefab");
                bool success = PrefabUtility.SaveAsPrefabAsset(gameObject, assetPathAndName, out success);
                
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPathAndName);
                animator.prefab = prefab;

                MeshRenderer[] renderersOld = gameObject.GetComponentsInChildren<MeshRenderer>();
                MeshFilter[] meshfiltersOld = gameObject.GetComponentsInChildren<MeshFilter>();


                MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
                MeshFilter[] meshfilters = prefab.GetComponentsInChildren<MeshFilter>();
                ModelAnimator modelAnimator = prefab.AddComponent<ModelAnimator>();

                modelAnimator.m_controller = animator;

                for (int j = 0; j < renderers.Length; j++)
                {
                    //Material material = CloneMaterial(renderers[j].sharedMaterial);
                    Material material = new Material(m_target.renderers[j].material);
                    assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + renderers[j].name + ".mat");
                    AssetDatabase.CreateAsset(material, assetPathAndName);

                    assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + renderers[j].name + ".asset");
                    AssetDatabase.CreateAsset(meshfiltersOld[j].sharedMesh, assetPathAndName);
                    meshfilters[j].sharedMesh = meshfiltersOld[j].sharedMesh;

                    renderers[j].material = material;
                    animator.animationData[j].renderer = renderers[j];
                    animator.animationData[j].rendererMaterials = material;
                }

                DestroyImmediate(gameObject);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
 //           catch
            {
                
            }

            m_state = STATE.BakeReady;
        }

        ModelAnimatorController GetModelAnimatorController(AnimatedModelData data)
        {
            MeshAnimationData[] animatedMeshes = data.animatedMeshes;
            ModelAnimatorController.RuntimeMeshAnimationData[] animationData = new ModelAnimatorController.RuntimeMeshAnimationData[animatedMeshes.Length];

            for (int i=0;i< animatedMeshes.Length;i++)
            {
                MeshAnimationData animatedMesh = animatedMeshes[i];
                animationData[i] = new ModelAnimatorController.RuntimeMeshAnimationData
                {
                    bonesWeightsPerVertexStart = new int[animatedMesh.bonesWeightsPerVertexStart.Length],
                    bonesWeightsPerVertex = new BoneWeight1[animatedMesh.bonesWeightsPerVertex.Length],
                    editorMaterials = m_target.renderers[i].material
                };
                Array.Copy(animatedMesh.bonesWeightsPerVertexStart, animationData[i].bonesWeightsPerVertexStart, animationData[i].bonesWeightsPerVertexStart.Length);
                Array.Copy(animatedMesh.bonesWeightsPerVertex, animationData[i].bonesWeightsPerVertex, animationData[i].bonesWeightsPerVertex.Length);
            }

            ModelAnimatorController.ClipData[] clipsData = new ModelAnimatorController.ClipData[data.clipsData.Count];
            for(int i=0;i<data.clipsData.Count;i++)
            {
                AnimatedClipData clip = data.clipsData[i];
                clipsData[i] = new ModelAnimatorController.ClipData
                {
                    length = clip.clip.length,
                    fps = clip.fps
                };
            }

            ModelAnimatorController animator = CreateInstance<ModelAnimatorController>();

            animator.bonesCount = data.bonesCount;
            animator.clipsFramesStart = new int[data.clipsFramesStart.Length];
            animator.boneKey = new Matrix4x4[data.boneKey.Length];
            Array.Copy(data.clipsFramesStart, animator.clipsFramesStart, animator.clipsFramesStart.Length);
            Array.Copy(data.boneKey, animator.boneKey, animator.boneKey.Length);

            animator.animationData = animationData;
            animator.clipsData = clipsData;

            return animator;
        }
    }
}
