using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry.Managers
{
    public class AggroManager : MonoSingleton<AggroManager>
    {
        private List<CharacterControllerBase> m_iFF_FriendCharacterControllerBases = new List<CharacterControllerBase>();
        private List<CharacterControllerBase> m_iFF_FoeCharacterControllerBases = new List<CharacterControllerBase>();

        //------------------------------------------------------------------------------------
        public void AddIFFCharacterAggro(CharacterControllerBase characterControllerBase)
        {
            if (characterControllerBase.IFFType == IFFType.IFF_Friend)
            {
                if (m_iFF_FriendCharacterControllerBases.Contains(characterControllerBase) == true)
                    return;

                m_iFF_FriendCharacterControllerBases.Add(characterControllerBase);
            }
            else
            {
                if (m_iFF_FoeCharacterControllerBases.Contains(characterControllerBase) == true)
                    return;

                m_iFF_FoeCharacterControllerBases.Add(characterControllerBase);
            }
        }
        //------------------------------------------------------------------------------------
        public void RemoveIFFCharacterAggro(CharacterControllerBase characterControllerBase)
        {
            if (characterControllerBase.IFFType == IFFType.IFF_Friend)
            {
                if (m_iFF_FriendCharacterControllerBases.Contains(characterControllerBase) == false)
                    return;

                m_iFF_FriendCharacterControllerBases.Remove(characterControllerBase);
            }
            else
            {
                if (m_iFF_FoeCharacterControllerBases.Contains(characterControllerBase) == false)
                    return;

                m_iFF_FoeCharacterControllerBases.Remove(characterControllerBase);
            }
        }
        //------------------------------------------------------------------------------------
        public CharacterControllerBase GetIFFTargetCharacter(CharacterControllerBase myControllerBase)
        {
            CharacterControllerBase target = null;

            List<CharacterControllerBase> iFF_Target = myControllerBase.IFFType == IFFType.IFF_Friend ? m_iFF_FoeCharacterControllerBases : m_iFF_FriendCharacterControllerBases;

            float neardis = float.MaxValue;

            for (int i = 0; i < iFF_Target.Count; ++i)
            {
                if (iFF_Target[i] == null)
                    continue;

                if (iFF_Target[i].IsDead == true)
                    continue;

                if (target == null)
                    target = iFF_Target[i];

                float distance = GetDistance(myControllerBase, iFF_Target[i]);
                if (neardis > distance)
                {
                    target = iFF_Target[i];
                    neardis = distance;
                }
            }

            return target;
        }
        //------------------------------------------------------------------------------------
        public float GetDistance(CharacterControllerBase my, CharacterControllerBase enemy)
        {
            if (my == null || enemy == null)
                return -1.0f;

            return MathDatas.GetDistance(my.transform.position.x, my.transform.position.z, enemy.transform.position.x, enemy.transform.position.z);
        }
        //------------------------------------------------------------------------------------
        public List<CharacterControllerBase> GetAllTargetCharacter(CharacterControllerBase characterControllerBase)
        {
            if (characterControllerBase.IFFType == IFFType.IFF_Friend)
                return m_iFF_FoeCharacterControllerBases;

            return m_iFF_FriendCharacterControllerBases;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// 각 타겟 몬스터에 대해, 전역 중복 없이 N개씩 무작위 선택
        /// 자기 자신과 다른 타겟 몬스터들도 제외됨
        /// </summary>
        public static Dictionary<T, List<T>> GetNPerTarget_NoGlobalDup<T>(
            List<T> allMonsters,
            List<T> targetMonsters,
            int pickPerTarget)
        {
            var result = new Dictionary<T, List<T>>();
            var globalUsed = new HashSet<T>(); // 전체 중복 방지
            var allTargetsSet = new HashSet<T>(targetMonsters); // 타겟 몬스터 전체

            foreach (var target in targetMonsters)
            {
                var filtered = new List<T>();

                foreach (var monster in allMonsters)
                {
                    // 자기 자신 + 다른 타겟 몬스터 + 이미 선택된 몬스터 제외
                    if (allTargetsSet.Contains(monster)) continue;
                    if (globalUsed.Contains(monster)) continue;

                    filtered.Add(monster);
                }

                // Fisher-Yates 셔플
                Shuffle(filtered);

                int countToPick = Mathf.Min(pickPerTarget, filtered.Count);
                var picked = filtered.GetRange(0, countToPick);

                foreach (var p in picked)
                    globalUsed.Add(p);

                result[target] = picked;
            }

            return result;
        }
        //------------------------------------------------------------------------------------
        /// <summary>
        /// Fisher-Yates Shuffle
        /// </summary>
        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        //------------------------------------------------------------------------------------
    }
}