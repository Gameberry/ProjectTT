using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class AnimationSpriteManager : MonoSingleton<AnimationSpriteManager>
    {
        private Dictionary<string, AnimationSpriteLibraryAsset> m_aniSpriteLibrary_Dic = new Dictionary<string, AnimationSpriteLibraryAsset>();
        private Dictionary<string, Dictionary<int, string>> m_aniPath_Dic = new Dictionary<string, Dictionary<int, string>>();

        private const string LibraryDefaultPath = "CharAnimationResources/{0}/{1}";
        private const string LibraryWeaponPath = "CharAnimationResources/{0}";
        private const string LibraryPlayerWeaponPath = "CharAnimationResources/DarkKnight_Weapon";

        //------------------------------------------------------------------------------------
        public AnimationSpriteLibraryAsset GetAnimationSpriteLibraryAsset(string AniResourceKey, int varia)
        {
            if (m_aniSpriteLibrary_Dic.ContainsKey(GetBundlePath(AniResourceKey, varia)) == false)
            {
                CachedAnimationSpriteLibrary(AniResourceKey, varia);
            }

            return m_aniSpriteLibrary_Dic[GetBundlePath(AniResourceKey, varia)];
        }
        //------------------------------------------------------------------------------------
        public void CachedAnimationSpriteLibrary(string AniResourceKey, int varia)
        {
            string bundlepath = GetBundlePath(AniResourceKey, varia);

            AnimationSpriteLibraryAsset animationTableAsset = null;

            AssetBundleLoader.Instance.Load<AnimationSpriteLibraryAsset>(bundlepath, AniResourceKey, o =>
            {
                animationTableAsset = o as AnimationSpriteLibraryAsset;

                m_aniSpriteLibrary_Dic.Add(bundlepath, animationTableAsset);
            });
        }
        //------------------------------------------------------------------------------------
        public string GetBundlePath(string AniResourceKey, int varia)
        {
            if (m_aniPath_Dic.ContainsKey(AniResourceKey) == false)
                m_aniPath_Dic.Add(AniResourceKey, new Dictionary<int, string>());

            if (m_aniPath_Dic[AniResourceKey].ContainsKey(varia) == false)
                m_aniPath_Dic[AniResourceKey].Add(varia, string.Format(LibraryDefaultPath, AniResourceKey, varia));

            return m_aniPath_Dic[AniResourceKey][varia];
        }
        //------------------------------------------------------------------------------------
    }
}