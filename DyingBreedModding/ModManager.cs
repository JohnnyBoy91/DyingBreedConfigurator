using BepInEx;
using BepInEx.Unity.IL2CPP;
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
using static JCDyingBreedConfigurator.Utilities;
using HarmonyLib;
using UnityEngine.SceneManagement;
using UniverseLib;

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
                        unitDataB.Health = 8000;
                        unitDataB.speed = 20;
                    }
                    //if (LocManager.legalUnitNames.Contains(unitData.name)) unitDataList.Add(unitData);
                    //if (unitData.name == "Bear" || unitData.name == "BearSlayer") unitDataList.Add(unitData);
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
