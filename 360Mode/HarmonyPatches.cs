using Harmony;
using IPA.Utilities;
using System;
using UnityEngine;
using Zenject;

namespace _360Mode
{
    #region General

    [HarmonyPatch(typeof(BeatmapObjectSpawnController), new Type[]
    {
        typeof(BeatmapObjectData)
    })]
    [HarmonyPatch("BeatmapObjectSpawnCallback")]
    internal class ObjectSpawnPatch
    {
        private static int lastSpawnedObjectId;

        private static bool Prefix(BeatmapObjectSpawnController __instance, BeatmapObjectData beatmapObjectData, ref bool ____disableSpawning, ref float ____moveDistance, ref float ____moveSpeed, ref float ____jumpDistance, ref float ____noteJumpMovementSpeed, ref float ____topObstaclePosY, ref float ____globalYJumpOffset, ref float ____verticalObstaclePosY, ref ObstacleController.Pool ____obstaclePool, ref float ____spawnAheadTime, ref float ____noteLinesDistance, ref Action<BeatmapObjectSpawnController, ObstacleController> ___obstacleDiStartMovementEvent, ref float ____topLinesZPosOffset, ref NoteController.Pool ____bombNotePool, ref NoteController.Pool ____noteAPool, ref NoteController.Pool ____noteBPool, ref int ____numberOfSpawnedBasicNotes, ref float ____firstBasicNoteTime, ref NoteController ____prevSpawnedNormalNoteController, ref bool ____disappearingArrows, ref bool ____ghostNotes, ref Action<BeatmapObjectSpawnController, BeatmapObjectData, float, float> ___beatmapObjectWasSpawnedEvent,
            ref float ____topObstacleHeight, ref float ____verticalObstacleHeight)
        {
            if (!Plugin.active)
            {
                return true;
            }

            float angleFloat;
            Quaternion angle = ExtraBeatmapData.beatmapObjectsAngles.TryGetValue(beatmapObjectData.id, out angleFloat) ? Quaternion.Euler(0f, angleFloat, 0f) : Quaternion.identity;

            if (____disableSpawning)
            {
                return false;
            }

            float num2 = ____moveDistance / ____moveSpeed;
            float num3 = ____jumpDistance / ____noteJumpMovementSpeed;

            if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Obstacle)
            {
                ObstacleData obstacleData = (ObstacleData)beatmapObjectData;
                Vector3 forward = __instance.transform.forward;
                Vector3 vector = __instance.transform.position;
                vector += forward * (____moveDistance + ____jumpDistance * 0.5f);
                Vector3 vector2 = vector - forward * ____moveDistance;
                Vector3 vector3 = vector - forward * (____moveDistance + ____jumpDistance);
                Vector3 noteOffset = __instance.GetNoteOffset(beatmapObjectData.lineIndex, NoteLineLayer.Base);
                noteOffset.y = ((obstacleData.obstacleType == ObstacleType.Top) ? (____topObstaclePosY + ____globalYJumpOffset) : ____verticalObstaclePosY);

                float height = (obstacleData.obstacleType == ObstacleType.Top) ? ____topObstacleHeight : ____verticalObstacleHeight;
                ObstacleController obstacleController = ____obstaclePool.Spawn();
                __instance.SetObstacleEventCallbacks(obstacleController);
                obstacleController.transform.SetPositionAndRotation(angle * (vector + noteOffset), angle);
                obstacleController.Init(obstacleData, angle * (vector + noteOffset), angle * (vector2 + noteOffset), angle * (vector3 + noteOffset), num2, num3, beatmapObjectData.time - ____spawnAheadTime, ____noteLinesDistance, height);

                ___obstacleDiStartMovementEvent?.Invoke(__instance, obstacleController);
            }
            else
            {
                NoteData noteData = (NoteData)beatmapObjectData;
                Vector3 forward2 = __instance.transform.forward;
                Vector3 vector4 = __instance.transform.position;
                vector4 += forward2 * (____moveDistance + ____jumpDistance * 0.5f);
                Vector3 vector5 = vector4 - forward2 * ____moveDistance;
                Vector3 vector6 = vector4 - forward2 * (____moveDistance + ____jumpDistance);

                if (noteData.noteLineLayer == NoteLineLayer.Top)
                {
                    vector6 += forward2 * ____topLinesZPosOffset * 2f;
                }
                Vector3 noteOffset2 = __instance.GetNoteOffset(noteData.lineIndex, noteData.startNoteLineLayer);
                float jumpGravity = __instance.JumpGravityForLineLayer(noteData.noteLineLayer, noteData.startNoteLineLayer);

                if (noteData.noteType == NoteType.Bomb)
                {
                    NoteController noteController = ____bombNotePool.Spawn();
                    __instance.SetNoteControllerEventCallbacks(noteController);
                    noteController.transform.SetPositionAndRotation(angle * (vector4 + noteOffset2), angle);
                    lastSpawnedObjectId = beatmapObjectData.id;
                    noteController.Init(noteData, angle * (vector4 + noteOffset2), angle * (vector5 + noteOffset2), angle * (vector6 + noteOffset2), num2, num3, noteData.time - ____spawnAheadTime, jumpGravity);
                }
                else
                {
                    if (noteData.noteType.IsBasicNote())
                    {
                        MemoryPool<NoteController> memoryPool = (noteData.noteType == NoteType.NoteA) ? ____noteAPool : ____noteBPool;

                        if (____numberOfSpawnedBasicNotes == 0)
                        {
                            ____firstBasicNoteTime = noteData.time;
                        }

                        bool isFirstNote = ____firstBasicNoteTime == noteData.time;

                        NoteController noteController2 = memoryPool.Spawn();
                        __instance.SetNoteControllerEventCallbacks(noteController2);
                        Vector3 noteOffset3 = __instance.GetNoteOffset(noteData.flipLineIndex, noteData.startNoteLineLayer);
                        noteController2.transform.SetPositionAndRotation(angle * (vector4 + noteOffset3), angle);
                        GameNoteController gameNoteController = noteController2 as GameNoteController;
                        lastSpawnedObjectId = beatmapObjectData.id;

                        if (gameNoteController != null)
                        {
                            gameNoteController.Init(noteData, angle * (vector4 + noteOffset3), angle * (vector5 + noteOffset3), angle * (vector6 + noteOffset2), num2, num3, noteData.time - ____spawnAheadTime, jumpGravity, ____disappearingArrows, ____ghostNotes && !isFirstNote);
                        }
                        else
                        {
                            noteController2.Init(noteData, angle * (vector4 + noteOffset3), angle * (vector5 + noteOffset3), angle * (vector6 + noteOffset2), num2, num3, noteData.time - ____spawnAheadTime, jumpGravity);
                        }
                        ____numberOfSpawnedBasicNotes++;

                        if (____prevSpawnedNormalNoteController != null)
                        {
                            float time = ____prevSpawnedNormalNoteController.noteData.time;
                            float time2 = noteController2.noteData.time;
                        }
                        ____prevSpawnedNormalNoteController = noteController2;
                    }
                }
            }
            ___beatmapObjectWasSpawnedEvent?.Invoke(__instance, beatmapObjectData, num2, num3);
            return false;
        }
    }

    #endregion

    #region Effects

    [HarmonyPatch(typeof(NoteCutEffectSpawner), new Type[]
    {
        typeof(Vector3),
        typeof(NoteController),
        typeof(NoteCutInfo)
    })]
    [HarmonyPatch("SpawnBombCutEffect")]
    internal class BombCutEffectSpawnerPatch
    {
        public static bool Prefix(MissedNoteEffectSpawner __instance, Vector3 pos, NoteController noteController, NoteCutInfo noteCutInfo, ref BombExplosionEffect ____bombExplosionEffect, ref FlyingSpriteSpawner ____failFlyingSpriteSpawner, ref ShockwaveEffect ____shockwaveEffect)
        {
            if (!Plugin.active)
            {
                return true;
            }

            FlyingSpriteSpawnerPatch.lastNoteRotation = noteController.transform.rotation;
            FlyingSpriteSpawnerPatch.lastNoteRotationSet = true;
            Vector3 pos2 = Quaternion.Inverse(FlyingSpriteSpawnerPatch.lastNoteRotation) * pos;
            ____failFlyingSpriteSpawner.SpawnFlyingSprite(pos2);
            ____bombExplosionEffect.SpawnExplosion(pos);
            Vector3 pos3 = pos;
            pos3.y = 0.01f;
            ____shockwaveEffect.SpawnShockwave(pos3);

            return false;
        }
    }

    [HarmonyPatch(typeof(DisappearingArrowController), new Type[] {})]
    [HarmonyPatch("HandleNoteMovementDidInit")]
    internal class DisappearingArrowInitPatch
    {
        public static bool Prefix(DisappearingArrowController __instance, ref NoteMovement ____noteMovement, ref float ____minDistance, ref float ____maxDistance, ref bool ____ghostNote, ref float ____disappearingGhostStart, ref float ____disappearingGhostEnd, ref float ____disappearingNormalStart, ref float ____disappearingNormalEnd)
        {
            if (!Plugin.active)
            {
                return true;
            }

            ____maxDistance = Mathf.Min(____noteMovement.moveEndPos.magnitude * 0.8f, ____ghostNote ? ____disappearingGhostStart : ____disappearingNormalStart);
            ____minDistance = Mathf.Min(____noteMovement.moveEndPos.magnitude * 0.5f, ____ghostNote ? ____disappearingGhostEnd : ____disappearingNormalEnd);

            return false;
        }
    }

    [HarmonyPatch(typeof(DisappearingArrowController), new Type[] {})]
    [HarmonyPatch("ManualUpdate")]
    internal class DisappearingArrowUpdatePatch
    {
        public static bool Prefix(DisappearingArrowController __instance, ref NoteMovement ____noteMovement, ref float ____minDistance, ref float ____maxDistance)
        {
            if (!Plugin.active)
            {
                return true;
            }

            PlayerController privateField = ReflectionUtil.GetPrivateField<PlayerController>(ReflectionUtil.GetPrivateField<NoteJump>(____noteMovement, "_jump"), "_playerController");
            float arrowTransparency = Mathf.Clamp01(((____noteMovement.position - privateField.headPos).magnitude - ____minDistance) / (____maxDistance - ____minDistance));
            __instance.SetArrowTransparency(arrowTransparency);

            return false;
        }
    }

    [HarmonyPatch(typeof(FlyingObjectEffect), new Type[] {})]
    [HarmonyPatch("Update")]
    internal class FlyingObjectEffectUpdatePatch
    {
        public static bool Prefix(FlyingObjectEffect __instance, ref bool ____initialized, ref float ____elapsedTime, ref float ____duration, ref Vector3 ____startPos, ref Vector3 ____targetPos, ref AnimationCurve ____moveAnimationCurve, ref bool ____shake, ref Quaternion ____rotation, ref float ____shakeFrequency, ref float ____shakeStrength, ref AnimationCurve ____shakeStrengthAnimationCurve, ref Action<FlyingObjectEffect> ___didFinishEvent)
        {
            if (!Plugin.active)
            {
                return true;
            }

            if (!____initialized)
            {
                __instance.enabled = false;
                return false;
            }

            if (____elapsedTime < ____duration)
            {
                float num = ____elapsedTime / ____duration;
                ReflectionUtil.InvokePrivateMethod(__instance, "ManualUpdate", new object[]
                {
                    num
                });
                __instance.transform.localPosition = Vector3.Lerp(____startPos, ____targetPos, ____moveAnimationCurve.Evaluate(num));

                if (____shake)
                {
                    Vector3 eulerAngles = __instance.transform.localRotation.eulerAngles;
                    ____rotation.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, Mathf.Sin(num * 3.14159274f * ____shakeFrequency) * ____shakeStrength * ____shakeStrengthAnimationCurve.Evaluate(num));
                    __instance.transform.localRotation = ____rotation;
                }
                ____elapsedTime += Time.deltaTime;
            }
            else
            {
                ___didFinishEvent?.Invoke(__instance);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FlyingScoreSpawner), new Type[]
    {
        typeof(NoteCutInfo),
        typeof(int),
        typeof(int),
        typeof(Vector3),
        typeof(Color)
    })]
    [HarmonyPatch("SpawnFlyingScore")]
    internal class FlyingScoreSpawnerPatch
    {
        public static Quaternion lastNoteRotation;
        public static bool lastNoteRotationSet;

        public static bool Prefix(FlyingScoreSpawner __instance, NoteCutInfo noteCutInfo, int noteLineIndex, int multiplier, Vector3 pos, Color color, ref float[,] ___lineSlotSpawnTimes, ref FlyingScoreEffect.Pool ____flyingScoreEffectPool)
        {
            if (!Plugin.active)
            {
                return true;
            }

            if (noteLineIndex >= ___lineSlotSpawnTimes.GetLength(0))
            {
                float[,] array = new float[noteLineIndex, 1];
                for (int i = 0; i < ___lineSlotSpawnTimes.GetLength(0); i++)
                {
                    for (int j = 0; j < 1; j++)
                    {
                        array[i, j] = ___lineSlotSpawnTimes[i, j];
                    }
                }
                ___lineSlotSpawnTimes = array;
            }

            int num = 0;

            while (num < 0 && ___lineSlotSpawnTimes[noteLineIndex, num] + 0.4f >= Time.timeSinceLevelLoad)
            {
                num++;
            }

            ___lineSlotSpawnTimes[noteLineIndex, num] = Time.timeSinceLevelLoad;
            FlyingScoreEffect flyingScoreEffect = ____flyingScoreEffectPool.Spawn();
            flyingScoreEffect.didFinishEvent += __instance.HandleFlyingScoreEffectDidFinish;
            Vector3 targetPos = Vector3.zero;

            if (lastNoteRotationSet)
            {
                flyingScoreEffect.transform.SetPositionAndRotation(lastNoteRotation * pos, lastNoteRotation);
                pos.z = 0f;
                pos.y = -0.24f;
                targetPos = lastNoteRotation * (pos + new Vector3(0f, -0.23f * num, 7.55f));
            }
            else
            {
                flyingScoreEffect.transform.SetPositionAndRotation(pos, Quaternion.identity);
                pos.z = 0f;
                pos.y = -0.24f;
                targetPos = pos + new Vector3(0f, -0.23f * num, 7.55f);
            }

            flyingScoreEffect.InitAndPresent(noteCutInfo, multiplier, 0.7f, targetPos, color);

            return false;
        }
    }

    [HarmonyPatch(typeof(FlyingSpriteSpawner), new Type[]
    {
        typeof(Vector3)
    })]
    [HarmonyPatch("SpawnFlyingSprite")]
    internal class FlyingSpriteSpawnerPatch
    {
        public static Quaternion lastNoteRotation;
        public static bool lastNoteRotationSet;

        public static bool Prefix(FlyingSpriteSpawner __instance, Vector3 pos, ref FlyingSpriteEffect.Pool ____flyingSpriteEffectPool, ref float ____xSpread, ref float ____targetYPos, ref float ____targetZPos, ref float ____duration, ref Sprite ____sprite, ref Material ____material, ref Color ____color, ref bool ____shake)
        {
            if (!Plugin.active)
            {
                return true;
            }

            FlyingSpriteEffect flyingSpriteEffect = ____flyingSpriteEffectPool.Spawn();
            flyingSpriteEffect.didFinishEvent += __instance.HandleFlyingSpriteEffectDidFinish;
            Vector3 targetPos = Vector3.zero;

            if (lastNoteRotationSet)
            {
                Quaternion quaternion = lastNoteRotation;
                lastNoteRotationSet = false;
                Vector3 vector = quaternion * pos;
                flyingSpriteEffect.transform.SetPositionAndRotation(vector, quaternion);
                targetPos = quaternion * new Vector3(pos.x, ____targetYPos, ____targetZPos);
            }
            else
            {
                flyingSpriteEffect.transform.SetPositionAndRotation(pos, Quaternion.identity);
                targetPos = new Vector3(pos.x, ____targetYPos, ____targetZPos);
            }

            flyingSpriteEffect.InitAndPresent(____duration, targetPos, ____sprite, ____material, ____color, ____shake);

            return false;
        }
    }

    [HarmonyPatch(typeof(MissedNoteEffectSpawner), new Type[]
    {
        typeof(BeatmapObjectSpawnController),
        typeof(NoteController)
    })]
    [HarmonyPatch("HandleNoteWasMissed")]
    internal class MissedNoteEffectSpawnerPatch
    {
        public static bool Prefix(MissedNoteEffectSpawner __instance, BeatmapObjectSpawnController noteSpawnController, NoteController noteController, ref FlyingSpriteSpawner ____missedNoteFlyingSpriteSpawner, ref float ____spawnPosZ)
        {
            if (!Plugin.active)
            {
                return true;
            }

            NoteData noteData = noteController.noteData;
            if (noteData.noteType == NoteType.NoteA || noteData.noteType == NoteType.NoteB)
            {
                FlyingSpriteSpawnerPatch.lastNoteRotation = noteController.transform.rotation;
                FlyingSpriteSpawnerPatch.lastNoteRotationSet = true;
                Vector3 pos = Quaternion.Inverse(FlyingSpriteSpawnerPatch.lastNoteRotation) * noteController.noteTransform.position;
                pos.z = ____spawnPosZ;
                ____missedNoteFlyingSpriteSpawner.SpawnFlyingSprite(pos);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(NoteCutEffectSpawner), new Type[]
    {
        typeof(Vector3),
        typeof(NoteController),
        typeof(NoteCutInfo)
    })]
    [HarmonyPatch("SpawnNoteCutEffect")]
    internal class NoteCutEffectSpawnerPatch
    {
        public static bool Prefix(Vector3 pos, NoteController noteController, NoteCutInfo noteCutInfo, ref ColorManager ____colorManager, ref NoteCutParticlesEffect ____noteCutParticlesEffect, ref bool ____spawnScores, ref ScoreController ____scoreController, ref FlyingScoreSpawner ____flyingScoreSpawner, ref ShockwaveEffect ____shockwaveEffect, ref FlyingSpriteSpawner ____failFlyingSpriteSpawner, ref NoteDebrisSpawner ____noteDebrisSpawner)
        {
            if (!Plugin.active)
            {
                return true;
            }

            if (noteCutInfo.allIsOK)
            {
                NoteData noteData = noteController.noteData;
                Color color = ____colorManager.ColorForNoteType(noteData.noteType).ColorWithAlpha(0.5f);
                ____noteCutParticlesEffect.SpawnParticles(pos, noteCutInfo.cutNormal, noteCutInfo.saberDir, color, 150, 50, Mathf.Clamp(noteData.timeToNextBasicNote, 0.4f, 1f), noteCutInfo.saberType);

                if (____spawnScores)
                {
                    int multiplierWithFever = ____scoreController.multiplierWithFever;
                    FlyingScoreSpawnerPatch.lastNoteRotation = noteController.transform.rotation;
                    FlyingScoreSpawnerPatch.lastNoteRotationSet = true;
                    Vector3 pos2 = Quaternion.Inverse(FlyingSpriteSpawnerPatch.lastNoteRotation) * pos;
                    ____flyingScoreSpawner.SpawnFlyingScore(noteCutInfo, noteData.lineIndex, multiplierWithFever, pos2, new Color(0.8f, 0.8f, 0.8f));
                }
                Vector3 pos3 = pos;
                pos3.y = 0.01f;
                ____shockwaveEffect.SpawnShockwave(pos3);
            }
            else
            {
                FlyingSpriteSpawnerPatch.lastNoteRotation = noteController.transform.rotation;
                FlyingSpriteSpawnerPatch.lastNoteRotationSet = true;
                Vector3 pos4 = Quaternion.Inverse(FlyingSpriteSpawnerPatch.lastNoteRotation) * pos;
                ____failFlyingSpriteSpawner.SpawnFlyingSprite(pos4);
            }
            ____noteDebrisSpawner.SpawnDebris(noteCutInfo, noteController);

            return false;
        }
    }

    #endregion

    #region Notes

    [HarmonyPatch(typeof(NoteFloorMovement), new Type[] {})]
    [HarmonyPatch("SetToStart")]
    internal class NoteFloorMovementStartPatch
    {
        public static bool Prefix(NoteFloorMovement __instance, ref Vector3 __result, ref Vector3 ____localPosition, ref Vector3 ____startPos, ref Transform ____rotatedObject)
        {
            if (!Plugin.active)
            {
                return true;
            }

            ____localPosition = ____startPos;
            Vector3 vector = ____localPosition;
            __instance.transform.SetPositionAndRotation(vector, __instance.transform.rotation);
            ____rotatedObject.transform.rotation = __instance.transform.rotation;
            __result = vector;

            return false;
        }
    }

    [HarmonyPatch(typeof(NoteJump), new Type[]
    {
        typeof(Vector3),
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(NoteCutDirection)
    })]
    [HarmonyPatch("Init")]
    internal class NoteJumpInitPatch
    {
        public static void Postfix(NoteJump __instance, Vector3 startPos, Vector3 endPos, float jumpDuration, float startTime, float gravity, float flipYSide, NoteCutDirection cutDirection, ref Quaternion ____startRotation, ref Quaternion ____middleRotation, ref Quaternion ____endRotation, ref int ____randomRotationIdx, ref Vector3[] ____randomRotations)
        {
            if (!Plugin.active)
            {
                return;
            }

            Vector3 eulerAngles = ____endRotation.eulerAngles;
            eulerAngles.y = __instance.transform.rotation.eulerAngles.y;
            ____endRotation.eulerAngles = eulerAngles;
            Vector3 vector = ____endRotation.eulerAngles;
            ____randomRotationIdx = (____randomRotationIdx + Mathf.RoundToInt(Mathf.Abs(startPos.x) * 10f) + 1) % ____randomRotations.Length;
            vector += ____randomRotations[____randomRotationIdx] * 20f;
            ____middleRotation = Quaternion.Euler(vector);
            ____startRotation = __instance.transform.rotation;
        }
    }

    [HarmonyPatch(typeof(NoteJump), new Type[] {})]
    [HarmonyPatch("ManualUpdate")]
    internal class NoteJumpUpdatePatch
    {
        public static bool Prefix(NoteJump __instance, ref Vector3 __result, ref AudioTimeSyncController ____audioTimeSyncController, ref float ____startTime, ref float ____jumpDuration, ref Vector3 ____startPos, ref Vector3 ____endPos, ref Vector3 ____localPosition, ref PlayerController ____playerController, ref float ____startVerticalVelocity, ref float ____gravity, ref float ____yAvoidance, ref Quaternion ____startRotation, ref Quaternion ____middleRotation, ref Quaternion ____endRotation, ref Transform ____rotatedObject, ref float ____endDistanceOffest, ref bool ____halfJumpMarkReported, ref Action ___noteJumpDidPassHalfEvent, ref bool ____threeQuartersMarkReported, ref Action<NoteJump> ___noteJumpDidPassThreeQuartersEvent, ref float ____missedTime, ref bool ____missedMarkReported, ref Action ___noteJumpDidPassMissedMarkerEvent, ref Action ___noteJumpDidFinishEvent)
        {
            if (!Plugin.active)
            {
                return true;
            }

            float songTime = ____audioTimeSyncController.songTime;
            float num = songTime - ____startTime;
            float num2 = num / ____jumpDuration;

            ____localPosition = Vector3.LerpUnclamped(____startPos + __instance.transform.rotation * ____playerController.headPos * Mathf.Min(1f, num2 * 2f), ____endPos + __instance.transform.rotation * ____playerController.headPos, num2);
            ____localPosition.y = ____startPos.y + ____startVerticalVelocity * num - ____gravity * num * num * 0.5f;

            if (____yAvoidance != 0f && num2 < 0.25f)
            {
                float num3 = 0.5f - Mathf.Cos(num2 * 8f * 3.14159274f) * 0.5f;
                ____localPosition.y += num3 * ____yAvoidance;
            }

            if (num2 < 0.5f)
            {
                Quaternion quaternion;
                if (num2 < 0.125f)
                {
                    quaternion = Quaternion.Lerp(____startRotation, ____middleRotation, Mathf.Sin(num2 * 3.14159274f * 4f));
                }
                else
                {
                    quaternion = Quaternion.Lerp(____middleRotation, ____endRotation, Mathf.Sin((num2 - 0.125f) * 3.14159274f * 2f));
                }
                Vector3 headPos = ____playerController.headPos;
                headPos.y = Mathf.Lerp(headPos.y, ____localPosition.y, 0.8f);
                Vector3 normalized = (____localPosition - headPos).normalized;
                Quaternion quaternion2 = default(Quaternion);
                quaternion2.SetLookRotation(normalized, ____rotatedObject.up);
                ____rotatedObject.rotation = Quaternion.Lerp(quaternion, quaternion2, num2 * 2f);
            }

            if (num2 >= 0.5f && !____halfJumpMarkReported)
            {
                ____halfJumpMarkReported = true;
                ___noteJumpDidPassHalfEvent?.Invoke();
            }

            if (num2 >= 0.75f && !____threeQuartersMarkReported)
            {
                ____threeQuartersMarkReported = true;
                ___noteJumpDidPassThreeQuartersEvent?.Invoke(__instance);
            }

            if (songTime >= ____missedTime && !____missedMarkReported)
            {
                ____missedMarkReported = true;
                ___noteJumpDidPassMissedMarkerEvent?.Invoke();
            }

            if (____threeQuartersMarkReported)
            {
                float num4 = (num2 - 0.75f) / 0.25f;
                num4 = num4 * num4 * num4;
                ____localPosition -= Vector3.LerpUnclamped(Vector3.zero, __instance.transform.forward * ____endDistanceOffest, num4);
            }

            if (num2 >= 1f)
            {
                ___noteJumpDidFinishEvent?.Invoke();
            }

            Vector3 vector = ____localPosition;
            __instance.transform.position = ____localPosition;
            __result = vector;

            return false;
        }
    }

    [HarmonyPatch(typeof(NoteMovement), new Type[]
    {
        typeof(Vector3),
        typeof(Vector3),
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(NoteCutDirection)
    })]
    [HarmonyPatch("Init")]
    internal class NoteMovementInitPatch
    {
        private static bool Prefix(NoteMovement __instance, Vector3 moveStartPos, Vector3 moveEndPos, Vector3 jumpEndPos, float moveDuration, float jumpDuration, float startTime, float jumpGravity, float flipYSide, NoteCutDirection cutDirection, ref float ____zOffset, ref NoteFloorMovement ____floorMovement, ref Vector3 ____position, ref Vector3 ____prevPosition, ref NoteJump ____jump, ref Action ___didInitEvent)
        {
            if (!Plugin.active)
            {
                return true;
            }

            moveStartPos += __instance.transform.forward * ____zOffset;
            moveEndPos += __instance.transform.forward * ____zOffset;
            jumpEndPos += __instance.transform.forward * ____zOffset;

            ____floorMovement.Init(moveStartPos, moveEndPos, moveDuration, startTime);
            ____position = ____floorMovement.SetToStart();
            ____prevPosition = ____position;
            ____jump.Init(moveEndPos, jumpEndPos, jumpDuration, startTime + moveDuration, jumpGravity, flipYSide, cutDirection);
            ReflectionUtil.SetPrivateProperty(__instance, "movementPhase", NoteMovement.MovementPhase.MovingOnTheFloor);
            ___didInitEvent?.Invoke();

            return false;
        }
    }

    #endregion

    #region Obstacles

    [HarmonyPatch(typeof(ObstacleController), new Type[]
    {
        typeof(ObstacleData),
        typeof(Vector3),
        typeof(Vector3),
        typeof(Vector3),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float)
    })]
    [HarmonyPatch("Init")]
    internal class ObstacleInitPatch
    {
        public static bool Prefix(ObstacleController __instance, ObstacleData obstacleData, Vector3 startPos, Vector3 midPos, Vector3 endPos, float move1Duration, float move2Duration, float startTimeOffset, float singleLineWidth, float height, ref bool ____initialized, ref ObstacleData ____obstacleData, ref float ____obstacleDuration, ref Vector3 ____startPos, ref Vector3 ____midPos, ref Vector3 ____endPos, ref float ____move1Duration, ref float ____move2Duration, ref float ____startTimeOffset, ref StretchableObstacle ____stretchableObstacle, ref Bounds ____bounds, ref bool ____passedThreeQuartersOfMove2Reported, ref bool ____passedAvoidedMarkReported, ref float ____passedAvoidedMarkTime, ref float ____finishMovementTime, ref Action<ObstacleController> ___didInitEvent, ref SimpleColorSO ____color)
        {
            if (!Plugin.active)
            {
                return true;
            }

            ____initialized = true;
            ____obstacleData = obstacleData;
            ____obstacleDuration = obstacleData.duration;
            float num = obstacleData.width * singleLineWidth;
            Vector3 vector = __instance.transform.rotation * new Vector3((num - singleLineWidth) * 0.5f, 0f, 0f);
            ____startPos = startPos + vector;
            ____midPos = midPos + vector;
            ____midPos.y = ____startPos.y;
            ____endPos = endPos + vector;
            ____endPos.y = ____startPos.y;
            ____move1Duration = move1Duration;
            ____move2Duration = move2Duration;
            ____startTimeOffset = startTimeOffset;
            float length = (____endPos - ____midPos).magnitude / move2Duration * obstacleData.duration;
            ____stretchableObstacle.SetSizeAndColor(num * 0.98f, height, length, ____color);
            ____bounds = ____stretchableObstacle.bounds;
            ____passedThreeQuartersOfMove2Reported = false;
            ____passedAvoidedMarkReported = false;
            ____passedAvoidedMarkTime = ____move1Duration + ____move2Duration * 0.5f + ____obstacleDuration + 0.15f;
            ____finishMovementTime = ____move1Duration + ____move2Duration + ____obstacleDuration;
            ___didInitEvent?.Invoke(__instance);

            return false;
        }
    }

    [HarmonyPatch(typeof(ObstacleController), new Type[]
    {
        typeof(float)
    })]
    [HarmonyPatch("GetPosForTime")]
    internal class ObstaclePosForTimePatch
    {
        public static bool Prefix(ObstacleController __instance, ref Vector3 __result, float time, ref float ____move1Duration, ref float ____move2Duration, ref Vector3 ____startPos, ref Vector3 ____midPos, ref Vector3 ____endPos, ref bool ____passedAvoidedMarkReported, ref float ____passedAvoidedMarkTime, ref float ____finishMovementTime, ref float ____endDistanceOffest, ref PlayerController ____playerController, ref Bounds ____bounds)
        {
            if (!Plugin.active)
            {
                return true;
            }

            if (time < ____move1Duration)
            {
                __result = Vector3.LerpUnclamped(____startPos, ____midPos, time / ____move1Duration);
            }
            else
            {
                float num = (time - ____move1Duration) / ____move2Duration;
                __result = Vector3.LerpUnclamped(____midPos + __instance.transform.rotation * ____playerController.headPos * Mathf.Min(1f, num * 2f), ____endPos + __instance.transform.rotation * ____playerController.headPos, num);

                if (____passedAvoidedMarkReported)
                {
                    float num2 = (time - ____passedAvoidedMarkTime) / (____finishMovementTime - ____passedAvoidedMarkTime);
                    num2 = num2 * num2 * num2;
                    __result -= Vector3.LerpUnclamped(Vector3.zero, __instance.transform.forward * ____endDistanceOffest, num2);
                }
            }
            __result.y = ____startPos.y;

            return false;
        }
    }

    #endregion
}
