using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class PointDataManager : MonoSingleton<PointDataManager>
    {
        private StackableLocalTable _stackableLocalTable = null;

        private Dictionary<int, Sprite> _pointIcons = new Dictionary<int, Sprite>();

        protected override void Init()
        {
            _stackableLocalTable = Managers.TableManager.Instance.GetTableClass<StackableLocalTable>();


            CheckCheatDia(GetPointAmount(V2Enum_Point.Dia.Enum32ToInt()));
        }
        //------------------------------------------------------------------------------------
        public List<PointData> GetAllPointData()
        {
            return _stackableLocalTable.GetAllPointData();
        }
        //------------------------------------------------------------------------------------
        public string GetPointLocalKey(int index)
        {
            PointData pointData = _stackableLocalTable.GetPointData(index);
            if (pointData == null)
                return string.Empty;

            return pointData.NameLocalStringKey;
        }
        //------------------------------------------------------------------------------------
        public Sprite GetPointSprite(int index)
        {
            PointData pointData = _stackableLocalTable.GetPointData(index);
            if (pointData == null)
                return null;

            Sprite sp = null;

            int iconIndex = pointData.ResourceIndex;

            if (_pointIcons.ContainsKey(iconIndex) == false)
            {
                ResourceLoader.Instance.Load<Sprite>(string.Format(Define.PointIconPath, iconIndex), o =>
                {
                    sp = o as Sprite;
                    _pointIcons.Add(iconIndex, sp);
                });
            }
            else
                sp = _pointIcons[iconIndex];

            return sp;
        }
        //------------------------------------------------------------------------------------
        public void SetPointAmount(int index, double amount)
        {
            if (PointDataContainer.m_pointAmount.ContainsKey(index) == false)
            {
                PointDataContainer.m_pointAmount.Add(index, 0.0);
            }

            PointDataContainer.m_pointAmount[index] = amount;

            if (V2Enum_Point.Dia.Enum32ToInt() == index)
                CheckCheatDia(amount);
        }
        //------------------------------------------------------------------------------------
        public double GetPointAmount(int index)
        {
            if (PointDataContainer.m_pointAmount.ContainsKey(index) == true)
                return PointDataContainer.m_pointAmount[index];

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public double AddPointAmount(int index, double amount)
        {
            if (PointDataContainer.m_pointAmount.ContainsKey(index) == false)
            {
                PointDataContainer.m_pointAmount.Add(index, 0.0);
            }

            double currentAmount = PointDataContainer.m_pointAmount[index] + amount;

            PointDataContainer.m_pointAmount[index] = currentAmount;

            if (V2Enum_Point.Dia.Enum32ToInt() == index)
                CheckCheatDia(currentAmount);

            return currentAmount;
        }
        //------------------------------------------------------------------------------------
        public double UsePointAmount(int index, double amount)
        {
            if (PointDataContainer.m_pointAmount.ContainsKey(index) == true)
            {
                double currentAmount = PointDataContainer.m_pointAmount[index] - amount;

                if (currentAmount < 0.0)
                    currentAmount = 0.0;

                PointDataContainer.m_pointAmount[index] = currentAmount;

                if (V2Enum_Point.Dia.Enum32ToInt() == index)
                { 
                    CheckCheatDia(currentAmount);
                    PointDataContainer.AccumUseDia += amount;
                }

                return currentAmount;
            }

            return 0.0;
        }
        //------------------------------------------------------------------------------------
        public void CheckCheatDia(double diaAmount)
        {
            PointDataContainer.DiaAmountRecord = diaAmount;
            PointDataContainer.DiaAmount = diaAmount.ToString();

            //if (Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Product
            //    || Managers.SceneManager.Instance.BuildElement == BuildEnvironmentEnum.Stage)
            //{
            //    if (PlayerDataContainer.PlayerDontSearchCheat.GetDecrypted() == 1)
            //        return;

            //    if (diaAmount > 21624600)
            //    {
            //        if (Managers.TimeManager.Instance.GetDayCount() <= 2
            //        && ShopPostContainer.m_shopPostInfos.Count < 2)
            //        {
            //            TheBackEnd.TheBackEndManager.Instance.OnCheatingDetected();
            //        }
            //    }
            //}
        }
        //------------------------------------------------------------------------------------
    }
}
