using HarmonyLib;
using HitScoreRumbler.Configuration;
using HitScoreRumbler.Utils;
using Libraries.HM.HMLib.VR;
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

            normalPreset._duration = 0.14f * PluginConfig.Instance.LoadedPreset.DurationMultiplier;
            normalPreset._strength = Helper.GetStrength(PluginConfig.Instance.LoadedPreset.Points, PluginConfig.Instance.LoadedPreset.StrengthMultiplier, CutEffect.distanceToCenter);
            CutEffect.distanceToCenter = 0;

            ____hapticFeedbackController.PlayHapticFeedback(saberType.Node(), normalPreset);

            return false;
        }
    }
}
