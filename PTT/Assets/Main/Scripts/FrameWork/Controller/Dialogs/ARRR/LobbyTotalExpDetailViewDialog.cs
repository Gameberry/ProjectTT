using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GameBerry.UI
{
    public class LobbyTotalExpDetailViewDialog : IDialog
    {
        [SerializeField]
        private Transform _desc1_Group;

        [SerializeField]
        private TMP_Text _desc1;


        [SerializeField]
        private Transform _desc2_Group;

        [SerializeField]
        private TMP_Text _desc2;

        private System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
        //------------------------------------------------------------------------------------
        protected override void OnLoad()
        {
            //Message.AddListener<GameBerry.Event.ShowTotalExpDetailMsg>(ShowTotalExpDetail);
        }
        //------------------------------------------------------------------------------------
        protected override void OnUnload()
        {
            //Message.RemoveListener<GameBerry.Event.ShowTotalExpDetailMsg>(ShowTotalExpDetail);
        }
        //------------------------------------------------------------------------------------
        //private void ShowTotalExpDetail(GameBerry.Event.ShowTotalExpDetailMsg msg)
        //{
        //    if (msg.ContentDetailList != ContentDetailList.LobbySynergy
        //        && msg.ContentDetailList != ContentDetailList.LobbyDescend)
        //    {
        //        if (_desc2_Group != null)
        //            _desc2_Group.gameObject.SetActive(false);

        //        SynergyTotalLevelEffectData synergyTotalLevelEffectData = msg.SynergyTotalLevelEffectData;

        //        if (_desc1 != null)
        //        {
        //            if (synergyTotalLevelEffectData.TotalLevelEffectType == Enum_ARRR_TotalLevelType.CardGradeLimitBreak)
        //            {
        //                if (_desc1_Group != null)
        //                    _desc1_Group.gameObject.SetActive(true);
        //                _desc1.SetText(SynergyOperator.GetCardGradeLimitBreakLocalString(synergyTotalLevelEffectData.EffectParam_Value));
        //            }
        //            else
        //            {
        //                if (_desc1_Group != null)
        //                    _desc1_Group.gameObject.SetActive(false);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (_desc1_Group != null)
        //            _desc1_Group.gameObject.SetActive(true);

        //        Dictionary<V2Enum_Stat, ObscuredDouble> stat = msg.ContentDetailList == ContentDetailList.LobbyDescend ? Managers.DescendManager.Instance.ArrrSynergyTotalStatValues : Managers.SynergyManager.Instance.ArrrSynergyTotalStatValues;

        //        int i = 0;

        //        _stringBuilder.Clear();

        //        foreach (var pair in stat)
        //        {
        //            if (i == 0)
        //                _stringBuilder.Append(SynergyOperator.GetStatLocal(pair.Key, pair.Value));
        //            else
        //                _stringBuilder.Append(string.Format("\n{0}", SynergyOperator.GetStatLocal(pair.Key, pair.Value)));

        //            i++;
        //        }

        //        if (_desc1 != null)
        //            _desc1.SetText(_stringBuilder.ToString());

        //        if (msg.ContentDetailList == ContentDetailList.LobbySynergy)
        //        {
        //            if (_desc2 != null)
        //            {
        //                if (_desc2_Group != null)
        //                    _desc2_Group.gameObject.SetActive(true);

        //                _desc2.SetText(SynergyOperator.GetCardGradeLimitBreakLocalString(Managers.SynergyManager.Instance.GambleGradeLevel));
        //            }
        //        }
        //        else
        //        {
        //            if (_desc2_Group != null)
        //                _desc2_Group.gameObject.SetActive(false);
        //        }
        //    }
        //}
        //------------------------------------------------------------------------------------
    }
}