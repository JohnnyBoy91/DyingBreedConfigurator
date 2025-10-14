using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UniverseLib;
using static JCDyingBreedConfigurator.Utilities;

namespace JCDyingBreedConfigurator
{
    [BepInPlugin("JCDyingBreedConfigurator", "DyingBreedConfigurator", "1.0.0")]
    public class Plugin : BasePlugin
    {
        #region Plugin Core
        public GameObject DyingBreedMainModObject;
        // Plugin startup logic
        public override void Load()
        {
            Log.LogInfo("Plugin JCDyingBreedConfigurator is loaded!");
            ClassInjector.RegisterTypeInIl2Cpp<ModManager>();
            if (DyingBreedMainModObject == null)
            {
                DyingBreedMainModObject = new GameObject("DyingBreedConfiguratorMaster");
                UnityEngine.Object.DontDestroyOnLoad(DyingBreedMainModObject);
                DyingBreedMainModObject.hideFlags = HideFlags.HideAndDontSave;
                DyingBreedMainModObject.AddComponent<ModManager>();
            }
            else
            {
                DyingBreedMainModObject.AddComponent<ModManager>();
            }
            DyingBreedMainModObject.GetComponent<ModManager>().plugin = this;

            var harmony = new Harmony("JCDyingBreedConfigurator");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Universe.Init();
        }
        #endregion

        #region HarmonyPatches
        [HarmonyPatch(typeof(PlayerController), "Awake")]
        static class ModPlayerController
        {
            [HarmonyPriority(100)]
            private static void Postfix(PlayerController __instance)
            {
                ModManager.Log("PlayerControllerPatched");
                ModManager.Instance.ModifyStats();
            }
        }
        #endregion

        public class ModManager : MonoBehaviour
        {
            private static ModManager instance;
            public string[] datalines;
            public string[] settingslines;
            public Plugin plugin;
            private bool initialized;
            private bool dataInjected;
            private bool verboseLogging = false; 
            public static bool VerboseLogging() => Instance.verboseLogging;

            public List<UnitSoData> unitSODataList = new List<UnitSoData>();
            public List<BuildingSoData> buildingSODataList = new List<BuildingSoData>();

            public string unitDataConfigPath => modRootPath + "ModUnitData.json";
            public string modSettingsPath => modRootPath + "ModSettings.json";
            public string modRootPath => Directory.GetCurrentDirectory() + @"\BepInEx\plugins\DyingBreedConfigurator\";
            public const string generatedConfigFolderPath = @"DefaultConfigData\";

            //config delimiters
            public const string dlmList = ", ";
            public const string dlmWord = "_";
            public const string dlmKey = ":";
            public const string dlmComment = "//";
            public const string dlmNewLine = "\n";

            public static ModManager Instance
            {
                get
                {
                    return instance;
                }
            }

            internal void Awake()
            {
                instance = this;
            }

            internal void Update()
            {
                if (!initialized)
                {
                    InitializeMod();
                }

                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyUp(KeyCode.F10))
                {
                    Instance.plugin.Log.LogInfo("SendDevCMD");
                    ModifyStats();
                }
            }

            private void InitializeMod()
            {
                Instance.plugin.Log.LogInfo("confining cursor");
                //Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Cursor.lockState = CursorLockMode.Confined;

                if (!Directory.Exists(modRootPath + generatedConfigFolderPath)) Directory.CreateDirectory(modRootPath + generatedConfigFolderPath);
                initialized = true;
            }

            internal void ModifyStats()
            {
                if (dataInjected) return;

                foreach (var unitData in RuntimeHelper.FindObjectsOfTypeAll<UnitScriptableObject>())
                {
                    foreach (var unitDataB in unitData.unitData)
                    {
                        Log(CombineStrings('"'.ToString(), unitDataB.displayName, '"'.ToString(), ","));
                        unitSODataList.Add(unitDataB);
                    }
                }
                List<UnitDataBlueprint> unitDataBlueprints = new List<UnitDataBlueprint>();
                foreach (var unitSODataFromGame in unitSODataList)
                {
                    UnitDataBlueprint data = new UnitDataBlueprint();
                    data.key = unitSODataFromGame.displayName;
                    data.Health = unitSODataFromGame.Health;
                    data.MinAttackDamage = unitSODataFromGame.MinAttackDamage;
                    data.MaxAttackDamage = unitSODataFromGame.MaxAttackDamage;
                    data.speed = unitSODataFromGame.speed;
                    data.prodCost = unitSODataFromGame.prodCost;
                    data.prodTimeCost = unitSODataFromGame.prodIntervalCount[0];
                    data.AttackSpeed = unitSODataFromGame.AttackSpeed;
                    data.AttackSpread = unitSODataFromGame.AttackSpread;
                    data.AttackRate = unitSODataFromGame.AttackRate;
                    data.AttackRange = unitSODataFromGame.AttackRange;
                    data.faction_DONTCHANGETHIS = unitSODataFromGame.faction.ToString();
                    data.ArmorClass = unitSODataFromGame.ArmorClass.ToString();
                    data.AttackDamageType = unitSODataFromGame.AttackDamageType.ToString();
                    unitDataBlueprints.Add(data);
                }

                WriteJsonConfig(CombineStrings(modRootPath, generatedConfigFolderPath, "DefaultUnitData.json"), unitDataBlueprints);

                List<UnitDataBlueprint> moddedUnitData = (List<UnitDataBlueprint>)ReadJsonConfig<List<UnitDataBlueprint>>(unitDataConfigPath);
                foreach (var ModdedData in moddedUnitData)
                {
                    var unitToMod = unitSODataList.Where(x => x.displayName == ModdedData.key && x.faction.ToString() == ModdedData.faction_DONTCHANGETHIS).FirstOrDefault();
                    if (unitToMod != null)
                    {
                        //Log(CombineStrings("Modding ", unitToMod.displayName));
                        //Log(CombineStrings("item.Health ", ModdedData.Health.ToString()));
                        unitToMod.Health = ModdedData.Health;
                        unitToMod.MinAttackDamage = ModdedData.MinAttackDamage;
                        unitToMod.MaxAttackDamage = ModdedData.MaxAttackDamage;
                        unitToMod.speed = ModdedData.speed;
                        unitToMod.prodCost = ModdedData.prodCost;
                        //dynamic income/tick/cost calculation
                        for (int i = 0; i < unitToMod.prodIntervalCount.Count; i++)
                        {
                            unitToMod.prodIntervalCount[i] = ModdedData.prodTimeCost; //base ticks
                            unitToMod.prodIntervalCost[i] = ModdedData.prodCost / ModdedData.prodTimeCost; //calculate tick cost
                            if (i > 0)
                            {
                                for (int j = 0; j < i; j++)
                                {
                                    unitToMod.prodIntervalCount[i] = (int)(unitToMod.prodIntervalCount[i] * 0.8f); //each factory applies 20% reduction: 100, 80, 64, 51, 41
                                    unitToMod.prodIntervalCost[i] = Mathf.CeilToInt(unitToMod.prodIntervalCost[i] * 1.25f);
                                }
                            }
                        }
                        unitToMod.AttackSpeed = ModdedData.AttackSpeed;
                        unitToMod.AttackSpread = ModdedData.AttackSpread;
                        unitToMod.AttackRate = ModdedData.AttackRate;
                        unitToMod.AttackRange = ModdedData.AttackRange;
                        unitToMod.ArmorClass = (DyingBreed.Enums.ArmorClass) Enum.Parse(typeof(DyingBreed.Enums.ArmorClass), ModdedData.ArmorClass);
                        unitToMod.AttackDamageType = (DyingBreed.Enums.AttackDamageType)Enum.Parse(typeof(DyingBreed.Enums.AttackDamageType), ModdedData.AttackDamageType);
                    }
                }
                dataInjected = true;
            }

            public static void Log(string logString, int level = 1)
            {
                if (level == 1)
                {
                    Instance.plugin.Log.LogInfo(logString);
                }
                else if (level == 2)
                {
                    Instance.plugin.Log.LogWarning(logString);
                }
                else if (level == 3)
                {
                    Instance.plugin.Log.LogError(logString);
                }
            }
        }

    }
}
