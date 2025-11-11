using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{
    public class DefineData
    {
        public int Index;
        public V2Enum_DefineType DefineType;
        public string DefineValue;
    }

    public class DefineLocalTable : LocalTableBase
    {
        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            JsonData rows = null;

            TheBackEnd.TheBackEnd_GameChart.GetBackEndChart("Define", o =>
            { rows = o; });

            await UniTask.WaitUntil(() => rows != null);

            List<DefineData> m_defineDatas = JsonConvert.DeserializeObject<List<DefineData>>(rows.ToJson());

            for (int i = 0; i < m_defineDatas.Count; ++i)
            {
                switch (m_defineDatas[i].DefineType)
                {
                    case V2Enum_DefineType.PenetrationFactorMin:
                        {
                            Define.PenetrationFactorMin = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.ResistanceFactorMin:
                        {
                            Define.ResistanceFactorMin = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.InterestTimer:
                        {
                            Define.InterestTimer = m_defineDatas[i].DefineValue.ToFloat();
                            break;
                        }
                    case V2Enum_DefineType.InterestMaxReward:
                        {
                            Define.InterestMaxReward = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.InterestRate:
                        {
                            Define.InterestRate = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.GambleMinimumRate:
                        {
                            Define.GambleMinimumRate = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.GambleReinforcementMaxLevel:
                        {
                            Define.GambleReinforcementMaxLevel = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.DefaultSkin:
                        {
                            Define.DefaultSkin = m_defineDatas[i].DefineValue;
                            break;
                        }
                    case V2Enum_DefineType.DefenseStandard:
                        {
                            Define.DefenseStandard = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.MaximumDecreaseAtt:
                        {
                            Define.MaximumDecreaseAtt = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.MaximumDecreaseArmor:
                        {
                            Define.MaximumDecreaseArmor = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.DefaultGainGold:
                        {
                            Define.DefaultGainGold = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.NexusMonsterIndex:
                        {
                            Define.NexusMonsterIndex = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    //case V2Enum_DefineType.NewCharacterSkill:
                    //    {
                    //        Define.NewCharacterSkill = JsonConvert.DeserializeObject<List<int>>(m_defineDatas[i].DefineValue);
                    //        break;
                    //    }

                    //case V2Enum_DefineType.NewCharacterEquipSkill:
                    //    {
                    //        Define.NewCharacterEquipSkill = JsonConvert.DeserializeObject<List<int>>(m_defineDatas[i].DefineValue);
                    //        break;
                    //    }

                    case V2Enum_DefineType.StageStartGold:
                        {
                            Define.StageStartGold = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }


                    case V2Enum_DefineType.StaminaChargeTime:
                        {
                            Define.StaminaChargeTime = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.MaxStamina:
                        {
                            Define.MaxStamina = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.RequiredStamina:
                        {
                            Define.RequiredStamina = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.StaminaIndex:
                        {
                            Define.StaminaIndex = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }


                    case V2Enum_DefineType.JokerCardProb:
                        {
                            Define.JokerCardProb = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.JokerCardCount:
                        {
                            Define.JokerCardCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }


                    case V2Enum_DefineType.LimitDailyAdStamina:
                        {
                            Define.LimitDailyAdStamina = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.StaminaChargeCount:
                        {
                            Define.StaminaChargeCount = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.StaminaPrice:
                        {
                            Define.StaminaPrice = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }

                    case V2Enum_DefineType.GasSynergyProb:
                        {
                            Define.GasSynergyProb = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.GasSynergyCount:
                        {
                            Define.GasSynergyCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                    case V2Enum_DefineType.GasHpRecovery:
                        {
                            Define.GasHpRecovery = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.GasIndex:
                        {
                            Define.GasIndex = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.RandomShopAdRefreshCount:
                        {
                            Define.RandomShopAdRefreshCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                    case V2Enum_DefineType.RandomShopRefreshCount:
                        {
                            Define.RandomShopRefreshCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                    case V2Enum_DefineType.RandomShopRefreshIndex:
                        {
                            Define.RandomShopRefreshIndex = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                    case V2Enum_DefineType.RandomShopRefreshValue:
                        {
                            Define.RandomShopRefreshValue = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.RandomShopSlot:
                        {
                            Define.RandomShopSlot = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.RandomShopFreeDia:
                        {
                            Define.RandomShopFreeDia = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.RandomShopFreeDiaAD:
                        {
                            Define.RandomShopFreeDiaAD = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.BuffAdDailyCount:
                        {
                            Define.BuffAdDailyCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                    case V2Enum_DefineType.BaseSellingItemOnce:
                        {
                            Define.BaseSellingItemOnce = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }


                    case V2Enum_DefineType.Research2SlotOpenCost:
                        {
                            Define.Research2SlotOpenCost = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                    case V2Enum_DefineType.Research3SlotOpenCost:
                        {
                            Define.Research3SlotOpenCost = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }



                    case V2Enum_DefineType.ResearchGoodsEarn:
                        {
                            Define.ResearchGoodsEarn = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.ResearchChargingTime:
                        {
                            Define.ResearchChargingTime = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }


                    case V2Enum_DefineType.ResearchGoodsLimitCount:
                        {
                            Define.ResearchGoodsLimitCount = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.ResearchGoodsIndex:
                        {
                            Define.ResearchGoodsIndex = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }


                    case V2Enum_DefineType.ResearchADTime:
                        {
                            Define.ResearchADTime = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }
                    case V2Enum_DefineType.ResearchADTimeCount:
                        {
                            Define.ResearchADTimeCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }


                    case V2Enum_DefineType.ResearchAccelTicketIndex:
                        {
                            Define.ResearchAccelTicketIndex = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                    case V2Enum_DefineType.ResearchAccelTime:
                        {
                            Define.ResearchAccelTime = m_defineDatas[i].DefineValue.ToDouble();
                            break;
                        }


                    case V2Enum_DefineType.SkillEvolutionUnlock:
                        {
                            Define.SkillEvolutionUnlock = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.SynergyMAXGoods:
                        {
                            Define.SynergyMAXGoods = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }


                    case V2Enum_DefineType.DailySweepCount:
                        {
                            Define.DailySweepCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }

                    case V2Enum_DefineType.DailyDoubleRewardCount:
                        {
                            Define.DailyDoubleRewardCount = m_defineDatas[i].DefineValue.ToInt();
                            break;
                        }
                }
            }

            //Contents.DataLoadContent.LoadTable.Add(null);
            //TheBackEnd.TheBackEndManager.Instance.GetPlayerInfoTableData();
        }
        //------------------------------------------------------------------------------------
    }
}