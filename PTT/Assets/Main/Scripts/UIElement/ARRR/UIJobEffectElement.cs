using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Gpm.Ui;
using Spine;
using Spine.Unity;

namespace GameBerry.UI
{
    public class UIJobEffectElement : MonoBehaviour
    {
        [Header("-------JobName-------")]
        [SerializeField]
        private TMP_Text _jobName;

        [SerializeField]
        private UIARRRSkillDescGroup _jobOriginEffect;

        [Header("-------JobEffect-------")]
        [SerializeField]
        private Image _gearEffectIcon;

        [SerializeField]
        private TMP_Text _gearTitleText;

        [SerializeField]
        private TMP_Text _gearValueText;

        [SerializeField]
        private Image _runeIcon;

        [SerializeField]
        private TMP_Text _runeTitleText;

        [SerializeField]
        private TMP_Text _runeValueText;

        [Header("-------Barbar-------")]
        [SerializeField]
        private SkeletonGraphic _skeletonGraphic;

        Skin myEquipsSkin = new Skin("my new skin");

        [SerializeField]
        private List<ContentSizeFitter> customRefreshSizeFilter = new List<ContentSizeFitter>();

        [SerializeField]
        private List<RectTransform> customRefresh;


        public void SetJobEffectElement(Enum_SynergyType Enum_SynergyType, ObscuredInt tier)
        {
            SpineModelData _currentSpineModelData = StaticResource.Instance.GetARRRSpineModelData();

            if (_skeletonGraphic != null)
            {
                _skeletonGraphic.skeletonDataAsset = _currentSpineModelData.SkeletonData;
                _skeletonGraphic.initialSkinName = _currentSpineModelData.SkinList[0];
                _skeletonGraphic.Initialize(true);

                Skeleton skeleton = _skeletonGraphic.Skeleton;
                SkeletonData skeletonData = skeleton.Data;

                // 초기 스킨 세팅
                skeleton.SetSkin(_currentSpineModelData.SkinList[0]);

                myEquipsSkin.Clear();

                foreach (var skinname in _currentSpineModelData.DefaultAttackSkin)
                {
                    if (!string.IsNullOrEmpty(skinname))
                        myEquipsSkin.AddSkin(skeletonData.FindSkin(skinname));
                }

                myEquipsSkin.AddSkin(skeletonData.FindSkin(Managers.JobManager.Instance.GetWeaponSkinName(Enum_SynergyType, tier)));

                skeleton.SetSkin(myEquipsSkin);
                skeleton.SetSlotsToSetupPose(); // 포즈 적용

                _skeletonGraphic.AnimationState.ClearTracks(); // 스킨 적용 후 초기화
                _skeletonGraphic.AnimationState.SetAnimation(0, "Idle", true); // Idle 적용
            }

            JobData jobData = Managers.JobManager.Instance.GetJobData(Enum_SynergyType, tier);

            if (jobData == null)
                return;

            if (_jobName != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_jobName, jobData.NameKey);

            if (_jobOriginEffect != null)
                _jobOriginEffect.SetSkillData(jobData.SynergySkillData);

            if (_gearTitleText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_gearTitleText, jobData.JobGearEffectKey);

            if (_gearValueText != null)
                _gearValueText.SetText("+{0}%", jobData.GearEffect.GetDecrypted().ToInt());



            if (_runeTitleText != null)
                Managers.LocalStringManager.Instance.SetLocalizeText(_runeTitleText, jobData.JobRuneEffectKey);

            if (_runeValueText != null)
                _runeValueText.SetText("+{0}%", jobData.RuneEffect.GetDecrypted());

            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(Enum_SynergyType);
            if (gambleCardSprite != null)
            {
                if (_gearEffectIcon != null)
                    _gearEffectIcon.sprite = gambleCardSprite.JobGearIcon;

                if (_runeIcon != null)
                    _runeIcon.sprite = gambleCardSprite.JobRuneIcon;
            }

            RefreshSizeFilter().Forget();
        }
        //------------------------------------------------------------------------------------
        private async UniTask RefreshSizeFilter()
        {
            await UniTask.NextFrame();
            await UniTask.NextFrame();
            await UniTask.NextFrame();
            for (int i = 0; i < customRefreshSizeFilter.Count; ++i)
            {
                customRefreshSizeFilter[i].SetLayoutVertical();
            }

            if (customRefresh != null)
            {
                for (int i = 0; i < customRefresh.Count; ++i)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(customRefresh[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
    }
}