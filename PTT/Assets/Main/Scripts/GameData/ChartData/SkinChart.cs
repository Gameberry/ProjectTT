using System.Collections;
using System.Collections.Generic;

namespace GameBerry.Chart
{
    public class SkinInfo
    {
        public int Index;
        public SkinSlotType SkinType;
        public string SkinName;
    }

    public class SkinChart : ChartBase
    {
        public SkinInfo this[int index] => rows[index];
        public SkinInfo[] rows;

        // Lookup용 Dictionary들
        private Dictionary<int, SkinInfo> _indexToSkin;
        private Dictionary<SkinSlotType, List<SkinInfo>> _skinTypeToSkins;


        public override bool IsLoaded()
        {
            return rows != null;
        }

        /// <summary>
        /// Chart 로드가 끝난 후, Lookup용 Dictionary를 빌드해준다.
        /// 로딩 직후 1번만 호출해도 되고, 아래 메서드에서 lazy하게 호출해도 됨.
        /// </summary>
        public override void LoadComplete()
        {
            if (rows == null)
            {
                UnityEngine.Debug.LogError("SkinChart rows is null. Cannot build lookup.");
                return;
            }

            _indexToSkin = new Dictionary<int, SkinInfo>(rows.Length);
            _skinTypeToSkins = new Dictionary<SkinSlotType, List<SkinInfo>>();

            foreach (var row in rows)
            {
                if (row == null)
                    continue;

                // Index → SkinInfo
                _indexToSkin[row.Index] = row;

                // SkinType → List<SkinInfo>
                if (!_skinTypeToSkins.TryGetValue(row.SkinType, out var list))
                {
                    list = new List<SkinInfo>();
                    _skinTypeToSkins[row.SkinType] = list;
                }

                list.Add(row);
            }
        }

        /// <summary>
        /// Index로 SkinInfo 단건 조회 (없으면 default/null)
        /// </summary>
        public SkinInfo GetSkinInfo(int index)
        {
            return _indexToSkin.TryGetValue(index, out var info)
                ? info
                : null;
        }

        /// <summary>
        /// Index로 SkinInfo를 안전하게 조회하는 TryGet 패턴
        /// </summary>
        public bool TryGetSkinInfo(int index, out SkinInfo info)
        {
            return _indexToSkin.TryGetValue(index, out info);
        }

        /// <summary>
        /// SkinSlotType 기준으로 해당 타입의 스킨 리스트 반환
        /// (없으면 빈 배열 반환)
        /// </summary>
        public SkinInfo[] GetSkinSlotInfoList(SkinSlotType type)
        {
            if (_skinTypeToSkins.TryGetValue(type, out var list))
            {
                // 외부에서 수정 못하게 배열 복사 or ToArray
                return list.ToArray();
            }

            return System.Array.Empty<SkinInfo>();
        }

        /// <summary>
        /// Index로 이름만 가져오기.
        /// 못 찾으면 빈 문자열 반환 + Warning 로그
        /// </summary>
        public string GetSkinName(int index)
        {
            if (_indexToSkin.TryGetValue(index, out var info))
            {
                return info.SkinName ?? string.Empty;
            }

            UnityEngine.Debug.LogWarning($"Skin index {index} not found!");
            return string.Empty;
        }
    }

}