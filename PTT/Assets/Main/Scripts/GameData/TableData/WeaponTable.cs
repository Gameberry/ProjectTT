using LitJson;
using BackEnd;
using System;
using System.Collections.Generic;

namespace GameBerry.Table
{
    public class WeaponData : ItemData, IPackable
    {
        public int level = 1;
        public int Awake = 0;

        public string Pack() => $"{PackUtil.PackValue(itemId)},{PackUtil.PackValue(count)},{PackUtil.PackValue(level)},{PackUtil.PackValue(Awake)}";

        public void Unpack(string str)
        {
            itemId = 0;
            instanceId = 0;
            count = 0;
            level = 1;
            Awake = 0;

            if (string.IsNullOrEmpty(str))
                return;

            var sp = str.Split(',');
            if (sp.Length > 0) itemId = PackUtil.UnpackValue<int>(sp[0]);
            if (sp.Length > 1) count = PackUtil.UnpackValue<long>(sp[1]);
            if (sp.Length > 2) level = PackUtil.UnpackValue<int>(sp[2]);
            if (sp.Length > 3) Awake = PackUtil.UnpackValue<int>(sp[3]);
        }
    }


    public class WeaponTable : TableBase
    {
        private const string weaponKey = "Weapon";
        private List<WeaponData> weapons = new();

        private const string equipWeaponKey = "Equip";
        private int equipWeapon = 0;

        //------------------------------------------------------------------------------------
        public override void SetData(JsonData data)
        {
            if (data == null || data.Count == 0) return;

            for (int i = 0; i < data.Count; ++i)
            {
                foreach (var key in data[i].Keys)
                {
                    if (key == "inDate") SetInData(data[i][key].ToString());
                    else if (key == weaponKey) weapons = PackUtil.UnpackList<WeaponData>(data[i][key].ToString());
                    else if (key == equipWeaponKey) equipWeapon = PackUtil.UnpackValue<int>(data[i][key].ToString());
                }
            }
        }
        //------------------------------------------------------------------------------------
        public override Param GetParam()
        {
            Param p = new Param();
            p.Add(weaponKey, PackUtil.PackList(weapons));
            p.Add(equipWeaponKey, PackUtil.PackValue(equipWeapon));
            return p;
        }
        //------------------------------------------------------------------------------------
        public long GetAmount(int pointId)
        {
            WeaponData pointData = weapons.Find(x => x.itemId == pointId);
            if (pointData == null)
                return 0;

            return pointData.count;
        }
        //------------------------------------------------------------------------------------
        public void Add(int pointId, long amount)
        {
            if (amount == 0) return;

            WeaponData pointData = weapons.Find(x => x.itemId == pointId);

            if (pointData == null)
            {
                WeaponData newPoint = new WeaponData { itemId = pointId, count = 0 };
                pointData = newPoint;
                weapons.Add(newPoint);
            }

            long next = pointData.count + amount;
            if (next < 0) next = 0;

            pointData.count = next;
        }
        //------------------------------------------------------------------------------------
    }
}