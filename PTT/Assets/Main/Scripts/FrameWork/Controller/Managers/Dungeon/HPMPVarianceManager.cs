using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameBerry.Contents;
using GameBerry.UI;

namespace GameBerry.Managers
{
    public class HPMPVarianceManager : MonoSingleton<HPMPVarianceManager>
    {
        private Dictionary<HpMpVarianceType, VarianceColor> m_varianceColor_Dic = new Dictionary<HpMpVarianceType, VarianceColor>();

        private List<UIHpMpVarianceText> m_variancePool = new List<UIHpMpVarianceText>();

        private UIHpMpVarianceText m_varianceObj;

        [SerializeField]
        private float m_varianceDirectionPos = 120.0f;

        [SerializeField]
        private float m_varianceDirectionDuration = 0.5f;

        [SerializeField]
        private float m_varianceDelayDuration = 0.5f;

        private Camera m_characterCamera;
        private Camera m_uiCamera;

        private bool m_visibleDamageFont = true;

        public float m_damageFontYGab = 0.1f;
        public float m_damageFontRandomX = 0.1f;

        Dictionary<UIHpMpVarianceText, Transform> m_damageTextRoot = new Dictionary<UIHpMpVarianceText, Transform>();
        Dictionary<Transform, List<UIHpMpVarianceText>> m_transformDamage = new Dictionary<Transform, List<UIHpMpVarianceText>>();

        int nextDamageFontIdx = 0;

        //------------------------------------------------------------------------------------
        protected override void Init()
        {
            RefreshVisibleDamageFont();
            GameSettingManager.Instance.AddListener(GameSettingBtn.VisibleDamageFont, RefreshVisibleDamageFont);
        }
        //------------------------------------------------------------------------------------
        protected override void Release()
        {
            GameSettingManager.Instance.RemoveListener(GameSettingBtn.VisibleDamageFont, RefreshVisibleDamageFont);
        }
        //------------------------------------------------------------------------------------
        private void RefreshVisibleDamageFont()
        {
            m_visibleDamageFont = GameSettingManager.Instance.IsOn(GameSettingBtn.VisibleDamageFont);
        }
        //------------------------------------------------------------------------------------
        public void InitVariance(List<VarianceColor> variancecolor)
        {
            m_characterCamera = BattleSceneCamera.Instance.BattleCamera;
            m_uiCamera = UIManager.Instance.screenCanvasCamera;

            for (int i = 0; i < variancecolor.Count; ++i)
            {
                m_varianceColor_Dic.Add(variancecolor[i].VarianceType, variancecolor[i]);
            }

            ResourceLoader.Instance.Load<GameObject>("ContentResources/InGameContent/Objects/UIHpMpVarianceText", o =>
            {
                GameObject Obj = o as GameObject;
                if (Obj != null)
                    m_varianceObj = Obj.GetComponent<UIHpMpVarianceText>();
            });

            for (int i = 0; i < 20; ++i)
            {
                CreateVarianceText();
            }
        }
        //------------------------------------------------------------------------------------
        private void CreateVarianceText()
        {
            if (m_varianceObj == null)
                return;

            GameObject clone = Instantiate(m_varianceObj.gameObject, UIManager.Instance.DamageCanvasContent);
            if (clone != null)
            { 
                m_variancePool.Add(clone.GetComponent<UIHpMpVarianceText>());
                clone.SetActive(false);
            }
        }
        //------------------------------------------------------------------------------------
        public void ShowVarianceText(HpMpVarianceType type, double variancevalue, Transform trans)
        {
            if (type == HpMpVarianceType.None)
                return;

            if (m_visibleDamageFont == false)
            {
                if (type != HpMpVarianceType.RecoveryHP)
                    return;
            }

            UIHpMpVarianceText variance = m_variancePool[nextDamageFontIdx];

            if (variance == null)
                return;

            if (variance.isAlive == true)
                variance.ForcePoolText();

            nextDamageFontIdx++;
            if (nextDamageFontIdx >= m_variancePool.Count)
                nextDamageFontIdx = 0;

            string text = string.Empty;
            if (type == HpMpVarianceType.Miss)
                text = "Miss";
            else if (type == HpMpVarianceType.Block)
                text = "Block";
            else if (type == HpMpVarianceType.Dead)
                text = "Dead";
            else if (type == HpMpVarianceType.Revive)
                text = "Revive";
            else if (type == HpMpVarianceType.KillingMoney)
                text = "+1Gold";
            else if (type == HpMpVarianceType.DoubleInterest)
                text = "DoubleInterest";
            else if (type == HpMpVarianceType.GoldGainTimer)
                text = "GoldGainTimer";
            else
                text = Util.GetAlphabetNumber(variancevalue);

            Color color = Color.white;
            VarianceColor colordata = null;
            if (m_varianceColor_Dic.ContainsKey(type) == true)
            {
                if (m_varianceColor_Dic.TryGetValue(type, out colordata) == true)
                {
                    if (colordata != null)
                        color = colordata.color;
                }
            }

            Vector3 ViewPortPos = m_characterCamera.WorldToScreenPoint(trans.position);
            Vector3 TextPos = m_uiCamera.ScreenToWorldPoint(ViewPortPos);

            if (m_transformDamage.ContainsKey(trans) == false)
                m_transformDamage.Add(trans, new List<UIHpMpVarianceText>());

            int selectidx = -1;
            for (int i = 0; i < m_transformDamage[trans].Count; ++i)
            {
                if (m_transformDamage[trans][i] == null)
                {
                    selectidx = i;
                    m_transformDamage[trans][i] = variance;
                    break;
                }
            }

            if (selectidx == -1)
            { 
                m_transformDamage[trans].Add(variance);
                selectidx = m_transformDamage[trans].Count - 1;
            }

            TextPos.y += m_damageFontYGab * selectidx;
            TextPos.x += Random.Range(m_damageFontRandomX * -1.0f, m_damageFontRandomX);

            variance.gameObject.SetActive(true);
            variance.transform.position = TextPos;
            variance.ShowVarianceText(text, colordata);
            //variance.ShowVarianceText(text, color, m_varianceDirectionPos, m_varianceDirectionDuration, m_varianceDelayDuration);
            variance.transform.SetAsLastSibling();
            if (m_damageTextRoot.ContainsKey(variance) == false)
                m_damageTextRoot.Add(variance, null);

            m_damageTextRoot[variance] = trans;

            m_transformDamage[trans].Add(variance);
        }
        //------------------------------------------------------------------------------------
        public void ShowVarianceText(HpMpVarianceType type, double variancevalue, Vector3 worldpos)
        {
            if (type == HpMpVarianceType.None)
                return;

            if (m_visibleDamageFont == false)
            {
                if (type != HpMpVarianceType.RecoveryHP)
                    return;
            }

            if (m_variancePool.Count <= 0)
                CreateVarianceText();

            UIHpMpVarianceText variance = m_variancePool[nextDamageFontIdx];

            if (variance == null)
                return;

            if (variance.isAlive == true)
                variance.ForcePoolText();

            nextDamageFontIdx++;
            if (nextDamageFontIdx >= m_variancePool.Count)
                nextDamageFontIdx = 0;

            string text = string.Empty;
            if (type == HpMpVarianceType.Miss)
                text = "Miss";
            else if (type == HpMpVarianceType.Block)
                text = "Block";
            else if (type == HpMpVarianceType.Dead)
                text = "Dead";
            else if (type == HpMpVarianceType.Revive)
                text = "Revive";
            else if (type == HpMpVarianceType.KillingMoney)
                text = "+1Gold";
            else if (type == HpMpVarianceType.DoubleInterest)
                text = "DoubleInterest";
            else
                text = Util.GetAlphabetNumber(variancevalue);

            Color color = Color.white;

            VarianceColor colordata = null;
            if (m_varianceColor_Dic.ContainsKey(type) == true)
            {
                if (m_varianceColor_Dic.TryGetValue(type, out colordata) == true)
                {
                    if (colordata != null)
                        color = colordata.color;
                }
            }

            Vector3 ViewPortPos = m_characterCamera.WorldToScreenPoint(worldpos);
            Vector3 TextPos = m_uiCamera.ScreenToWorldPoint(ViewPortPos);

            TextPos.x += Random.Range(m_damageFontRandomX * -1.0f, m_damageFontRandomX);

            variance.gameObject.SetActive(true);
            variance.transform.position = TextPos;
            variance.ShowVarianceText(text, colordata);
            //variance.ShowVarianceText(text, color, m_varianceDirectionPos, m_varianceDirectionDuration, m_varianceDelayDuration);
            variance.transform.SetAsLastSibling();
        }
        //------------------------------------------------------------------------------------
        public void PoolVarianceText(UIHpMpVarianceText varianceText, bool dontSetactiveFalse = false)
        {
            if (varianceText == null)
                return;

            if (m_damageTextRoot.ContainsKey(varianceText) == true)
            {
                Transform trans = m_damageTextRoot[varianceText];

                if (trans != null)
                {
                    List<UIHpMpVarianceText> list = m_transformDamage[trans];

                    int idx = list.IndexOf(varianceText);
                    if (idx != -1)
                        list[idx] = null;

                    m_damageTextRoot[varianceText] = null;
                }
            }

            if (dontSetactiveFalse == false)
                varianceText.gameObject.SetActive(false);
            //m_variancePool.Enqueue(varianceText);
        }
        //------------------------------------------------------------------------------------
    }
}