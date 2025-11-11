using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    public class CCState_Painter : MonoBehaviour
    {
        [SerializeField]
        private Transform m_stunRoot;

        [SerializeField]
        private Transform m_otherCCRoot;


        private List<CCState_PaintIcon> m_otherCCPaintIcon = new List<CCState_PaintIcon>();

        private Dictionary<V2Enum_SkillEffectType, CCState_PaintIcon> m_cCPaintIcon_Dic = new Dictionary<V2Enum_SkillEffectType, CCState_PaintIcon>();

        //[SerializeField]
        private float m_otherCCIconPosTurm = 0.3f;

        //------------------------------------------------------------------------------------
        public void PlayCCPaint(V2Enum_SkillEffectType cCType)
        {
            if (m_cCPaintIcon_Dic.ContainsKey(cCType) == true)
                return;

            SpriteAnimation spriteAnimation = CCPaintManager.Instance.GetCCSpriteAnimation(cCType);

            if (spriteAnimation == null)
                return;

            CCState_PaintIcon cCState_PaintIcon = CCPaintManager.Instance.GetCCState_Painter();

            if (cCState_PaintIcon == null)
                return;

            if (cCType == V2Enum_SkillEffectType.Stun)
            {
                cCState_PaintIcon.transform.SetParent(m_stunRoot);
                cCState_PaintIcon.transform.ResetLocal();
            }
            else
            {
                cCState_PaintIcon.transform.SetParent(m_otherCCRoot);
                cCState_PaintIcon.transform.ResetLocal();
            }

            cCState_PaintIcon.gameObject.SetActive(true);
            cCState_PaintIcon.SetAniData(spriteAnimation);
            cCState_PaintIcon.PlayCCIconAni();

            m_cCPaintIcon_Dic.Add(cCType, cCState_PaintIcon);

            if (cCType != V2Enum_SkillEffectType.Stun)
            {
                m_otherCCPaintIcon.Add(cCState_PaintIcon);
                RefreshOtherCCPaintIcon();
            }
        }
        //------------------------------------------------------------------------------------
        public void StopCCPaint(V2Enum_SkillEffectType cCType)
        {
            if (m_cCPaintIcon_Dic.ContainsKey(cCType) == false)
                return;

            if (cCType != V2Enum_SkillEffectType.Stun)
            {
                CCState_PaintIcon cCState_PaintIcon = m_cCPaintIcon_Dic[cCType];
                m_otherCCPaintIcon.Remove(cCState_PaintIcon);
            }

            CCPaintManager.Instance.PoolCCState_Painter(m_cCPaintIcon_Dic[cCType]);
            m_cCPaintIcon_Dic.Remove(cCType);

            RefreshOtherCCPaintIcon();
        }
        //------------------------------------------------------------------------------------
        private void RefreshOtherCCPaintIcon()
        {
            if (m_otherCCPaintIcon.Count <= 0)
                return;

            if (m_otherCCPaintIcon.Count == 1)
            {
                m_otherCCPaintIcon[0].transform.ResetLocal();
            }

            bool isOddNum = m_otherCCPaintIcon.Count % 2 != 0;

            int positionCount = m_otherCCPaintIcon.Count / 2;
            if (isOddNum == false)
                positionCount--;

            float startPos = (m_otherCCIconPosTurm * positionCount) + (isOddNum == false ? (m_otherCCIconPosTurm * 0.5f) : 0.0f);
            startPos *= -1.0f;


            for (int i = 0; i < m_otherCCPaintIcon.Count; ++i)
            {
                Vector3 pos = Vector3.zero;
                pos.x = startPos;

                m_otherCCPaintIcon[i].transform.localPosition = pos;

                startPos += m_otherCCIconPosTurm;
            }
        }
        //------------------------------------------------------------------------------------
        public void ReleaseCC()
        {
            foreach (KeyValuePair<V2Enum_SkillEffectType, CCState_PaintIcon> pair in m_cCPaintIcon_Dic)
            {
                CCPaintManager.Instance.PoolCCState_Painter(pair.Value);
            }

            m_otherCCPaintIcon.Clear();
            m_cCPaintIcon_Dic.Clear();
        }
        //------------------------------------------------------------------------------------
        //private void Update()
        //{
        //    if (Input.GetKeyUp(KeyCode.N))
        //    {
        //        ReleaseCC();
        //    }

        //    if (Input.GetKeyUp(KeyCode.M))
        //    {
        //        for (int i = 0; i < (int)V2Enum_CrowdControlType.Max; ++i)
        //        {
        //            V2Enum_CrowdControlType cCType = (V2Enum_CrowdControlType)i;

        //            PlayCCPaint(cCType);
        //        }
        //    }

        //    if (Input.GetKeyUp(KeyCode.K))
        //    {
        //        V2Enum_CrowdControlType selectcc = V2Enum_CrowdControlType.None;

        //        foreach (KeyValuePair<V2Enum_CrowdControlType, CCState_PaintIcon> pair in m_cCPaintIcon_Dic)
        //        {
        //            selectcc = pair.Key;
        //        }

        //        StopCCPaint(selectcc);
        //    }
        //}
        ////------------------------------------------------------------------------------------
    }
}