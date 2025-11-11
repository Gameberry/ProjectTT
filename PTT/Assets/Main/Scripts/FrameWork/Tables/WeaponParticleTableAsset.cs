using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    [CreateAssetMenu(fileName = "WeaponParticleTable", menuName = "Table/WeaponParticleTable")]
    public class WeaponParticleTableAsset : ScriptableObject
    {
        public List<GameObject> m_weaponParticleDatas = new List<GameObject>();

        //------------------------------------------------------------------------------------
        public GameObject GetParticleObject(string particleFxKey)
        {
            GameObject weaponParticle = m_weaponParticleDatas.Find(x =>
            {
                if (x == null)
                    return false;

                return x.name == particleFxKey;
            });

            return weaponParticle;
        }
        //------------------------------------------------------------------------------------
    }
}