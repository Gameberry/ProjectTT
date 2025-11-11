using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [System.Serializable]
    public class SkillSpriteData
    {
        [Header("-----------Loop-----------")]
        public List<Sprite> LoopImage = new List<Sprite>();

        public bool UseLoopAnimation = false;
        public string LoopAnimationName = string.Empty;

        [Header("-----------Hit-----------")]
        public List<Sprite> HitImage = new List<Sprite>();

        public bool UseHitAnimation = false;
        public string HitAnimationName = string.Empty;

        [Header("-----------AttackEffectData-----------")]
        public string EffectAniName = string.Empty;
    }

    [CreateAssetMenu(fileName = "AnimationSpriteLibrary", menuName = "Table/AnimationSpriteLibrary", order = 1)]
    public class AnimationSpriteLibraryAsset : ScriptableObject
    {
        public List<Sprite> MySprites = new List<Sprite>();
        public Dictionary<string, Sprite> MySprites_Dic = new Dictionary<string, Sprite>();
        public Dictionary<string, Sprite> WeaponSprites_Dic = new Dictionary<string, Sprite>();

        public SkillSpriteData MySkillSpriteData;

        public void OnEnable()
        {
            MySprites_Dic.Clear();

            

            for (int i = 0; i < MySprites.Count; ++i)
            {
                if (name == "DarkKnight_Weapon")
                {
                    WeaponSprites_Dic.Add(MySprites[i].name, MySprites[i]);
                }
                else
                {
                    MySprites_Dic.Add(Util.GetSpriteVariationName(MySprites[i].name), MySprites[i]);
                }
            }
        }
        //------------------------------------------------------------------------------------
        [ContextMenu("test")]
        public void test()
        {
            MySprites_Dic.Clear();



            for (int i = 0; i < MySprites.Count; ++i)
            {
                if (name == "DarkKnight_Weapon")
                {
                    WeaponSprites_Dic.Add(MySprites[i].name, MySprites[i]);
                }
                else
                {
                    string str = Util.GetSpriteVariationName(MySprites[i].name);
                    if (MySprites_Dic.ContainsKey(str) == true)
                    {
                        Debug.LogError("Index" + i);
                        break;
                    }
                    MySprites_Dic.Add(Util.GetSpriteVariationName(MySprites[i].name), MySprites[i]);
                }
            }
        }


        public Sprite GetSprite(string spritename)
        {
            if (MySprites_Dic.ContainsKey(spritename) == false)
                return null;

            return MySprites_Dic[spritename];
        }
        //------------------------------------------------------------------------------------
        public Sprite GetWeaponSprite(string spritename)
        {
            if (WeaponSprites_Dic.ContainsKey(spritename) == false)
                return null;

            return WeaponSprites_Dic[spritename];
        }
        //------------------------------------------------------------------------------------
    }
}