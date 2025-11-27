using System.Collections;
using System.Collections.Generic;
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

        //------------------------------------------------------------------------------------
        private void OnEnable()
        {
            ReloadPresets();
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
            // === 프리셋 UI ===
            DrawPresetGUI();
            GUILayout.Space(LargeSpace);

            // === 기존 인스펙터 ===
            iDialogAnimation.AnimationTarget = (Transform)EditorGUILayout.ObjectField(
                "AnimationTarget",
                iDialogAnimation.AnimationTarget,
                typeof(Transform),
                true);

            DrawInAnimation();
            GUILayout.Space(LargeSpace);
            DrawOutAnimation();

            if (Application.isPlaying == true)
            {
                if (GUILayout.Button("PlayInAnimaion"))
                {
                    iDialogAnimation.PlayInAnimation();
                }

                if (GUILayout.Button("PlayOutAnimaion"))
                {
                    iDialogAnimation.PlayOutAnimation();
                }
            }

            DrawDefaultInspector();
        }

        //------------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------------
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

            // IDialogAnimations 깊은 복사 (JsonUtility 사용)
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

        //------------------------------------------------------------------------------------
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

            // 컴포넌트 -> 프리셋 깊은 복사
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

        //------------------------------------------------------------------------------------
        private void CreateNewPresetFromCurrent()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Dialog Animation Preset",
                "DialogAnimationPreset",
                "asset",
                "새 프리셋 이름을 입력하세요.");

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

            // 방금 만든 프리셋을 선택 상태로
            for (int i = 0; i < _presets.Length; ++i)
            {
                if (_presets[i] == preset)
                {
                    _selectedPresetIndex = i;
                    break;
                }
            }
        }

        //------------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------------
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

        //------------------------------------------------------------------------------------
        private void DrawBaseStructEditor(BaseAnimationStruct animation)
        {
            animation.StartDelay = EditorGUILayout.FloatField("StartDelay", animation.StartDelay);
            animation.Duration = EditorGUILayout.FloatField("Duration", animation.Duration);

            animation.Linear = EditorGUILayout.BeginToggleGroup("Linear", animation.Linear);
            EditorGUILayout.EndToggleGroup();

            if (animation.Linear == false)
                animation.AnimationCurve = EditorGUILayout.CurveField("AnimationCurve", animation.AnimationCurve);
        }
        //------------------------------------------------------------------------------------
    }
}
