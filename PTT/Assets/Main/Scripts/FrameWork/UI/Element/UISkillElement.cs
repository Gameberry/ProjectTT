using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameBerry.Chart;

namespace GameBerry.UI
{
    /// <summary>
    /// 개별 스킬 정보를 표시하는 UI 요소
    /// - 스킬 목록, 스킬 상세 정보 등에서 사용
    /// </summary>
    public class UISkillElement : MonoBehaviour
    {
        [SerializeField] private Image _skillIcon;
        [SerializeField] private Image _frameImage;
        [SerializeField] private TMP_Text _skillName;
        [SerializeField] private TMP_Text _skillLevel;
        [SerializeField] private TMP_Text _skillType;      // Active/Passive
        
        [SerializeField] private Transform _lockedIndicator;   // 미해금 표시
        [SerializeField] private Transform _equippedIndicator; // 장착 중 표시
        
        [SerializeField] private Button _button;

        private int _skillId = 0;
        private System.Action<int> _onClickCallback;

        //------------------------------------------------------------------------------------
        private void Awake()
        {
            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }
        //------------------------------------------------------------------------------------
        public void ConnectCallBack(System.Action<int> onClickCallback = null)
        {
            _onClickCallback = onClickCallback;
        }
        //------------------------------------------------------------------------------------
        public void Bind(int skillId)
        {
            _skillId = skillId;

            SkillInfo skillInfo = Chart.GameChart.Get<SkillChart>()?.Get(skillId);
            if (skillInfo == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // 스킬 아이콘
            if (_skillIcon != null)
            {
                // TODO: 스킬 아이콘 로드
                _skillIcon.sprite = SkillManager.Instance.GetIcon(skillInfo.SkillId);
            }

            // 스킬 이름
            if (_skillName != null)
            {
                // TODO: 로컬라이제이션
                _skillName.SetText($"Skill {skillId}");
            }

            // 스킬 타입
            if (_skillType != null)
            {
                _skillType.SetText(skillInfo.SkillType.ToString());
            }

            // 보유 여부에 따른 표시
            bool hasSkill = SkillManager.Instance.HasSkill(skillId);

            if (_lockedIndicator != null)
                _lockedIndicator.gameObject.SetActive(!hasSkill);

            if (hasSkill)
            {
                // 스킬 레벨 표시
                int level = SkillManager.Instance.GetSkillLevel(skillId);
                if (_skillLevel != null)
                {
                    _skillLevel.SetText($"Lv.{level}");
                    _skillLevel.gameObject.SetActive(true);
                }

                // 장착 여부 표시 (액티브만)
                if (skillInfo.SkillType == Enum_SkillType.Active)
                {
                    bool isEquipped = SkillManager.Instance.IsSkillEquipped(skillId);
                    if (_equippedIndicator != null)
                        _equippedIndicator.gameObject.SetActive(isEquipped);
                }
                else
                {
                    if (_equippedIndicator != null)
                        _equippedIndicator.gameObject.SetActive(false);
                }
            }
            else
            {
                if (_skillLevel != null)
                    _skillLevel.gameObject.SetActive(false);

                if (_equippedIndicator != null)
                    _equippedIndicator.gameObject.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void Refresh()
        {
            if (_skillId > 0)
                Bind(_skillId);
        }
        //------------------------------------------------------------------------------------
        private void OnClick()
        {
            _onClickCallback?.Invoke(_skillId);
        }
        //------------------------------------------------------------------------------------
    }
}
