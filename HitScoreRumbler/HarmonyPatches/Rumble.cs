using HarmonyLib;
using HitScoreRumbler.Configuration;
using Libraries.HM.HMLib.VR;
using System.Linq;
using UnityEngine;

namespace HitScoreRumbler.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteCutCoreEffectsSpawner), nameof(NoteCutCoreEffectsSpawner.HandleNoteWasCut))]
    internal class CutEffect
    {
        public static float distanceToCenter = 0;

        static void Prefix(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteController.noteData.gameplayType == NoteData.GameplayType.Normal)
            {
                if (noteCutInfo.allIsOK)
                {
                    distanceToCenter = noteCutInfo.cutDistanceToCenter;
                }
            }
        }
    }

    [HarmonyPatch(typeof(NoteCutHapticEffect), nameof(NoteCutHapticEffect.HitNote))]
    internal class Rumble
    {
        public static readonly HapticPresetSO normalPreset = ScriptableObject.CreateInstance<HapticPresetSO>();

        static bool Prefix(HapticFeedbackController ____hapticFeedbackController, SaberType saberType, NoteCutHapticEffect.Type type)
        {
            if (!PluginConfig.Instance.Enabled)
                return true;

            normalPreset._duration = 0.14f * PluginConfig.Instance.DurationMultiplier;
            normalPreset._strength = GetStrength();
            CutEffect.distanceToCenter = 0;

            ____hapticFeedbackController.PlayHapticFeedback(saberType.Node(), normalPreset);

            return false;
        }

        private static float GetStrength()
        {
            Vector2 start = PluginConfig.Instance.Points.FirstOrDefault();
            Vector2 end = PluginConfig.Instance.Points.LastOrDefault();

            if (PluginConfig.Instance.Points.Count > 1)
            {
                if (CutEffect.distanceToCenter <= start.x * 0.28f)
                    return start.y * PluginConfig.Instance.StrengthMultiplier;
                else if (CutEffect.distanceToCenter >= end.x * 0.28f)
                    return end.y * PluginConfig.Instance.StrengthMultiplier;
                else
                {
                    int i = 0;
                    foreach (var p in PluginConfig.Instance.Points)
                    {
                        float d = p.x * 0.28f;
                        if (CutEffect.distanceToCenter < d)
                        {
                            end = p;
                            if (i != 0)
                                start = PluginConfig.Instance.Points[i - 1];

                            break;
                        }
                        i++;
                    }

                    float t = (CutEffect.distanceToCenter - (start.x * 0.28f)) / ((end.x * 0.28f) - (start.x * 0.28f));

                    return Mathf.Lerp(start.y, end.y, t) * PluginConfig.Instance.StrengthMultiplier;
                }
            }
            else
            {
                return start.y * PluginConfig.Instance.StrengthMultiplier;
            }
        }
    }
}
