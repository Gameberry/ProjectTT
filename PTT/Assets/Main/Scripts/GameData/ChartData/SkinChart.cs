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

        //------------------------------------------------------------------------------------
        public override bool IsLoaded()
        {
            return rows != null;
        }
        //------------------------------------------------------------------------------------
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
        //------------------------------------------------------------------------------------
        public SkinInfo GetSkinInfo(int index)
        {
            return _indexToSkin.TryGetValue(index, out var info)
                ? info
                : null;
        }
        //------------------------------------------------------------------------------------
        public bool TryGetSkinInfo(int index, out SkinInfo info)
        {
            return _indexToSkin.TryGetValue(index, out info);
        }
        //------------------------------------------------------------------------------------
        public string GetSkinName(int index)
        {
            if (_indexToSkin.TryGetValue(index, out var info))
            {
                return info.SkinName ?? string.Empty;
            }

            UnityEngine.Debug.LogWarning($"Skin index {index} not found!");
            return string.Empty;
        }
        //------------------------------------------------------------------------------------
        public List<SkinInfo> GetSkinSlotInfoList(SkinSlotType type)
        {
            if (_skinTypeToSkins.TryGetValue(type, out var list))
            {
                return list;
            }

            return new List<SkinInfo>();
        }
        //------------------------------------------------------------------------------------
    }

}