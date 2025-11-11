using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class UICardMoveSynergyElement : MonoBehaviour
    {
        [SerializeField]
        private UIGambleChoiceSkillElement uIGambleChoiceSkillElement;

        private System.Action<UICardMoveSynergyElement> m_callback;

        public float curvDuration = 0.3f;
        public float convertDistance = 0.0f;
        public float convertDistanceRatio = 1.0f;


        //------------------------------------------------------------------------------------
        public void Init(System.Action<UICardMoveSynergyElement> action)
        {
            m_callback = action;
        }
        //------------------------------------------------------------------------------------
        public void SetCardData(ARR_CardGambleData selectSkill)
        {
            uIGambleChoiceSkillElement.gameObject.SetActive(true);
            uIGambleChoiceSkillElement.SetElement(selectSkill);
            //uIGambleChoiceSkillElement.EnableCardType(true);
            //uIGambleChoiceSkillElement.EnableCardGrade(true);
            //uIGambleChoiceSkillElement.EnableSkillCard(true);
            //uIGambleChoiceSkillElement.EnablePickCard(false);
            //uIGambleChoiceSkillElement.EnableCardSymbol(false);
        }
        //------------------------------------------------------------------------------------
        public async UniTask Explosion(Vector3 from, Vector3 to, Ease toEase, float toDuration)
        {
            from.z = 0;
            to.z = 0;

            Vector3 dirVecpos = to - from;
            dirVecpos.Normalize();

            Vector3 crossDirVec = Vector3.zero;

            crossDirVec.x = -dirVecpos.y;
            crossDirVec.y = dirVecpos.x;

            crossDirVec.Normalize();

            float distance = MathDatas.GetDistance(from.x, from.y, to.x, to.y);

            float ConvertDis = (distance * convertDistanceRatio);

            if (convertDistance > 0)
                ConvertDis = convertDistance;

            //Vector3 convertPos = crossDirVec * ConvertDis * (Random.Range(0, 2) == 0 ? -1.0f : 1.0f);

            //Vector3 otherPos = from + convertPos;


                //float randomCrossDir = Random.Range(0, 2) == 0 ? -1.0f : 1.0f;
            float randomCrossDir = -1.0f;

            float duration = curvDuration;
            float startTime = Time.time;
            float endTime = Time.time + duration;

            while (Time.time < endTime)
            {
                float ratio = (Time.time - startTime) / duration;

                Vector3 newdirVec = -dirVecpos * MathDatas.Sin(180.0f * ratio);
                Vector3 newCrossdirVec = crossDirVec * MathDatas.Sin(90.0f * ratio) * randomCrossDir;

                Vector3 newDirpos = ConvertDis * ((newdirVec * 0.1f) + newCrossdirVec);

                if (ratio < 0.7f)
                {
                    transform.localScale = (1.0f - ratio).ToVector3();
                }
                else
                    transform.localScale = 0.3f.ToVector3();

                transform.position = from + newDirpos;

                Vector3 localpos = transform.localPosition;
                localpos.z = 0;
                transform.localPosition = localpos;

                await UniTask.Yield();
            }

            await transform.DOMove(to, toDuration).SetEase(toEase);

            endDirection();
        }
        //------------------------------------------------------------------------------------
        private void endDirection()
        {
            gameObject.SetActive(false);

            m_callback?.Invoke(this);
        }
        //------------------------------------------------------------------------------------
    }
}
