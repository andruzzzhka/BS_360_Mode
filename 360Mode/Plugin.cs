using System;
using System.Linq;
using System.Reflection;
using BS_Utils;
using BS_Utils.Gameplay;
using Harmony;
using IPA;
using IPA.Logging;
using SongCore;
using SongCore.Data;
using UnityEngine.SceneManagement;

namespace _360Mode
{
    public class Plugin : IBeatSaberPlugin
    {
        public static Logger log;

        internal static HarmonyInstance harmony;
        internal static bool patched = false;
        internal static bool active;

        public void Init(object nullObject, Logger logger)
        {
            log = logger;
        }

        private void CheckActivation()
        {
            IDifficultyBeatmap difficultyBeatmap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;
            ExtraSongData.DifficultyData difficultyData = Collections.RetrieveDifficultyData(difficultyBeatmap);

            if (difficultyData != null)
            {
                if (Config.swingMode)
                {
                    active = true;
                    ExtraBeatmapData.ParseExtraDataForSong();
                    ScoreSubmission.DisableSubmission("360 Mode");
                }
                else
                {
                    if (difficultyData.additionalDifficultyData._requirements.Contains("360 Mode"))
                    {
                        active = true;
                        ExtraBeatmapData.ParseExtraDataForSong();
                    }
                    else
                    {
                        active = false;
                    }
                }
            }
            else
            {
                active = false;
            }
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "MenuCore")
            {
                active = false;
            }
            if (nextScene.name == "GameCore")
            {
                CheckActivation();
            }
        }

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
            Config.LoadConfig();
            Collections.RegisterCapability("360 Mode");
            harmony = HarmonyInstance.Create("com.andruzzzhka.360Mode");
            ApplyPatches();
        }

        internal static void ApplyPatches()
        {
            try
            {
                if (!patched)
                {
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                    patched = true;
                }
            }
            catch (Exception arg)
            {
                log.Critical($"Unable to patch assembly! Exception: {arg}");
            }
        }

        public void OnFixedUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        public void OnUpdate()
        {
        }

    }
}
