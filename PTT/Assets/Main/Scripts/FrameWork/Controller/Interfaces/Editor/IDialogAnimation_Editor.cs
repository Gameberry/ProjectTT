using System;
using UnityEngine;
using UnityEditor;

namespace GameBerry.UI
{
    [CustomEditor(typeof(IDialogAnimation), true)]
    public class IDialogAnimation_Editor : UnityEditor.Editor
    {
        IDialogAnimation iDialogAnimation { get { return (IDialogAnimation)target; } }

        private float SmallSpace = 5.0f;
        private float LargeSpace = 10.0f;
        private int Indentation = 1;

        // --- 프리셋 관련 필드 ----------------------------------------------------
        private DialogAnimationPreset[] _presets;
        private string[] _presetNames;
        private int _selectedPresetIndex = -1;
        // ------------------------------------------------------------------------

        // ========== 에디터 프리뷰 관련 필드 ==========
        private bool _isPreviewPlaying;
        private bool _previewIsIn;             // true: In, false: Out
        private double _previewStartTime;

        private IDialogAnimation _previewTarget;

        private Vector3 _backupPos;
        private Vector3 _backupRot;
        private Vector3 _backupScale;
        private float _backupAlpha;
        private bool _hasBackup;
        // =============================================

        //------------------------------------------------------------------------------------
        private void OnEnable()
        {
            ReloadPresets();
        }

        private void OnDisable()
        {
            StopPreview(true);
        }

        //------------------------------------------------------------------------------------
        void ReloadPresets()
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogAnimationPreset");

            _presets = new DialogAnimationPreset[guids.Length];
            _presetNames = new string[guids.Length];

            for (int i = 0; i < guids.Length; ++i)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                _presets[i] = AssetDatabase.LoadAssetAtPath<DialogAnimationPreset>(path);
                _presetNames[i] = _presets[i] != null ? _presets[i].name : "NULL";
            }

            if (_presets.Length == 0)
                _selectedPresetIndex = -1;
            else if (_selectedPresetIndex < 0 || _selectedPresetIndex >= _presets.Length)
                _selectedPresetIndex = 0;
        }

        //------------------------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // === 프리셋 UI ===
            DrawPresetGUI();
            GUILayout.Space(LargeSpace);

            // ====== 에디터 프리뷰 UI ======
            if (!Application.isPlaying)
            {
                DrawPreviewGUI();
                GUILayout.Space(LargeSpace);
            }
            // =============================

            // === 기존 인스펙터 UI ===
            iDialogAnimation.AnimationTarget = (Transform)EditorGUILayout.ObjectField(
                "AnimationTarget",
                iDialogAnimation.AnimationTarget,
                typeof(Transform),
                true);

            DrawInAnimation();
            GUILayout.Space(LargeSpace);
            DrawOutAnimation();

            // 플레이 모드에서 런타임 테스트 버튼
            if (Application.isPlaying == true)
            {
                if (GUILayout.Button("PlayInAnimation"))
                    iDialogAnimation.PlayInAnimation();

                if (GUILayout.Button("PlayOutAnimation"))
                    iDialogAnimation.PlayOutAnimation();
            }

            // 혹시 숨겨진 필드들 보고 싶으면 유지
            DrawDefaultInspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(iDialogAnimation);
            }

            serializedObject.ApplyModifiedProperties();
        }

        //------------------------------------------------------------------------------------
        #region Preview GUI & Logic

        private void DrawPreviewGUI()
        {
            EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = !_isPreviewPlaying;
                if (GUILayout.Button("Preview In"))
                {
                    StartPreview(true);
                }

                if (GUILayout.Button("Preview Out"))
                {
                    StartPreview(false);
                }

                GUI.enabled = _isPreviewPlaying;
                if (GUILayout.Button("Stop"))
                {
                    StopPreview(true);
                }
                GUI.enabled = true;
            }
        }

        private void StartPreview(bool isIn)
        {
            var rt = (iDialogAnimation.AnimationTarget != null
                ? iDialogAnimation.AnimationTarget.GetComponent<RectTransform>()
                : iDialogAnimation.GetComponent<RectTransform>());

            var cg = iDialogAnimation.GetComponent<CanvasGroup>();

            if (rt == null || cg == null)
            {
                Debug.LogWarning("IDialogAnimation Preview: RectTransform 또는 CanvasGroup이 없습니다.");
                return;
            }

            // 기존 프리뷰 정리
            StopPreview(true);

            _previewTarget = iDialogAnimation;
            _previewIsIn = isIn;
            _previewStartTime = EditorApplication.timeSinceStartup;

            // 백업
            _backupPos = rt.anchoredPosition3D;
            _backupRot = rt.eulerAngles;
            _backupScale = rt.localScale;
            _backupAlpha = cg.alpha;
            _hasBackup = true;

            // 총 길이 계산
            if (isIn)
                _previewTarget.InAnimation.SetTotalDuration();
            else
                _previewTarget.OutAnimation.SetTotalDuration();

            _isPreviewPlaying = true;
            EditorApplication.update += OnEditorPreviewUpdate;
        }

        private void StopPreview(bool restore)
        {
            if (_isPreviewPlaying)
            {
                EditorApplication.update -= OnEditorPreviewUpdate;
                _isPreviewPlaying = false;
            }

            if (restore && _hasBackup && _previewTarget != null)
            {
                var rt = (_previewTarget.AnimationTarget != null
                    ? _previewTarget.AnimationTarget.GetComponent<RectTransform>()
                    : _previewTarget.GetComponent<RectTransform>());
                var cg = _previewTarget.GetComponent<CanvasGroup>();

                if (rt != null)
                {
                    rt.anchoredPosition3D = _backupPos;
                    rt.eulerAngles = _backupRot;
                    rt.localScale = _backupScale;
                }

                if (cg != null)
                    cg.alpha = _backupAlpha;
            }

            _hasBackup = false;
            _previewTarget = null;
        }

        private void OnEditorPreviewUpdate()
        {
            if (!_isPreviewPlaying || _previewTarget == null)
            {
                StopPreview(false);
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float elapsed = (float)(now - _previewStartTime);

            IDialogAnimations anim = _previewIsIn
                ? _previewTarget.InAnimation
                : _previewTarget.OutAnimation;

            if (anim == null)
            {
                StopPreview(true);
                return;
            }

            // 혹시 TotalDuration이 0이면 다시 계산
            if (anim.TotalDuration <= 0f)
            {
                anim.SetTotalDuration();
                if (anim.TotalDuration <= 0f)
                {
                    StopPreview(true);
                    return;
                }
            }

            float total = anim.TotalDuration;
            float t = Mathf.Clamp(elapsed, 0f, total);

            SampleAnimationAtTime(_previewTarget, anim, _previewIsIn, t);

            // 끝났으면 자동 종료 + 복원
            if (elapsed >= total)
            {
                StopPreview(true);
            }

            // 씬/인스펙터 다시 그리기
            SceneView.RepaintAll();
            Repaint();
        }

        private void SampleAnimationAtTime(IDialogAnimation target, IDialogAnimations anims, bool isIn, float time)
        {
            var rt = (target.AnimationTarget != null
                ? target.AnimationTarget.GetComponent<RectTransform>()
                : target.GetComponent<RectTransform>());
            var cg = target.GetComponent<CanvasGroup>();

            if (rt == null || cg == null)
                return;

            // 기준값은 프리뷰 시작 시점의 백업값 사용
            Vector3 basePos = _backupPos;
            Vector3 baseRot = _backupRot;
            Vector3 baseScale = _backupScale;

            // ----- Move -----
            if (anims.MoveAni != null && anims.MoveAni.UseAnimation)
            {
                float r = GetAnimRatio(anims.MoveAni, time);
                if (r > 0f)
                {
                    Vector3 from, to;
                    if (isIn)
                    {
                        from = anims.MoveAni.GetTargetPosition(rt, basePos);
                        to = basePos;
                    }
                    else
                    {
                        from = basePos;
                        to = anims.MoveAni.GetTargetPosition(rt, basePos);
                    }

                    rt.anchoredPosition3D = Vector3.Lerp(from, to, r);
                }
                else
                {
                    rt.anchoredPosition3D = basePos;
                }
            }
            else
            {
                rt.anchoredPosition3D = basePos;
            }

            // ----- Rotate -----
            if (anims.RotateAni != null && anims.RotateAni.UseAnimation)
            {
                float r = GetAnimRatio(anims.RotateAni, time);
                if (r > 0f)
                {
                    Vector3 from, to;
                    if (isIn)
                    {
                        from = anims.RotateAni.Rotate;
                        to = baseRot;
                    }
                    else
                    {
                        from = baseRot;
                        to = anims.RotateAni.Rotate;
                    }

                    rt.eulerAngles = Vector3.Lerp(from, to, r);
                }
                else
                {
                    rt.eulerAngles = baseRot;
                }
            }
            else
            {
                rt.eulerAngles = baseRot;
            }

            // ----- Scale -----
            if (anims.ScaleAni != null && anims.ScaleAni.UseAnimation)
            {
                float r = GetAnimRatio(anims.ScaleAni, time);
                if (r > 0f)
                {
                    Vector3 from, to;
                    if (isIn)
                    {
                        from = anims.ScaleAni.Scale;
                        to = baseScale;
                    }
                    else
                    {
                        from = baseScale;
                        to = anims.ScaleAni.Scale;
                    }

                    rt.localScale = Vector3.Lerp(from, to, r);
                }
                else
                {
                    rt.localScale = baseScale;
                }
            }
            else
            {
                rt.localScale = baseScale;
            }

            // ----- Fade -----
            if (anims.FadeAni != null && anims.FadeAni.UseAnimation)
            {
                float r = GetAnimRatio(anims.FadeAni, time);
                if (r > 0f)
                {
                    float from = anims.FadeAni.StartAlpha;
                    float to = anims.FadeAni.EndAlpha;
                    cg.alpha = Mathf.Lerp(from, to, r);
                }
                else
                {
                    cg.alpha = anims.FadeAni.StartAlpha;
                }
            }
            else
            {
                cg.alpha = _backupAlpha;
            }
        }

        private float GetAnimRatio(BaseAnimationStruct ani, float time)
        {
            if (ani == null || !ani.UseAnimation)
                return 0f;

            float start = ani.StartDelay;
            float end = ani.StartDelay + ani.Duration;

            if (time <= start)
                return 0f;
            if (time >= end)
                return 1f;

            float t = (time - start) / ani.Duration;
            if (!ani.Linear && ani.AnimationCurve != null)
                t = ani.AnimationCurve.Evaluate(t);

            return Mathf.Clamp01(t);
        }

        #endregion

        //------------------------------------------------------------------------------------
        #region Preset GUI & Logic

        private void DrawPresetGUI()
        {
            EditorGUILayout.LabelField("Dialog Animation Presets", EditorStyles.boldLabel);

            // 프리셋이 없을 때
            if (_presets == null || _presets.Length == 0)
            {
                EditorGUILayout.HelpBox("등록된 프리셋이 없습니다.", MessageType.Info);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _selectedPresetIndex = Mathf.Clamp(_selectedPresetIndex, 0, _presets.Length - 1);
                    _selectedPresetIndex = EditorGUILayout.Popup("Preset", _selectedPresetIndex, _presetNames);

                    if (GUILayout.Button("Reload", GUILayout.Width(60)))
                    {
                        ReloadPresets();
                        GUI.FocusControl(null);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Apply Preset"))
                    {
                        ApplyPresetToComponent();
                    }

                    if (GUILayout.Button("Save To Preset"))
                    {
                        SaveComponentToPreset();
                    }
                }
            }

            if (GUILayout.Button("Create New Preset From Current"))
            {
                CreateNewPresetFromCurrent();
            }
        }

        private void ApplyPresetToComponent()
        {
            if (_presets == null || _presets.Length == 0)
                return;

            if (_selectedPresetIndex < 0 || _selectedPresetIndex >= _presets.Length)
                return;

            DialogAnimationPreset preset = _presets[_selectedPresetIndex];
            if (preset == null)
                return;

            Undo.RecordObject(iDialogAnimation, "Apply Dialog Animation Preset");

            iDialogAnimation.useInAnimation = preset.useInAnimation;
            iDialogAnimation.useOutAnimation = preset.useOutAnimation;

            if (preset.InAnimation != null && iDialogAnimation.InAnimation != null)
            {
                string json = JsonUtility.ToJson(preset.InAnimation);
                JsonUtility.FromJsonOverwrite(json, iDialogAnimation.InAnimation);
            }

            if (preset.OutAnimation != null && iDialogAnimation.OutAnimation != null)
            {
                string json = JsonUtility.ToJson(preset.OutAnimation);
                JsonUtility.FromJsonOverwrite(json, iDialogAnimation.OutAnimation);
            }

            EditorUtility.SetDirty(iDialogAnimation);
        }

        private void SaveComponentToPreset()
        {
            if (_presets == null || _presets.Length == 0)
                return;

            if (_selectedPresetIndex < 0 || _selectedPresetIndex >= _presets.Length)
                return;

            DialogAnimationPreset preset = _presets[_selectedPresetIndex];
            if (preset == null)
                return;

            Undo.RecordObject(preset, "Save Dialog Animation Preset");

            preset.useInAnimation = iDialogAnimation.useInAnimation;
            preset.useOutAnimation = iDialogAnimation.useOutAnimation;

            if (preset.InAnimation == null)
                preset.InAnimation = new IDialogAnimations();
            if (preset.OutAnimation == null)
                preset.OutAnimation = new IDialogAnimations();

            if (iDialogAnimation.InAnimation != null)
            {
                string json = JsonUtility.ToJson(iDialogAnimation.InAnimation);
                JsonUtility.FromJsonOverwrite(json, preset.InAnimation);
            }

            if (iDialogAnimation.OutAnimation != null)
            {
                string json = JsonUtility.ToJson(iDialogAnimation.OutAnimation);
                JsonUtility.FromJsonOverwrite(json, preset.OutAnimation);
            }

            EditorUtility.SetDirty(preset);
            AssetDatabase.SaveAssets();
        }

        private void CreateNewPresetFromCurrent()
        {
            const string defaultFolder = "Assets/Main/ProjectResources/DialogAnimationPreset";

            // --- 폴더 없으면 생성 ---
            if (!AssetDatabase.IsValidFolder("Assets/Main"))
                AssetDatabase.CreateFolder("Assets", "Main");

            if (!AssetDatabase.IsValidFolder("Assets/Main/ProjectResources"))
                AssetDatabase.CreateFolder("Assets/Main", "ProjectResources");

            if (!AssetDatabase.IsValidFolder(defaultFolder))
                AssetDatabase.CreateFolder("Assets/Main/ProjectResources", "DialogAnimationPreset");

            // --- 기본 경로 지정해서 저장 패널 띄우기 ---
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Dialog Animation Preset",
                "DialogAnimationPreset",
                "asset",
                "새 프리셋 이름을 입력하세요.",
                defaultFolder           // ✅ 여기!
            );

            if (string.IsNullOrEmpty(path))
                return;

            DialogAnimationPreset preset = ScriptableObject.CreateInstance<DialogAnimationPreset>();

            // 현재 값 복사
            preset.useInAnimation = iDialogAnimation.useInAnimation;
            preset.useOutAnimation = iDialogAnimation.useOutAnimation;

            if (preset.InAnimation == null)
                preset.InAnimation = new IDialogAnimations();
            if (preset.OutAnimation == null)
                preset.OutAnimation = new IDialogAnimations();

            if (iDialogAnimation.InAnimation != null)
            {
                string json = JsonUtility.ToJson(iDialogAnimation.InAnimation);
                JsonUtility.FromJsonOverwrite(json, preset.InAnimation);
            }

            if (iDialogAnimation.OutAnimation != null)
            {
                string json = JsonUtility.ToJson(iDialogAnimation.OutAnimation);
                JsonUtility.FromJsonOverwrite(json, preset.OutAnimation);
            }

            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ReloadPresets();

            // 방금 만든 프리셋 선택
            for (int i = 0; i < _presets.Length; ++i)
            {
                if (_presets[i] == preset)
                {
                    _selectedPresetIndex = i;
                    break;
                }
            }
        }

        #endregion

        //------------------------------------------------------------------------------------
        #region Animation Drawers

        private void DrawInAnimation()
        {
            iDialogAnimation.useInAnimation = EditorGUILayout.BeginToggleGroup("InAnimation", iDialogAnimation.useInAnimation);

            if (iDialogAnimation.useInAnimation == true)
            {
                DrawMoveEditor(iDialogAnimation.InAnimation.MoveAni);
                GUILayout.Space(SmallSpace);
                DrawRotateEditor(iDialogAnimation.InAnimation.RotateAni);
                GUILayout.Space(SmallSpace);
                DrawScaleEditor(iDialogAnimation.InAnimation.ScaleAni);
                GUILayout.Space(SmallSpace);
                DrawFadeEditor(iDialogAnimation.InAnimation.FadeAni);
            }

            EditorGUILayout.EndToggleGroup();
        }

        private void DrawOutAnimation()
        {
            iDialogAnimation.useOutAnimation = EditorGUILayout.BeginToggleGroup("OutAnimation", iDialogAnimation.useOutAnimation);

            if (iDialogAnimation.useOutAnimation == true)
            {
                DrawMoveEditor(iDialogAnimation.OutAnimation.MoveAni);
                GUILayout.Space(SmallSpace);
                DrawRotateEditor(iDialogAnimation.OutAnimation.RotateAni);
                GUILayout.Space(SmallSpace);
                DrawScaleEditor(iDialogAnimation.OutAnimation.ScaleAni);
                GUILayout.Space(SmallSpace);
                DrawFadeEditor(iDialogAnimation.OutAnimation.FadeAni);
            }

            EditorGUILayout.EndToggleGroup();
        }

        private void DrawMoveEditor(MoveAniStruct animation)
        {
            animation.UseAnimation = EditorGUILayout.BeginToggleGroup("MoveAnimation", animation.UseAnimation);

            if (animation.UseAnimation == true)
            {
                EditorGUI.indentLevel += Indentation;

                DrawBaseStructEditor(animation);

                GUILayout.Width(SmallSpace);
                animation.MoveFrom = (MoveAniStruct.MoveDirection)EditorGUILayout.EnumPopup("MoveFrom", animation.MoveFrom);

                if (animation.MoveFrom == MoveAniStruct.MoveDirection.CustomPosition)
                    animation.CustomPosition = EditorGUILayout.Vector3Field("CustomPosition", animation.CustomPosition);

                EditorGUI.indentLevel -= Indentation;
            }

            EditorGUILayout.EndToggleGroup();
        }

        private void DrawRotateEditor(RotateAniStruct animation)
        {
            animation.UseAnimation = EditorGUILayout.BeginToggleGroup("RotateAnimation", animation.UseAnimation);

            if (animation.UseAnimation == true)
            {
                EditorGUI.indentLevel += Indentation;

                DrawBaseStructEditor(animation);

                animation.Rotate = EditorGUILayout.Vector3Field("Rotate", animation.Rotate);

                EditorGUI.indentLevel -= Indentation;
            }

            EditorGUILayout.EndToggleGroup();
        }

        private void DrawScaleEditor(ScaleAniStruct animation)
        {
            animation.UseAnimation = EditorGUILayout.BeginToggleGroup("ScaleAnimation", animation.UseAnimation);

            if (animation.UseAnimation == true)
            {
                EditorGUI.indentLevel += Indentation;

                DrawBaseStructEditor(animation);

                animation.Scale = EditorGUILayout.Vector3Field("Scale", animation.Scale);

                EditorGUI.indentLevel -= Indentation;
            }

            EditorGUILayout.EndToggleGroup();
        }

        private void DrawFadeEditor(FadeAniStruct animation)
        {
            animation.UseAnimation = EditorGUILayout.BeginToggleGroup("FadeAnimation", animation.UseAnimation);

            if (animation.UseAnimation == true)
            {
                EditorGUI.indentLevel += Indentation;

                DrawBaseStructEditor(animation);

                animation.StartAlpha = EditorGUILayout.Slider("StartAlpha", animation.StartAlpha, 0.0f, 1.0f);
                animation.EndAlpha = EditorGUILayout.Slider("EndAlpha", animation.EndAlpha, 0.0f, 1.0f);

                EditorGUI.indentLevel -= Indentation;
            }

            EditorGUILayout.EndToggleGroup();
        }

        private void DrawBaseStructEditor(BaseAnimationStruct animation)
        {
            animation.StartDelay = EditorGUILayout.FloatField("StartDelay", animation.StartDelay);
            animation.Duration = EditorGUILayout.FloatField("Duration", animation.Duration);

            animation.Linear = EditorGUILayout.BeginToggleGroup("Linear", animation.Linear);
            EditorGUILayout.EndToggleGroup();

            if (animation.Linear == false)
                animation.AnimationCurve = EditorGUILayout.CurveField("AnimationCurve", animation.AnimationCurve);
        }

        #endregion
    }
}
