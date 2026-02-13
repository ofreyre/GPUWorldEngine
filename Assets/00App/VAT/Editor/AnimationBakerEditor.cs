using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Threading;
using System.Threading.Tasks;

[CustomEditor(typeof(AnimatedMeshData))]
public class AnimationBakerEditor : Editor
{
    enum STATE
    {
        Edit,
        Bake,
        BakeReady
    }

    AnimatedMeshData m_target;
    PopupWindowContent m_messagePopup;
    ReorderableList m_clipsList;
    
    List<string> m_buttonLabels = new List<string>();
    List<Action> m_buttonActions = new List<Action>();

    STATE m_state = STATE.Edit;
    float m_bakeTotal;
    float m_bakeProgress;
    string m_bakeInfo;
    bool m_bakeCancel;
    string m_message = "";
    int m_messageDuration = 3 * 1000;
    string m_messageMissingSkinnedRenderer = "There must be at leat one GameObject in the hierarchy with a SkinnedMeshRenderer component.";
    string m_messageMissingClip = "There is a missing animation clip.";
    string m_messageDuplicatedClip = "There is a duplicated animation clip.";
    GUIStyle m_messageStyle = new GUIStyle(EditorStyles.label);

    GUIStyle m_headerStyle = new GUIStyle(EditorStyles.label);

    private void Awake()
    {
        m_target = (AnimatedMeshData)target;
        InitClipsList();
        m_messagePopup = new PopupMessage(new Vector2(250, 250), "The assigned object don't have a SkinnedMeshRenderer component");
        SetEditionButtons();

        m_messageStyle.normal.textColor = Color.red;
        m_messageStyle.wordWrap = true;

        m_headerStyle.fontStyle = FontStyle.Bold;
    }

    void InitClipsList()
    {
        m_clipsList = new ReorderableList(m_target.clips, typeof(AnimationClip), true, true, true, true);
        m_clipsList.drawHeaderCallback = DrawClipsHeader;
        m_clipsList.drawElementCallback = DrawClip;
        m_clipsList.onAddCallback = OnAddClip;
        m_clipsList.onRemoveCallback = OnRemoveClip;
        m_clipsList.onReorderCallbackWithDetails = OnReorderClipWithDetails;
    }

    public override void OnInspectorGUI()
    {
        if(m_state == STATE.BakeReady)
        {
            BakeEnd();
            DrawEdition();
        }

        switch(m_state)
        {
            case STATE.Edit:
                DrawEdition();
                break;
            case STATE.Bake:
                DrawBake();
                break;
        }
    }


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
                    m_target.clips.Clear();
                    m_target.clips = UtilsGameObject.GetAnimationClips<AnimationClip>(m_target.gameObject);
                    UpdateSerializedObject();
                    InitClipsList();
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
            m_target.gameObject = null;
            m_target.clips.Clear();
            UpdateSerializedObject();
            SetEditionButtons();
        }

        //SetEditionButtons();

        GUILayout.Space(15);
        m_clipsList.DoLayoutList();
        DrawBoxButtons(m_buttonLabels, m_buttonActions);

        if(m_message != "")
        {
            DrawMessageBox(m_message);
        }

        GUILayout.EndVertical();
    }

    void DrawBake()
    {
        GUILayout.BeginVertical("GroupBox", GUILayout.Height(120));
        EditorGUILayout.LabelField("Baking", m_headerStyle);
        EditorGUILayout.LabelField(m_bakeInfo);
        GUILayout.EndVertical();
        float buttomSpaceWidth = EditorGUIUtility.currentViewWidth - 37;
        DrawBoxButtons(m_buttonLabels, m_buttonActions);
        EditorGUI.ProgressBar(new Rect(25, 200, buttomSpaceWidth, 25), m_bakeProgress / m_bakeTotal, "");
    }

    void DrawClipsHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Clips", m_headerStyle);
    }

    void DrawClip(Rect rect, int index, bool isActive, bool isFocused)
    {
        AnimationClip clip = m_target.clips[index];        
        AnimationClip newClip = (AnimationClip)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, 20), clip, typeof(AnimationClip), false);
        if (newClip != clip)
        {
            m_target.clips[index] = newClip;
            UpdateSerializedObject();
        }
    }

    void OnAddClip(ReorderableList list)
    {
        m_target.clips.Add(null);
        UpdateSerializedObject();
    }

    void OnRemoveClip(ReorderableList list)
    {
        m_target.clips.RemoveAt(list.index);
        UpdateSerializedObject();
    }

    void OnReorderClipWithDetails(ReorderableList list, int oldIndex, int newIndex)
    {
        UpdateSerializedObject();
    }

    int GetTotalProgress()
    {
        List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        SkinnedMeshRenderer renderer = m_target.gameObject.GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            renderers.Add(renderer);
        }
        renderers.AddRange(m_target.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>());
        int bakeProgress = 0;

        for (int k = 0; k < renderers.Count; k++)
        {
            for (int i = 0; i < m_target.clips.Count; i++)
            {
                bakeProgress += AnimationUtility.GetCurveBindings(m_target.clips[i]).Length;
            }
        }

        return bakeProgress;
    }

    void BakeCancel()
    {
        m_bakeCancel = true;
    }

    bool ExistNullClip()
    {
        foreach(var clip in  m_target.clips)
        {
            if (clip == null)
                return true;
        }
        return false;
    }

    Vector2Int ExistDuplicatedClip()
    {
        for(int i=0;i< m_target.clips.Count;i++)
        {
            for (int j = i + 1; j < m_target.clips.Count; j++)
            {
                if (m_target.clips[i] == m_target.clips[j])
                    return new Vector2Int(i,j);
            }
        }
        return new Vector2Int(-1, -1);
    }

    async void Bake()
    {

        
        if(ExistNullClip())
        {
            WaitForMesssageEnd(m_messageDuration, m_messageMissingClip);
            return;
        }

        Vector2Int duplication = ExistDuplicatedClip();
        if (duplication.x != -1)
        {
            WaitForMesssageEnd(m_messageDuration, m_messageDuplicatedClip+"\nClips: "+ duplication.x +" and " + duplication.y + " are the same clip.");
            return;
        }

        m_bakeTotal = GetTotalProgress();
        m_bakeProgress = 0;

        m_bakeCancel = false;
        SetBakeButtons();

        m_state = STATE.Bake;
        m_target.animatedMeshes = await _Bake();
        m_state = STATE.BakeReady;
    }

    async Task<AnimatedMeshData.MeshAnimationData[]> _Bake()
    {
        List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();

        SkinnedMeshRenderer renderer = m_target.gameObject.GetComponent<SkinnedMeshRenderer>();
        if (renderer != null)
        {
            renderers.Add(renderer);
        }
        renderers.AddRange(m_target.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>());

        Dictionary<string, int> mapBonePathIndex = new Dictionary<string, int>();
        AnimatedMeshData.MeshAnimationData[] animatedMeshes = new AnimatedMeshData.MeshAnimationData[renderers.Count];

        int clipsCount = m_target.clips.Count;

        for (int k=0; k < renderers.Count; k++)
        {
            renderer = renderers[k];
            Transform[] bones = renderer.bones;
            for(int i=0;i< bones.Length;i++)
            {
                mapBonePathIndex.Add(AnimationUtility.CalculateTransformPath(bones[i], m_target.gameObject.transform), i);
            }
            float[][] mapBonePathKeystime = new float[bones.Length][];
            AnimatedMeshData.KeyCurve[][] mapBonePathTransform = new AnimatedMeshData.KeyCurve[bones.Length][];

            int bonesCount = bones.Length;
            int[] boneKeysStart = new int[clipsCount * bonesCount];
            List<float> boneKeysTimes = new List<float>();
            List<AnimatedMeshData.KeyCurve> boneKeycurves = new List<AnimatedMeshData.KeyCurve>();

            for (int i = 0; i < m_target.clips.Count; i++)
            {
                var clip = m_target.clips[i];

                var bindings = AnimationUtility.GetCurveBindings(clip);
                for (int j = 0; j < bindings.Length; j++)
                {
                    var binding = bindings[j];
                    //ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);

                    m_bakeInfo = "Mesh: " + renderer.name + ". Clip: " + clip.name + ". Bone: "+ binding.path;

                    int boneIndex = -1;
                    if (mapBonePathIndex.TryGetValue(binding.path, out boneIndex))
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        int n = curve.keys.Length;

                        if (mapBonePathKeystime[boneIndex] == null)
                        {
                            mapBonePathKeystime[boneIndex] = new float[n];
                            mapBonePathTransform[boneIndex] = new AnimatedMeshData.KeyCurve[n];
                        }
                        else if(n > mapBonePathKeystime[boneIndex].Length)
                        {
                            Array.Resize(ref mapBonePathKeystime[boneIndex], n);
                            Array.Resize(ref mapBonePathTransform[boneIndex], n);
                        }

                        for (int s = 0; s < n; s++)
                        {
                            Keyframe key = curve.keys[s];
                            mapBonePathKeystime[boneIndex][s] = key.time;
                            mapBonePathTransform[boneIndex][s].Set(binding.propertyName, key);
                        }
                    }

                    await Task.Yield();
                    m_bakeProgress++;
                    Repaint();
                    if (m_bakeCancel)
                        return animatedMeshes;

                }

                for(int boneIndex = 0; boneIndex < bonesCount; boneIndex++)
                {
                    if (mapBonePathKeystime[boneIndex] != null)
                    {
                        boneKeysStart[i * bonesCount + boneIndex] = boneKeysTimes.Count;
                        boneKeysTimes.AddRange(mapBonePathKeystime[boneIndex]);
                        boneKeycurves.AddRange(mapBonePathTransform[boneIndex]);
                    }
                    else
                    {
                        boneKeysStart[i * bonesCount + boneIndex] = -1;
                    }
                }
            }

            var bonesPerVertex = renderer.sharedMesh.GetBonesPerVertex();
            var boneWeights = renderer.sharedMesh.GetAllBoneWeights();
            int[] vertexBonesStart = new int[bonesPerVertex.Length];
            int start = 0;
            for (int i=0;i< bonesPerVertex.Length;i++)
            {
                vertexBonesStart[i] = start;
                start += bonesPerVertex[i];
            }

            animatedMeshes[k] = new AnimatedMeshData.MeshAnimationData
            {
                vertices = renderer.sharedMesh.vertices,
                normals = renderer.sharedMesh.normals,
                tangents = renderer.sharedMesh.tangents,
                uv = renderer.sharedMesh.uv,
                triangles = renderer.sharedMesh.triangles,

                bindPoses = renderer.sharedMesh.bindposes,
                bonesWeightsPerVertexStart = vertexBonesStart,
                bonesWeightsPerVertex = boneWeights.ToArray(),

                boneKeysSize = clipsCount * bonesCount,
                boneKeysStart = boneKeysStart,
                boneKeysTimes = boneKeysTimes.ToArray(),
                boneKeycurves = boneKeycurves.ToArray()
            };
        }

        return animatedMeshes;
    }

    void BakeEnd()
    {
        if (!m_bakeCancel)
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
            m_buttonLabels.Add("Debug");
            m_buttonActions.Add(Bake);
            m_buttonActions.Add(DebugBake);
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

        for(int row = 0; row < rows; row++)
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

    void DebugBake()
    {
        if(m_target.animatedMeshes != null)
        {
            Debug.Log(m_target.GetAnimatedMeshesInfo());
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
}
