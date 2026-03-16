using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Spine;
using Spine.Unity;
using GameBerry.Chart;

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
        public float MonsterAggroDistance = 6.0f;
        public float MonsterReturnRadius = 0.01f;
        public bool PlayerOutReturnMonster = true;
        public float MonsterWanderRadius = 1.5f;
        public float MonsterWanderIdleMinTime = 0.5f;
        public float MonsterWanderIdleMaxTime = 2.0f;

        public Vector3 MapRange_Min;
        public Vector3 MapRange_Max;

        [Header("-----------Stage Boss-----------")]
        public Vector3 StageBossPlayerSpawnPosition = new Vector3(-2.5f, 0f, 0f);
        public Vector3 StageBossMonsterSpawnPosition = new Vector3(2.5f, 0f, 0f);
        public float StageBossSubMonsterSpawnRadius = 0f;
        public float StageBossSubMonsterMinSeparation = 0f;

        public List<StatViewer> TempPlayerStat = new List<StatViewer>();
        public List<StatViewer> TempMonsterStat = new List<StatViewer>();

        public float MonsterHitDuration = 0.167f;
        public Color MonsterHitColor = Color.white;
        public float MonsterDeadDuration = 1.167f;
        public float ComboReleaseTime = 2.0f;
        public SkillInfo MonsterDefaultAttackData = new SkillInfo();

        [Header("Camera Shake")]
        public bool NormalAttackShake = false;
        public float NormalAttackShake_strengthOverride = 0.01f;
        public float NormalAttackShake_durationOverride = 0.08f;

        public bool CriticalAttackShake = true;
        public float CriticalAttackShake_strengthOverride = 0.05f;
        public float CriticalAttackShake_durationOverride = 0.1f;

        [Header("-----------Pet-----------")]
        public float PetMoveSpeed = 4.0f;

        public float PetRadius = 1.5f;
        public float PetStartAngle = 125f; // Start angle from look direction
        public float PetSectorAngle = 115f;
    }
}
