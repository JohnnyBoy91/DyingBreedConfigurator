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
using UnityEngine.SceneManagement;
using UniverseLib;
using static Il2CppSystem.Globalization.CultureInfo;
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
                ModManager.Log("PlayerControllerInjected");
                ModManager.Instance.MainSetup();
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

            public string dataConfigPath;
            public string modSettingsPath;
            public string modRootPath;
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
                    Instance.plugin.Log.LogInfo("locking cursor");
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    Cursor.lockState = CursorLockMode.Confined;

                    modRootPath = Directory.GetCurrentDirectory() + @"\BepInEx\plugins\DyingBreedConfigurator\";
                    dataConfigPath = modRootPath + "ModDataConfig.txt";
                    modSettingsPath = modRootPath + "ModSettings.txt";
                    if (!Directory.Exists(modRootPath + generatedConfigFolderPath))
                    {
                        Directory.CreateDirectory(modRootPath + generatedConfigFolderPath);
                    }
                    initialized = true;
                }

                if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyUp(KeyCode.F10))
                {
                    Instance.plugin.Log.LogInfo("SendDevCMD");
                    MainSetup();
                }
            }

            internal void MainSetup()
            {
                if (dataInjected) return;

                foreach (var unitData in RuntimeHelper.FindObjectsOfTypeAll<UnitScriptableObject>())
                {
                    foreach (var unitDataB in unitData.unitData)
                    {
                        Log(CombineStrings('"'.ToString(), unitDataB.displayName, '"'.ToString(), ","));
                        unitSODataList.Add(unitDataB);
                        //unitDataB.Health = 8000;
                        //unitDataB.speed = 20;
                        //unitDataB.prodCost = 50;
                    }
                    //if (LocManager.legalUnitNames.Contains(unitData.name)) unitDataList.Add(unitData);
                    //if (unitData.name == "Bear" || unitData.name == "BearSlayer") unitDataList.Add(unitData);
                }
                List<UnitDataBlueprint> blueprints = new List<UnitDataBlueprint>();
                foreach (var item in unitSODataList)
                {
                    UnitDataBlueprint data = new UnitDataBlueprint();
                    data.key = item.displayName;
                    data.Health = item.Health;
                    data.MinAttackDamage = item.MinAttackDamage;
                    data.MaxAttackDamage = item.MaxAttackDamage;
                    data.speed = item.speed;
                    data.prodCost = item.prodCost;
                    data.AttackSpeed = item.AttackSpeed;
                    data.AttackSpread = item.AttackSpread;
                    data.AttackRate = item.AttackRate;
                    data.AttackRange = item.AttackRange;
                    data.faction = item.faction;
                    data.ArmorClass = item.ArmorClass;
                    data.AttackDamageType = item.AttackDamageType;
                    blueprints.Add(data);
                }

                WriteJsonConfig(CombineStrings(modRootPath, generatedConfigFolderPath, "DefaultUnitData.json"), blueprints);

                List<UnitDataBlueprint> blueprintsB = (List<UnitDataBlueprint>)ReadJsonConfig<List<UnitDataBlueprint>>(CombineStrings(modRootPath, "ModUnitData.json"));
                foreach (var item in blueprintsB)
                {
                    var unitToMod = unitSODataList.Where(x => x.displayName == item.key && x.faction == item.faction).FirstOrDefault();
                    if (unitToMod != null)
                    {
                        Log(CombineStrings("Modding ", unitToMod.displayName));
                        Log(CombineStrings("item.Health ", item.Health.ToString()));
                        unitToMod.Health = item.Health;
                        unitToMod.MinAttackDamage = item.MinAttackDamage;
                        unitToMod.MaxAttackDamage = item.MaxAttackDamage;
                        unitToMod.speed = item.speed;
                        unitToMod.prodCost = item.prodCost;
                        unitToMod.AttackSpeed = item.AttackSpeed;
                        unitToMod.AttackSpread = item.AttackSpread;
                        unitToMod.AttackRate = item.AttackRate;
                        unitToMod.AttackRange = item.AttackRange;
                        //unitToMod.faction = item.faction;
                        unitToMod.ArmorClass = item.ArmorClass;
                        unitToMod.AttackDamageType = item.AttackDamageType;
                    }
                }
                dataInjected = true;
            }

            public static string GetValue(string key)
            {
                string[] linesToCheck;
                linesToCheck = Instance.datalines;

                foreach (string line in linesToCheck)
                {
                    //ignore commented lines
                    if (!line.Contains("//"))
                    {
                        if (line.Split(':')[0].Contains(key))
                        {
                            string value = line.Split(':')[1].TrimEnd();
                            return value;
                        }
                    }
                }
                if (VerboseLogging()) Log(CombineStrings("Failed to find key: ", key));
                return null;
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
