using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    [CreateAssetMenu(fileName = "BattleModeStaticData", menuName = "Table/BattleModeStaticData", order = 1)]
    public class BattleModeStaticDataAsset : ScriptableObject
    {
        [Header("-----------TestStage-----------")]
        public List<int> MonsterModelIdxs = new List<int>();
        public float MonsterAttackTurm = 1.0f;
        public int SpawnCount = 30;
        public float SpawnTurm = 5f;
        public float SpawnrouteRadius = 5f;
        public float MonsterMinSeparation = 0.5f;
        public float MonsterReturnRadius = 0.01f;
        public bool PlayerOutReturnMonster = true;

        public Vector3 MapRange_Min;
        public Vector3 MapRange_Max;

        public List<StatViewer> TempPlayerStat = new List<StatViewer>();
        public List<StatViewer> TempMonsterStat = new List<StatViewer>();

        public float MonsterHitDuration = 0.167f;
        public Color MonsterHitColor = Color.white;

        public float ComboReleaseTime = 2.0f;

        [Header("카메라 흔들기")]
        public bool NormalAttackShake = false;
        public float NormalAttackShake_strengthOverride = 0.01f;
        public float NormalAttackShake_durationOverride = 0.08f;

        public bool CriticalAttackShake = true;
        public float CriticalAttackShake_strengthOverride = 0.05f;
        public float CriticalAttackShake_durationOverride = 0.1f;

        [Header("-----------Pet-----------")]
        public float PetMoveSpeed = 4.0f;

        public float PetRadius = 1.5f;
        public float PetStartAngle = 125f; // Look 방향의 시작 각도
        public float PetSectorAngle = 115f;
    }
}