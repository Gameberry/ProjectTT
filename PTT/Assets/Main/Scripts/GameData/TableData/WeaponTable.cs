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
        public long GetAmount(int itemId)
        {
            WeaponData weaponData = weapons.Find(x => x.itemId == itemId);
            if (weaponData == null)
                return 0;

            return weaponData.count;
        }
        //------------------------------------------------------------------------------------
        public void Add(int itemId, long amount)
        {
            if (amount == 0) return;

            WeaponData weaponData = weapons.Find(x => x.itemId == itemId);

            if (weaponData == null)
            {
                WeaponData newWeapon = new WeaponData { itemId = itemId, count = 0, level = 1, Awake = 0 };
                weaponData = newWeapon;
                weapons.Add(newWeapon);
            }

            long next = weaponData.count + amount;
            if (next < 0) next = 0;

            weaponData.count = next;
        }
        //------------------------------------------------------------------------------------
        public WeaponData GetWeaponData(int itemId)
        {
            return weapons.Find(x => x.itemId == itemId);
        }
        //------------------------------------------------------------------------------------
        public List<WeaponData> GetAllWeapons()
        {
            return weapons;
        }
        //------------------------------------------------------------------------------------
        #region Equip
        //------------------------------------------------------------------------------------
        public int GetEquippedWeaponId()
        {
            return equipWeapon;
        }
        //------------------------------------------------------------------------------------
        public void SetEquipped(int itemId)
        {
            equipWeapon = itemId;
        }
        //------------------------------------------------------------------------------------
        public bool IsEquipped(int itemId)
        {
            return equipWeapon == itemId;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Level
        //------------------------------------------------------------------------------------
        public bool LevelUp(int itemId)
        {
            WeaponData weaponData = weapons.Find(x => x.itemId == itemId);
            if (weaponData == null)
                return false;

            weaponData.level += 1;
            return true;
        }
        //------------------------------------------------------------------------------------
        public bool SetLevel(int itemId, int level)
        {
            WeaponData weaponData = weapons.Find(x => x.itemId == itemId);
            if (weaponData == null)
                return false;

            weaponData.level = level;
            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
        #region Awake
        //------------------------------------------------------------------------------------
        public bool AwakeUp(int itemId)
        {
            WeaponData weaponData = weapons.Find(x => x.itemId == itemId);
            if (weaponData == null)
                return false;

            weaponData.Awake += 1;
            return true;
        }
        //------------------------------------------------------------------------------------
        public bool SetAwake(int itemId, int awake)
        {
            WeaponData weaponData = weapons.Find(x => x.itemId == itemId);
            if (weaponData == null)
                return false;

            weaponData.Awake = awake;
            return true;
        }
        //------------------------------------------------------------------------------------
        #endregion
        //------------------------------------------------------------------------------------
    }
}
