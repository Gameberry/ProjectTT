using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using LitJson;
using Cysharp.Threading.Tasks;

namespace GameBerry
{

    public class CharacterSkinData
    {
        public int Index;
        public int ResourceIndex;

        public V2Enum_Skin SkinType;
        public V2Enum_Grade SkinGrade;

        public int MaxLevel;

        public V2Enum_Goods LevelUpCostGoodsType = V2Enum_Goods.Point;
        public int LevelUpCostGoodsParam1 = V2Enum_Point.SkinLevelUP.Enum32ToInt();
        public double LevelUpCostGoodsParam2;
        public double LevelUpCostGoodsParam3;

        public V2Enum_Stat OwnEffectType;
        public double OwnEffectBaseValue;
        public double OwnEffectAddValue;

        public double SurpassAddValue;

        public int IsLevelUp;

        public int IsPaid;

        public int SkinIndex;

        public string NameLocalStringKey;
        public string IconStringKey;
        public string ResourceFxKey;

        public GearData MyFakeGearData = null;
    }


    public class CharacterSkinLocalTable : LocalTableBase
    {
        private Dictionary<int, CharacterSkinData> m_skinData_Dic = new Dictionary<int, CharacterSkinData>();
        private Dictionary<V2Enum_Skin, List<CharacterSkinData>> m_skinDataList_Dic = new Dictionary<V2Enum_Skin, List<CharacterSkinData>>();

        //------------------------------------------------------------------------------------
        public override async UniTask InitData_Async()
        {
            List<CharacterSkinData> characterSkinDatas = await TheBackEnd.TheBackEnd_GameChart.GetListDat_Async<CharacterSkinData>("CharacterSkin");

            for (int i = 0; i < characterSkinDatas.Count; ++i)
            {
                CharacterSkinData characterSkinData = characterSkinDatas[i];

                characterSkinData.NameLocalStringKey = string.Format("characterSkin/{0}/name", characterSkinData.ResourceIndex);

                if (characterSkinData.SkinType == V2Enum_Skin.SkinBody)
                    characterSkinData.IconStringKey = string.Format("creature/DarkKnight_{0}/icon", characterSkinData.ResourceIndex);
                else
                    characterSkinData.IconStringKey = string.Format("characterSkin/{0}/icon", characterSkinData.ResourceIndex);

                if (characterSkinData.SkinType == V2Enum_Skin.SkinWeapon)
                { 
                    characterSkinData.ResourceFxKey = string.Format("FX_DarkKnight_Weapon_{0}", characterSkinData.ResourceIndex);
                }

                m_skinData_Dic.Add(characterSkinData.Index, characterSkinData);

                if (m_skinDataList_Dic.ContainsKey(characterSkinData.SkinType) == false)
                    m_skinDataList_Dic.Add(characterSkinData.SkinType, new List<CharacterSkinData>());

                m_skinDataList_Dic[characterSkinData.SkinType].Add(characterSkinData);
            }
        }
        //------------------------------------------------------------------------------------
        public CharacterSkinData GetData(int id)
        {
            if (m_skinData_Dic.ContainsKey(id) == true)
                return m_skinData_Dic[id];

            return null;
        }
        //------------------------------------------------------------------------------------
        public List<CharacterSkinData> GeAlltData(V2Enum_Skin goodsType)
        {
            if (m_skinDataList_Dic.ContainsKey(goodsType) == true)
                return m_skinDataList_Dic[goodsType];

            return null;
        }
        //------------------------------------------------------------------------------------
    }
}