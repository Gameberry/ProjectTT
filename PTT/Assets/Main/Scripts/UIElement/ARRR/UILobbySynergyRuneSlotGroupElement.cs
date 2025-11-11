using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using Gpm.Ui;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UILobbySynergyRuneSlotGroupElement : MonoBehaviour
    {
        public V2Enum_ARR_SynergyType SynergyType = V2Enum_ARR_SynergyType.Max;

        [SerializeField]
        private Image _synergyIcon;

        [SerializeField]
        private List<UIARRRSkillSlotElement> uIARRRSkillSlotElements = new List<UIARRRSkillSlotElement>();

        private Dictionary<int, UIARRRSkillSlotElement> uIARRRSkillSlotElements_Dic = new Dictionary<int, UIARRRSkillSlotElement>();

        private System.Action<V2Enum_ARR_SynergyType, int> _callBack;
        private System.Action<V2Enum_ARR_SynergyType, int> _unEquipCallBack;

        public void Init(System.Action<V2Enum_ARR_SynergyType, int> action
            , System.Action<V2Enum_ARR_SynergyType, int> unEquipCallBack)
        {
            _callBack = action;
            _unEquipCallBack = unEquipCallBack;

            if (SynergyType == V2Enum_ARR_SynergyType.Max)
                return;

            Dictionary<ObscuredInt, ObscuredInt> slot = Managers.SynergyRuneManager.Instance.GetEquipSynergyEffect();

            int checkidx = 0;

            GambleCardSprite gambleCardSprite = StaticResource.Instance.GetGambleCardSpriteData(SynergyType);

            if (gambleCardSprite != null)
            {
                if (_synergyIcon != null)
                    _synergyIcon.sprite = gambleCardSprite.SynergyIcon;
            }

            foreach (var pair in slot)
            {
                if (uIARRRSkillSlotElements.Count > checkidx)
                {
                    uIARRRSkillSlotElements[checkidx].Init(OnClick_SlotBtn, null, null, null);
                    uIARRRSkillSlotElements[checkidx].ConnectUnEquipBtn(OnClick_UnEquipSlot);
                    uIARRRSkillSlotElements[checkidx].SetSlotID(pair.Key);
                    uIARRRSkillSlotElements[checkidx].DragLock(true);

                    if (uIARRRSkillSlotElements_Dic.ContainsKey(pair.Key) == false)
                        uIARRRSkillSlotElements_Dic.Add(pair.Key, uIARRRSkillSlotElements[checkidx]);

                    if (gambleCardSprite != null)
                    {
                        if (uIARRRSkillSlotElements[checkidx].ColorImage != null)
                            uIARRRSkillSlotElements[checkidx].ColorImage.color = gambleCardSprite.Bar;
                    }
                }

                checkidx++;
            }

            RefreshAllSlot();
        }
        //------------------------------------------------------------------------------------
        private void OnClick_SlotBtn(int slotid)
        {
            _callBack?.Invoke(SynergyType, slotid);
        }
        //------------------------------------------------------------------------------------
        private void OnClick_UnEquipSlot(int slotid)
        {
            _unEquipCallBack?.Invoke(SynergyType, slotid);
        }
        //------------------------------------------------------------------------------------
        public void RefreshAllSlot()
        {
            if (SynergyType == V2Enum_ARR_SynergyType.Max)
                return;

            Dictionary<ObscuredInt, ObscuredInt> slot = Managers.SynergyRuneManager.Instance.GetEquipSynergyEffect();

            foreach (var pair in slot)
            {
                if (uIARRRSkillSlotElements_Dic.ContainsKey(pair.Key) == false)
                    continue;

                UIARRRSkillSlotElement uIARRRSkillSlotElement = uIARRRSkillSlotElements_Dic[pair.Key];

                SynergyRuneData synergyRuneData = Managers.SynergyRuneManager.Instance.GetSynergyEffectData(pair.Value);
                uIARRRSkillSlotElement.SetSkill(synergyRuneData);

                bool isopen = Managers.SynergyRuneManager.Instance.IsOpenDescendSlot(pair.Key);
                uIARRRSkillSlotElement.VisibleLock(isopen == false);
            }
        }
        //------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------
    }
}