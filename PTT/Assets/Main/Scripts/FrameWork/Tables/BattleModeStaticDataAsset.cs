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

        public Vector3 MapRange_Min;
        public Vector3 MapRange_Max;

        public List<StatViewer> TempPlayerStat = new List<StatViewer>();
        public List<StatViewer> TempMonsterStat = new List<StatViewer>();

        [Header("-----------Stage-----------")]
        public float Stage_StartXPos = 0.0f;
        public double Stage_NexusCreateDistance = 5.0f;

        public float Stage_WaveSpawnTurm = 0.5f;
        public float Stage_WaveSpawnRatio = 0.7f;
        public Vector3 Stage_WaveSpawnPos;
        public float Stage_WaveCreateDistance = 3.0f;

        public float Stage_AddEndLine = 3.0f;

        [Header("-----------Pet-----------")]
        public float PetMoveSpeed = 4.0f;

        public float PetRadius = 1.5f;
        public float PetStartAngle = 125f; // Look 방향의 시작 각도
        public float PetSectorAngle = 115f;
    }
}