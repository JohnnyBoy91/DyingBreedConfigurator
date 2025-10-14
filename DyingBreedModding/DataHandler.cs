using System;
using System.Collections.Generic;

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
    public class UnitDataBlueprint
    {
        public string key;
        public int Health;
        public float speed;
        public int MinAttackDamage;
        public int MaxAttackDamage;
        public float AttackSpeed;
        public float AttackSpread;
        public float AttackRate;
        public float AttackRange;
        public int prodCost;
        public int prodTimeCost;
        public string faction_DONTCHANGETHIS = "";
        public string ArmorClass = "Unarmored";
        public string AttackDamageType = "Bullet";
    }

    #endregion
}