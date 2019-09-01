using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BS_Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace _360Mode
{
    internal static class ExtraBeatmapData
    {
        internal static Dictionary<int, float> beatmapObjectsAngles = new Dictionary<int, float>();

        public static void ParseExtraDataForSong()
        {
            IDifficultyBeatmap diffBeatmap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;
            CustomPreviewBeatmapLevel customPreviewBeatmapLevel = diffBeatmap.level as CustomPreviewBeatmapLevel;

            if (customPreviewBeatmapLevel != null)
            {
                string customLevelPath = customPreviewBeatmapLevel.customLevelPath;
                string beatmapFilename = customPreviewBeatmapLevel.standardLevelInfoSaveData.difficultyBeatmapSets.First((StandardLevelInfoSaveData.DifficultyBeatmapSet x) => x.beatmapCharacteristicName == diffBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName).difficultyBeatmaps.First((StandardLevelInfoSaveData.DifficultyBeatmap x) => x.difficulty == diffBeatmap.difficulty.ToString()).beatmapFilename;
                string jsonString = File.ReadAllText(Path.Combine(customLevelPath, beatmapFilename));

                JObject json = JObject.Parse(jsonString);
                JArray notes = (JArray)json["_notes"];

                int objId = 0;

                beatmapObjectsAngles.Clear();

                foreach (JToken note in notes)
                {
                    JObject noteObject = (JObject)note;
                    try
                    {
                        if (noteObject != null)
                        {
                            if (Config.swingMode)
                            {
                                if (Config.endlessSwing)
                                {
                                    beatmapObjectsAngles.Add(objId, Mathf.Abs(Extensions.Value<float>(noteObject["_time"]) * Config.swingAngleIncPerSecond % 360f));
                                }
                                else
                                {
                                    beatmapObjectsAngles.Add(objId, Mathf.Abs(Config.swingMaxAngle - Extensions.Value<float>(noteObject["_time"]) * Config.swingAngleIncPerSecond % (Config.swingMaxAngle * 2f)) - Config.swingMaxAngle / 2f);

                                }
                            }
                            else
                            {
                                if (noteObject.ContainsKey("_angle"))
                                {
                                    beatmapObjectsAngles.Add(objId, Extensions.Value<float>(noteObject["_angle"]));
                                }
                            }
                        }
                    }
                    catch (Exception arg)
                    {
                        Plugin.log.Error("Unable to add entry to the dictionary! Exception: " + arg);
                    }
                    objId++;
                }

                JArray obstacles = (JArray)json["_obstacles"];

                foreach (JToken obstacle in obstacles)
                {
                    JObject obstacleObject = (JObject)obstacle;

                    if (obstacleObject != null)
                    {
                        if (Config.swingMode)
                        {
                            if (Config.endlessSwing)
                            {
                                beatmapObjectsAngles.Add(objId, Mathf.Abs(Extensions.Value<float>(obstacleObject["_time"]) * Config.swingAngleIncPerSecond % 360f));
                            }
                            else
                            {
                                beatmapObjectsAngles.Add(objId, Mathf.Abs(Config.swingMaxAngle - Extensions.Value<float>(obstacleObject["_time"]) * Config.swingAngleIncPerSecond % (Config.swingMaxAngle * 2f)) - Config.swingMaxAngle / 2f);
                            }
                        }
                        else
                        {
                            if (obstacleObject.ContainsKey("_angle"))
                            {
                                beatmapObjectsAngles.Add(objId, Extensions.Value<float>(obstacleObject["_angle"]));
                            }
                        }
                    }
                    objId++;
                }
                Plugin.log.Info($"Successfully parsed extra song data! Entries count: {beatmapObjectsAngles.Count}");
            }
        }
    }
}
