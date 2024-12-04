using HarmonyLib;
using HitScoreRumbler.Configuration;
using HitScoreRumbler.Utils;
using Libraries.HM.HMLib.VR;
using UnityEngine;

namespace HitScoreRumbler.HarmonyPatches
{
    [HarmonyPatch(typeof(NoteCutCoreEffectsSpawner), "HandleNoteWasCut")]
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
        static bool Prefix(HapticFeedbackController ____hapticFeedbackController, SaberType saberType, NoteCutHapticEffect.Type type)
        {
            if (!PluginConfig.Instance.Enabled)
                return true;

            ____hapticFeedbackController.PlayHapticFeedback(saberType.Node(), Helper.GetHapticPreset(CutEffect.distanceToCenter));
            CutEffect.distanceToCenter = 0;

            return false;
        }
    }
}
