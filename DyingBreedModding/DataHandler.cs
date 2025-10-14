using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JCDyingBreedConfigurator
{

    internal class DataHandler
    {

    }

    #region Data Templates
    
    public class DataConfig
    {

    }

    [Serializable]
    public class CommonDataBlueprint
    {
        public string key;
        public string faction_DONTCHANGETHIS = "";
        public int Health;
        public int MinAttackDamage;
        public int MaxAttackDamage;
        public float AttackSpeed;
        public float AttackSpread;
        public float AttackRate;
        public float AttackRange;
        public float detectionRange;
        public float FogDiscoveryOuter;
        public int prodCost;
        public int prodTimeCost;
        public string AttackDamageType = "Bullet";
        public string ArmorClass = "Unarmored";
    }

    [Serializable]
    public class UnitDataBlueprint : CommonDataBlueprint
    {
        public string Name => key;
        //public bool autoRepair;
        //public int autoRepairRange;
        //public int autoRepairTime;
        public float speed;
    }

    [Serializable]
    public class BuildingDataBlueprint : CommonDataBlueprint
    {
        public string Name => key;
        public int power;
        public int sellPrice;
        public float healthRepairInterval;
        public int repairCostInterval;
        //public List<string> unitSpawnWhenDestroyed = new List<string>();
    }

    [Serializable]
    public class DamageTableBluePrint
    {
        public string key;
        public Dictionary<string, Dictionary<string, float>> attackTypeDamageModifier = new Dictionary<string, Dictionary<string, float>>();
    }

    #endregion
}