using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameBerry.Managers;
using TMPro;
using UnityEngine.EventSystems;


namespace GameBerry.UI
{
    public class UIARRRSkillSlotElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("------------SkillDetailInfo------------")]
        [SerializeField]
        private UISkillIconElement _uISkillIconElement;

        [SerializeField]
        private Image _uISkillDragIcon;

        [Header("------------SkillSlotDefaultUI------------")]
        [SerializeField]
        private Image m_skillMinusImage;

        [SerializeField]
        private Button m_slotBtn;


        [Header("------------SkillSlotLock------------")]
        [SerializeField]
        private Transform _lockImage;

        [Header("------------ColorImage------------")]
        public Image ColorImage;

        [Header("------------UnEquip------------")]
        public Button unEquipBtn;

        //------------------------------------------------------------------------------------
        private System.Action<int> m_callBack = null;

        private System.Action<int> _drag = null;
        private System.Action<int> _endDrag = null;

        private System.Action<int> _isOver = null;

        private System.Action<int> _unEquip = null;

        private bool m_useMinusImage = false;

        private int _slotID = -1;

        private CharacterSlotState m_mySlotState = CharacterSlotState.None;

        private bool _dragOn = false;
        private bool _dragLock = false;

        private ARRRSkillData _currentARRRSkillData = null;
        private DescendData _currentDescendData = null;
        private SynergyRuneData _currentSynergyRuneData = null;
        private GearData _currentCharacterGearData = null;

        //------------------------------------------------------------------------------------
        public void Init(System.Action<int> action,
            System.Action<int> drag,
            System.Action<int> endDrag,
            System.Action<int> isOver)
        {
            if (m_slotBtn)
                m_slotBtn.onClick.AddListener(OnClick_SlotBtn);

            if (unEquipBtn != null)
                unEquipBtn.onClick.AddListener(OnClick_unEquipBtn);

            m_callBack = action;

            _drag = drag;
            _endDrag = endDrag;
            _isOver = isOver;
        }
        //------------------------------------------------------------------------------------
        public void ConnectUnEquipBtn(System.Action<int> unEquip)
        {
            _unEquip = unEquip;
        }
        //------------------------------------------------------------------------------------
        public void SetSlotID(int id)
        {
            _slotID = id;
        }
        //------------------------------------------------------------------------------------
        public void DragLock(bool islock)
        {
            _dragLock = islock;
        }
        //------------------------------------------------------------------------------------
        public void SetSkill(ARRRSkillData characterSkillData)
        {
            if (_uISkillIconElement != null)
                _uISkillIconElement.gameObject.SetActive(false);

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(false);

            _dragOn = false;

            _currentARRRSkillData = characterSkillData;

            if (_uISkillDragIcon != null)
                _uISkillDragIcon.gameObject.SetActive(false);


            if (characterSkillData == null)
                return;

            SkillBaseData skillBaseData = Managers.ARRRSkillManager.Instance.GetSkillBaseData(characterSkillData);

            if (_uISkillDragIcon != null)
            {
                _uISkillDragIcon.sprite = Managers.SkillManager.Instance.GetSkillIcon(skillBaseData);
            }


            if (_uISkillIconElement != null)
            { 
                _uISkillIconElement.gameObject.SetActive(true);
                _uISkillIconElement.SetSkillElement(skillBaseData);
            }

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        public void SetSkill(DescendData characterSkillData)
        {
            if (_uISkillIconElement != null)
                _uISkillIconElement.gameObject.SetActive(false);

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(false);

            _dragOn = false;

            _currentDescendData = characterSkillData;

            if (_uISkillDragIcon != null)
                _uISkillDragIcon.gameObject.SetActive(false);


            if (characterSkillData == null)
                return;


            if (_uISkillDragIcon != null)
            {
                _uISkillDragIcon.sprite = Managers.DescendManager.Instance.GetDescendIcon(characterSkillData);
            }


            if (_uISkillIconElement != null)
            {
                _uISkillIconElement.gameObject.SetActive(true);
                _uISkillIconElement.SetSkillElement(characterSkillData);
            }

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        public void SetSkill(SynergyRuneData characterSkillData)
        {
            if (_uISkillIconElement != null)
                _uISkillIconElement.gameObject.SetActive(false);

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(false);

            _dragOn = false;

            _currentSynergyRuneData = characterSkillData;

            if (_uISkillDragIcon != null)
                _uISkillDragIcon.gameObject.SetActive(false);


            if (characterSkillData == null)
                return;


            if (_uISkillDragIcon != null)
            {
                _uISkillDragIcon.sprite = Managers.SynergyRuneManager.Instance.GetDescendIcon(characterSkillData);
            }


            if (_uISkillIconElement != null)
            {
                _uISkillIconElement.gameObject.SetActive(true);
                _uISkillIconElement.SetSkillElement(characterSkillData);
            }

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        public SynergyRuneData GetSynergyRuneData()
        {
            return _currentSynergyRuneData;
        }
        //------------------------------------------------------------------------------------
        public void SetSkill(GearData characterSkillData)
        {
            if (_uISkillIconElement != null)
                _uISkillIconElement.gameObject.SetActive(false);

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(false);

            _dragOn = false;

            _currentCharacterGearData = characterSkillData;

            if (_uISkillDragIcon != null)
                _uISkillDragIcon.gameObject.SetActive(false);


            if (characterSkillData == null)
                return;


            if (_uISkillDragIcon != null)
            {
                _uISkillDragIcon.sprite = Managers.GearManager.Instance.GetDescendIcon(characterSkillData);
            }


            if (_uISkillIconElement != null)
            {
                _uISkillIconElement.gameObject.SetActive(true);
                _uISkillIconElement.SetSkillElement(characterSkillData);
            }

            if (m_skillMinusImage != null)
                m_skillMinusImage.gameObject.SetActive(true);
        }
        //------------------------------------------------------------------------------------
        public GearData GetGearData()
        {
            return _currentCharacterGearData;
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SlotBtn()
        {
            if (m_callBack != null)
                m_callBack(_slotID);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_unEquipBtn()
        {
            if (_unEquip == null)
                m_callBack?.Invoke(_slotID);
            else
                _unEquip.Invoke(_slotID);
        }
        //------------------------------------------------------------------------------------
        public void VisibleLock(bool enable)
        {
            if (_lockImage != null)
                _lockImage.gameObject.SetActive(enable);
        }
        //------------------------------------------------------------------------------------
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (_dragLock == true)
                return;

            if (_currentARRRSkillData == null && _currentDescendData == null && _currentSynergyRuneData == null)
                return;

            _dragOn = true;

            if (_uISkillDragIcon != null)
                _uISkillDragIcon.gameObject.SetActive(true);

            transform.SetAsLastSibling();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (_dragLock == true)
                return;

            if (_dragOn == true)
            {
                if (_uISkillDragIcon != null)
                {
                    Camera m_characterCamera = BattleSceneCamera.Instance.BattleCamera;
                    Camera m_uiCamera = UI.UIManager.Instance.screenCanvasCamera;

                    _uISkillDragIcon.transform.position = m_uiCamera.ScreenToWorldPoint(Input.mousePosition);
                    Vector3 localpos = _uISkillDragIcon.transform.localPosition;

                    localpos.z = 0.0f;
                    _uISkillDragIcon.transform.localPosition = localpos;
                }

                _drag?.Invoke(_slotID);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_dragLock == true)
                return;

            _dragOn = false;

            if (_uISkillDragIcon != null)
                _uISkillDragIcon.gameObject.SetActive(false);

            _endDrag?.Invoke(_slotID);
        }


        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            if (_dragLock == true)
                return;

            Debug.Log(_slotID);
            _isOver?.Invoke(_slotID);
        }
    }
}