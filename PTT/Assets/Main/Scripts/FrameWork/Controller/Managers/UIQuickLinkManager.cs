using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class UIQuickLinkManager : MonoSingleton<UIQuickLinkManager>
    {
        private Event.SetCharacterContentNavBarStateMsg SetCharacterContentNavBarStateMsg = new Event.SetCharacterContentNavBarStateMsg();

        private Event.SetAllyContentNavBarStateMsg SetAllyContentNavBarStateMsg = new Event.SetAllyContentNavBarStateMsg();

        private Event.ShowShortCutAllyJewelryViewMsg ShowShortCutAllyJewelryViewMsg = new Event.ShowShortCutAllyJewelryViewMsg();

        private Event.SetDungeonContentDialogStateMsg SetDungeonContentDialogStateMsg = new Event.SetDungeonContentDialogStateMsg();

        private Event.SetShopSummonDialogStateMsg SetShopSummonDialogStateMsg = new Event.SetShopSummonDialogStateMsg();
        private Event.SetShopGerneralDialogStateMsg SetShopGerneralDialogStateMsg = new Event.SetShopGerneralDialogStateMsg();

        private Event.SetPassDialogStateMsg SetPassDialogStateMsg = new Event.SetPassDialogStateMsg();

        //------------------------------------------------------------------------------------
        public void ShowQuickLink(ContentDetailList contentDetailList)
        {
            if (Managers.GuideInteractorManager.Instance.PlayResearchTutorial == true)
                return;

            switch (contentDetailList)
            {
                //case ContentDetailList.Character:
                //case ContentDetailList.CharacterGear:
                //case ContentDetailList.CharacterSkill:
                //case ContentDetailList.CharacterFame:
                //case ContentDetailList.CharacterMastery:
                //case ContentDetailList.CharacterTrait:
                //    {
                //        if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Character) == false)
                //        { 
                //            ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Character);
                //            break;
                //        }
                //        else if (contentDetailList == ContentDetailList.CharacterFame)
                //        {
                //            if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Fame) == false)
                //            {
                //                ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Fame);
                //                break;
                //            }
                //        }
                //        else if (contentDetailList == ContentDetailList.CharacterMastery)
                //        {
                //            if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Mastery) == false)
                //            {
                //                ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Mastery);
                //                break;
                //            }
                //            else if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.MasteryNormal) == false)
                //            {
                //                ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.MasteryNormal);
                //                break;
                //            }
                //        }
                //        else if (contentDetailList == ContentDetailList.CharacterTrait)
                //        {
                //            if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Trait) == false)
                //            {
                //                ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Trait);
                //                break;
                //            }
                //        }

                //        if (contentDetailList != ContentDetailList.Character)
                //        {
                //            SetCharacterContentNavBarStateMsg.ContentDetailList = contentDetailList;
                //            Message.Send(SetCharacterContentNavBarStateMsg);
                //        }
                //        break;
                //    }
                //case ContentDetailList.Ally:
                //case ContentDetailList.AllyContent:
                //case ContentDetailList.AllyRune:
                //case ContentDetailList.AllyCompose:
                //case ContentDetailList.AllyPromoAllContent:
                //case ContentDetailList.AllyJewelry:
                //    {
                //        if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Ally) == false)
                //        {
                //            ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Ally);
                //            break;
                //        }
                //        else if (contentDetailList == ContentDetailList.AllyRune)
                //        {
                //            if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Rune) == false)
                //            {
                //                ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Rune);
                //                break;
                //            }
                //        }

                //        if (contentDetailList != ContentDetailList.Ally)
                //        {
                //            if (contentDetailList == ContentDetailList.AllyJewelry)
                //            {
                //                SetAllyContentNavBarStateMsg.ContentDetailList = ContentDetailList.AllyContent;
                //                Message.Send(SetAllyContentNavBarStateMsg);

                //                Message.Send(ShowShortCutAllyJewelryViewMsg);
                //            }
                //            else
                //            {
                //                SetAllyContentNavBarStateMsg.ContentDetailList = contentDetailList;
                //                Message.Send(SetAllyContentNavBarStateMsg);
                //            }
                //        }
                //        break;
                //    }
                case ContentDetailList.Shop:
                case ContentDetailList.ShopGeneral:
                case ContentDetailList.ShopRandomStore:
                case ContentDetailList.ShopSummon_Normal:
                case ContentDetailList.ShopSummon_Relic:
                case ContentDetailList.ShopSummon_Rune:
                case ContentDetailList.ShopSummon_Gear:
                case ContentDetailList.ShopInGameStore:
                case ContentDetailList.ShopInGameStore_Descend:
                case ContentDetailList.ShopInGameStore_Gold:
                case ContentDetailList.ShopInGameStore_Synergy:
                case ContentDetailList.ShopDiamondStore:
                case ContentDetailList.ShopVip:
                case ContentDetailList.ShopVipStore_AD:


                case ContentDetailList.ShopDescend:
                case ContentDetailList.ShopDescendStore:
                case ContentDetailList.ShopPackage:
                case ContentDetailList.ShopDailyWeek_DiaPackage:
                case ContentDetailList.ShopDailyWeek_WeekPackage:
                case ContentDetailList.ShopDailyWeek_DayPackage:
                case ContentDetailList.ShopDailyWeek_MonthPackage:
                case ContentDetailList.ShopDailyWeek:
                case ContentDetailList.ShopRelayPackage:
                case ContentDetailList.ShopVipStore_Dia:
                case ContentDetailList.ShopCharge:
                case ContentDetailList.ShopCharge_Gold:
                case ContentDetailList.ShopCharge_Dia:





                    {
                        if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Shop) == false)
                        {
                            ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Shop);
                            break;
                        }
                        //else if (contentDetailList == ContentDetailList.ShopDailyStore)
                        //{
                        //    if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.SummonCharacterWeapon) == false)
                        //    {
                        //        ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.SummonCharacterWeapon);
                        //        break;
                        //    }
                        //}
                        //else if (contentDetailList == ContentDetailList.ShopSummon_Normal)
                        //{
                        //    if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.SummonCharacterSkill) == false)
                        //    {
                        //        ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.SummonCharacterSkill);
                        //        break;
                        //    }
                        //}
                        //else if (contentDetailList == ContentDetailList.ShopSummon_Relic)
                        //{
                        //    if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.SummonAlly) == false)
                        //    {
                        //        ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.SummonAlly);
                        //        break;
                        //    }
                        //}

                        UI.UIManager.DialogEnter<UI.ShopGeneralDialog>();

                        switch (contentDetailList)
                        {
                            case ContentDetailList.ShopRandomStore:
                            case ContentDetailList.ShopSummon_Normal:
                            case ContentDetailList.ShopSummon_Relic:
                            case ContentDetailList.ShopSummon_Rune:
                            case ContentDetailList.ShopSummon_Gear:
                            case ContentDetailList.ShopInGameStore:
                            case ContentDetailList.ShopInGameStore_Descend:
                            case ContentDetailList.ShopInGameStore_Gold:
                            case ContentDetailList.ShopInGameStore_Synergy:
                            case ContentDetailList.ShopDiamondStore:
                            case ContentDetailList.ShopVipStore_AD:


                            case ContentDetailList.ShopDescend:
                            case ContentDetailList.ShopDescendStore:
                            case ContentDetailList.ShopPackage:
                            case ContentDetailList.ShopDailyWeek_DiaPackage:
                            case ContentDetailList.ShopDailyWeek_WeekPackage:
                            case ContentDetailList.ShopDailyWeek_DayPackage:
                            case ContentDetailList.ShopDailyWeek_MonthPackage:
                            case ContentDetailList.ShopDailyWeek:
                            case ContentDetailList.ShopRelayPackage:
                            case ContentDetailList.ShopVipStore_Dia:
                            case ContentDetailList.ShopCharge:
                            case ContentDetailList.ShopCharge_Gold:
                            case ContentDetailList.ShopCharge_Dia:
                                {
                                    SetShopGerneralDialogStateMsg.ContentDetailList = contentDetailList;
                                    Message.Send(SetShopGerneralDialogStateMsg);
                                    break;
                                }
                        }

                        break;
                    }
                case ContentDetailList.Pass:
                case ContentDetailList.PassWave:
                case ContentDetailList.PassCharacterLevel:
                case ContentDetailList.PassSkillLevel:
                    {
                        if (ContentOpenConditionManager.Instance.IsOpen(V2Enum_ContentType.Pass) == false)
                        {
                            ContentOpenConditionManager.Instance.ShowOpenConditionNotice(V2Enum_ContentType.Pass);
                            break;
                        }

                        UI.UIManager.DialogEnter<UI.LobbyPassDialog>();

                        if (contentDetailList != ContentDetailList.Pass)
                        {
                            SetPassDialogStateMsg.ContentDetailList = contentDetailList;
                            Message.Send(SetPassDialogStateMsg);
                        }
                        break;
                    }
            }
        }
    }
}