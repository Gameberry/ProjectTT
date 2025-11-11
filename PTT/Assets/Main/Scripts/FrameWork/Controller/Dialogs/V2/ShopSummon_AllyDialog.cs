using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.UI
{
    public class ShopSummon_AllyDialog : ShopSummonDialog
    {
        protected override void OnEnter()
        {
            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.AllySummon
                            || Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.JewelrySummon)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(3);
            }
        }
        //------------------------------------------------------------------------------------
        protected override void OnExit()
        {
            if (Managers.GuideInteractorManager.isAlive == false)
                return;

            if (Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.AllySummon
                            || Managers.GuideInteractorManager.Instance.GetCurrentGuide() == V2Enum_EventType.JewelrySummon)
            {
                Managers.GuideInteractorManager.Instance.SetGuideStep(3);
                Managers.GuideInteractorManager.Instance.SetPrevGuideInteractor();
            }
        }
        //------------------------------------------------------------------------------------
    }
}
