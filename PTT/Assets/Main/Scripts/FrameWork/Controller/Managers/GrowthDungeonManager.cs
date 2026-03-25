using System;
using System.Collections.Generic;
using GameBerry.Chart;
using GameBerry.Table;
using UnityEngine;

namespace GameBerry
{
    public class GrowthDungeonManager : Singleton<GrowthDungeonManager>
    {
        public event Action<Enum_Dungeon> OnGrowthDungeonProgressChanged;

        private static readonly Enum_Dungeon[] GrowthDungeonTypes =
        {
            Enum_Dungeon.GrowthWeapon,
            Enum_Dungeon.GrowthExperience,
            Enum_Dungeon.GrowthEquipment,
            Enum_Dungeon.GrowthTraining,
            Enum_Dungeon.GrowthEnhance,
        };

        private DungeonWeaponChart _dungeonWeaponChart;
        private DungeonExperienceChart _dungeonExperienceChart;
        private DungeonEquipmentChart _dungeonEquipmentChart;
        private DungeonTrainingChart _dungeonTrainingChart;
        private DungeonEnhanceChart _dungeonEnhanceChart;
        private DungeonProgressTable _dungeonProgressTable;
        private readonly Dictionary<Enum_Dungeon, Enum_PointType> _ticketPointTypes = new Dictionary<Enum_Dungeon, Enum_PointType>();

        protected override void Init()
        {
            _dungeonWeaponChart = GameChart.Get<DungeonWeaponChart>();
            _dungeonExperienceChart = GameChart.Get<DungeonExperienceChart>();
            _dungeonEquipmentChart = GameChart.Get<DungeonEquipmentChart>();
            _dungeonTrainingChart = GameChart.Get<DungeonTrainingChart>();
            _dungeonEnhanceChart = GameChart.Get<DungeonEnhanceChart>();
            _dungeonProgressTable = UserTable.Get<DungeonProgressTable>();

            _ticketPointTypes.Clear();
            _ticketPointTypes[Enum_Dungeon.GrowthWeapon] = Enum_PointType.DungeonWeaponTicket;
            _ticketPointTypes[Enum_Dungeon.GrowthExperience] = Enum_PointType.DungeonExperienceTicket;
            _ticketPointTypes[Enum_Dungeon.GrowthEquipment] = Enum_PointType.DungeonEquipmentTicket;
            _ticketPointTypes[Enum_Dungeon.GrowthTraining] = Enum_PointType.DungeonTrainingTicket;
            _ticketPointTypes[Enum_Dungeon.GrowthEnhance] = Enum_PointType.DungeonEnhanceTicket;
        }

        public static bool IsGrowthDungeon(Enum_Dungeon dungeonType)
        {
            for (int i = 0; i < GrowthDungeonTypes.Length; ++i)
            {
                if (GrowthDungeonTypes[i] == dungeonType)
                    return true;
            }

            return false;
        }

        public IReadOnlyList<Enum_Dungeon> GetDungeonTypes()
        {
            return GrowthDungeonTypes;
        }

        public string GetDungeonDisplayName(Enum_Dungeon dungeonType)
        {
            return dungeonType switch
            {
                Enum_Dungeon.GrowthWeapon => "무기 던전",
                Enum_Dungeon.GrowthExperience => "경험치 던전",
                Enum_Dungeon.GrowthEquipment => "장비 던전",
                Enum_Dungeon.GrowthTraining => "용사의 수련장",
                Enum_Dungeon.GrowthEnhance => "강화 던전",
                _ => "성장 던전",
            };
        }

        public string GetDungeonShortDescription(Enum_Dungeon dungeonType)
        {
            return dungeonType switch
            {
                Enum_Dungeon.GrowthWeapon => "머쉬맘의 점프 스턴을 피해 무기 재화를 획득합니다.",
                Enum_Dungeon.GrowthExperience => "부기를 처치해 제한 시간을 늘리며 경험치를 수급합니다.",
                Enum_Dungeon.GrowthEquipment => "포이즌 푸퍼의 독 확산으로 처치 수를 빠르게 채웁니다.",
                Enum_Dungeon.GrowthTraining => "잡몹 버프를 쌓은 뒤 엘리자를 마무리합니다.",
                Enum_Dungeon.GrowthEnhance => "크림슨 발록의 약화 타이밍에 극딜을 넣습니다.",
                _ => string.Empty,
            };
        }

        public Enum_PointType GetEntryTicketPointType(Enum_Dungeon dungeonType)
        {
            return _ticketPointTypes.TryGetValue(dungeonType, out Enum_PointType pointType)
                ? pointType
                : Enum_PointType.Max;
        }

        public string GetPointDisplayName(Enum_PointType pointType)
        {
            return pointType switch
            {
                Enum_PointType.Dia => "다이아",
                Enum_PointType.Gold => "골드",
                Enum_PointType.MagicShard => "마력 조각",
                Enum_PointType.BlackEssence => "흑정수",
                Enum_PointType.RoyalCoin => "로얄 코인",
                Enum_PointType.Pact => "서약",
                Enum_PointType.Starforce => "주문의 흔적",
                Enum_PointType.WeaponSummon => "무기 소환권",
                Enum_PointType.LanternSummon => "등불 소환권",
                Enum_PointType.Mileage => "마일리지",
                Enum_PointType.DungeonWeaponTicket => "무기 던전 입장권",
                Enum_PointType.DungeonExperienceTicket => "경험치 던전 입장권",
                Enum_PointType.DungeonEquipmentTicket => "장비 던전 입장권",
                Enum_PointType.DungeonTrainingTicket => "수련장 입장권",
                Enum_PointType.DungeonEnhanceTicket => "강화 던전 입장권",
                _ => pointType.ToString(),
            };
        }

        public int GetEntryTicketCost(Enum_Dungeon dungeonType, int stage)
        {
            return IsGrowthDungeon(dungeonType) && stage > 0 ? 1 : 0;
        }

        public int GetEntryTicketItemId(Enum_Dungeon dungeonType)
        {
            Enum_PointType pointType = GetEntryTicketPointType(dungeonType);
            if (pointType == Enum_PointType.Max)
                return 0;

            return GameChart.Get<PointChart>()?.GetByType(pointType)?.ItemId ?? 0;
        }

        public long GetEntryTicketCount(Enum_Dungeon dungeonType)
        {
            int itemId = GetEntryTicketItemId(dungeonType);
            if (itemId <= 0 || ItemManager.isAlive == false)
                return 0;

            return ItemManager.Instance.GetItemAmount(itemId);
        }

        public bool CanAffordEntryTicket(Enum_Dungeon dungeonType, int stage)
        {
            int itemId = GetEntryTicketItemId(dungeonType);
            int cost = GetEntryTicketCost(dungeonType, stage);
            if (itemId <= 0 || cost <= 0)
                return false;

            return GetEntryTicketCount(dungeonType) >= cost;
        }

        public bool TryGetInfo(Enum_Dungeon dungeonType, int stage, out DungeonRuntimeInfo info)
        {
            info = null;
            if (IsGrowthDungeon(dungeonType) == false)
                return false;

            return dungeonType switch
            {
                Enum_Dungeon.GrowthWeapon => _dungeonWeaponChart != null && _dungeonWeaponChart.TryGetInfo(stage, out DungeonWeaponInfo weaponInfo) && AssignInfo(weaponInfo, out info),
                Enum_Dungeon.GrowthExperience => _dungeonExperienceChart != null && _dungeonExperienceChart.TryGetInfo(stage, out DungeonExperienceInfo experienceInfo) && AssignInfo(experienceInfo, out info),
                Enum_Dungeon.GrowthEquipment => _dungeonEquipmentChart != null && _dungeonEquipmentChart.TryGetInfo(stage, out DungeonEquipmentInfo equipmentInfo) && AssignInfo(equipmentInfo, out info),
                Enum_Dungeon.GrowthTraining => _dungeonTrainingChart != null && _dungeonTrainingChart.TryGetInfo(stage, out DungeonTrainingInfo trainingInfo) && AssignInfo(trainingInfo, out info),
                Enum_Dungeon.GrowthEnhance => _dungeonEnhanceChart != null && _dungeonEnhanceChart.TryGetInfo(stage, out DungeonEnhanceInfo enhanceInfo) && AssignInfo(enhanceInfo, out info),
                _ => false,
            };
        }

        public IReadOnlyList<DungeonRuntimeInfo> GetRows(Enum_Dungeon dungeonType)
        {
            switch (dungeonType)
            {
                case Enum_Dungeon.GrowthWeapon:
                    return CastRows(_dungeonWeaponChart?.GetRows());
                case Enum_Dungeon.GrowthExperience:
                    return CastRows(_dungeonExperienceChart?.GetRows());
                case Enum_Dungeon.GrowthEquipment:
                    return CastRows(_dungeonEquipmentChart?.GetRows());
                case Enum_Dungeon.GrowthTraining:
                    return CastRows(_dungeonTrainingChart?.GetRows());
                case Enum_Dungeon.GrowthEnhance:
                    return CastRows(_dungeonEnhanceChart?.GetRows());
                default:
                    return null;
            }
        }

        public int GetMaxConfiguredStage(Enum_Dungeon dungeonType)
        {
            if (IsGrowthDungeon(dungeonType) == false)
                return 0;

            return dungeonType switch
            {
                Enum_Dungeon.GrowthWeapon => _dungeonWeaponChart?.GetMaxStage() ?? 0,
                Enum_Dungeon.GrowthExperience => _dungeonExperienceChart?.GetMaxStage() ?? 0,
                Enum_Dungeon.GrowthEquipment => _dungeonEquipmentChart?.GetMaxStage() ?? 0,
                Enum_Dungeon.GrowthTraining => _dungeonTrainingChart?.GetMaxStage() ?? 0,
                Enum_Dungeon.GrowthEnhance => _dungeonEnhanceChart?.GetMaxStage() ?? 0,
                _ => 0,
            };
        }

        public DungeonProgressData GetProgress(Enum_Dungeon dungeonType)
        {
            if (_dungeonProgressTable == null)
                return new DungeonProgressData { dungeonType = dungeonType };

            return _dungeonProgressTable.GetOrCreate(dungeonType);
        }

        public int GetCurrentStage(Enum_Dungeon dungeonType)
        {
            return Mathf.Max(1, GetProgress(dungeonType).currentStage);
        }

        public int GetMaxUnlockedStage(Enum_Dungeon dungeonType)
        {
            int maxConfiguredStage = Mathf.Max(1, GetMaxConfiguredStage(dungeonType));
            return Mathf.Clamp(GetProgress(dungeonType).maxStage, 1, maxConfiguredStage);
        }

        public bool CanEnter(Enum_Dungeon dungeonType, int stage)
        {
            if (TryGetInfo(dungeonType, stage, out _) == false)
                return false;

            return stage <= GetMaxUnlockedStage(dungeonType);
        }

        public bool SetCurrentStage(Enum_Dungeon dungeonType, int stage, bool immediate = true)
        {
            if (_dungeonProgressTable == null || TryGetInfo(dungeonType, stage, out _) == false)
                return false;

            _dungeonProgressTable.SetCurrent(dungeonType, 1, stage);
            _dungeonProgressTable.UpdateTable(immediate);
            OnGrowthDungeonProgressChanged?.Invoke(dungeonType);
            return true;
        }

        public bool SetMaxStage(Enum_Dungeon dungeonType, int stage, bool immediate = true)
        {
            if (_dungeonProgressTable == null || TryGetInfo(dungeonType, stage, out _) == false)
                return false;

            DungeonProgressData data = GetProgress(dungeonType);
            if (stage < data.maxStage)
                return false;

            _dungeonProgressTable.SetMax(dungeonType, 1, stage);
            _dungeonProgressTable.UpdateTable(immediate);
            OnGrowthDungeonProgressChanged?.Invoke(dungeonType);
            return true;
        }

        public bool PrepareDungeon(Enum_Dungeon dungeonType, int stage, bool immediate = true)
        {
            if (CanEnter(dungeonType, stage) == false)
                return false;

            return SetCurrentStage(dungeonType, stage, immediate);
        }

        public bool TryEnterDungeon(Enum_Dungeon dungeonType, int stage, bool immediate = true)
        {
            if (CanEnter(dungeonType, stage) == false)
                return false;

            int ticketItemId = GetEntryTicketItemId(dungeonType);
            int ticketCost = GetEntryTicketCost(dungeonType, stage);
            if (ticketItemId <= 0 || ticketCost <= 0)
                return false;

            if (CanAffordEntryTicket(dungeonType, stage) == false)
                return false;

            ConsumeItemResult consumeResult = ItemManager.Instance.ConsumeItem(ticketItemId, ticketCost, immediate);
            if (consumeResult.Success == false)
                return false;

            return SetCurrentStage(dungeonType, stage, immediate);
        }

        public bool TryAdvanceToNextStage(Enum_Dungeon dungeonType, bool immediate = true)
        {
            int currentStage = GetCurrentStage(dungeonType);
            int maxConfiguredStage = GetMaxConfiguredStage(dungeonType);
            if (currentStage >= maxConfiguredStage)
                return false;

            int nextStage = currentStage + 1;
            if (TryGetInfo(dungeonType, nextStage, out _) == false)
                return false;

            _dungeonProgressTable.SetCurrent(dungeonType, 1, nextStage);
            if (GetProgress(dungeonType).maxStage < nextStage)
                _dungeonProgressTable.SetMax(dungeonType, 1, nextStage);

            _dungeonProgressTable.UpdateTable(immediate);
            OnGrowthDungeonProgressChanged?.Invoke(dungeonType);
            return true;
        }

        public bool TryGrantRewards(DungeonRuntimeInfo info)
        {
            if (info == null)
                return false;

            TryGrantPointRewards(info.GetRewardPoints());

            return true;
        }

        private void TryGrantPointRewards(IReadOnlyList<DungeonRewardPointInfo> rewardPoints)
        {
            if (rewardPoints == null)
                return;

            for (int i = 0; i < rewardPoints.Count; ++i)
            {
                DungeonRewardPointInfo reward = rewardPoints[i];
                if (reward == null || reward.PointType == Enum_PointType.Max || reward.Amount <= 0)
                    continue;

                int itemId = GameChart.Get<PointChart>()?.GetByType(reward.PointType)?.ItemId ?? 0;
                if (itemId > 0)
                    ItemManager.Instance.AddItem(itemId, reward.Amount);
            }
        }

        private static bool AssignInfo<T>(T source, out DungeonRuntimeInfo info) where T : DungeonRuntimeInfo
        {
            info = source;
            return source != null;
        }

        private static IReadOnlyList<DungeonRuntimeInfo> CastRows<T>(IReadOnlyList<T> source) where T : DungeonRuntimeInfo
        {
            if (source == null)
                return null;

            List<DungeonRuntimeInfo> rows = new List<DungeonRuntimeInfo>(source.Count);
            for (int i = 0; i < source.Count; ++i)
                rows.Add(source[i]);

            return rows;
        }
    }
}
