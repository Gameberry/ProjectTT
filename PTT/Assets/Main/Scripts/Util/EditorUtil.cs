using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using LitJson;
using Cysharp.Threading.Tasks;
using GameBerry.UI;
using System.Linq;


namespace GameBerry.Managers
{
    public class EditorUtil : MonoBehaviour
    {
#if UNITY_EDITOR
        #region GetConvertTime
        [Header("[GetConvertTime]")]
        public string CheckTimeConvert_Start = string.Empty;
        public string CheckTimeConvert_End = string.Empty;

        [ContextMenu("CheckTimeConvert")]
        private void CheckTimeConvert()
        { //2023-12-07T05:00:00Z
            System.DateTime startTime = System.DateTime.Parse(CheckTimeConvert_Start);
            System.DateTime endTime = System.DateTime.Parse(CheckTimeConvert_End);

            Debug.Log(string.Format("{0},{1}"
                , startTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
                , endTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")));
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
#endif
    }
}