﻿using HitScoreRumbler.Configuration;
using HitScoreRumbler.HarmonyPatches;
using Libraries.HM.HMLib.VR;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace HitScoreRumbler.Utils
{
    public static class Helper
    {
        private static readonly HapticPresetSO normalPreset = ScriptableObject.CreateInstance<HapticPresetSO>();

        public static float GetStrength(List<PointF> points, float strength, float distance)
        {
            if (points.Count == 0)
                return strength;

            if (points.Count == 1)
                return points[0].Y * strength;

            PointF start = points.FirstOrDefault();
            PointF end = points.LastOrDefault();

            if (distance <= 0.28f * start.X + 0.01)
                return start.Y * strength;
            if (distance >= 0.28f * end.X + 0.01)
                return end.Y * strength;

            int i = 0;
            foreach (var p in points)
            {
                float d = p.X * 0.28f + 0.01f;
                if (distance < d)
                {
                    end = p;
                    if (i != 0)
                        start = points[i - 1];

                    break;
                }
                i++;
            }

            float startDistance = start.X * 0.28f + 0.01f;
            float endDistance = end.X * 0.28f + 0.01f;
            float t = (distance - startDistance) / (endDistance - startDistance);

            return Mathf.Lerp(start.Y, end.Y, t) * strength;
        }

        public static HapticPresetSO GetHapticPreset(float distance)
        {
            normalPreset._duration = 0.14f * GetStrength(PluginConfig.Instance.LoadedPreset.PointsDuration, PluginConfig.Instance.LoadedPreset.DurationMultiplier, distance);
            normalPreset._strength = GetStrength(PluginConfig.Instance.LoadedPreset.Points, PluginConfig.Instance.LoadedPreset.StrengthMultiplier, distance);
            normalPreset._frequency = GetStrength(PluginConfig.Instance.LoadedPreset.PointsFrequency, PluginConfig.Instance.LoadedPreset.Frequency, distance);
        
            return normalPreset;
        }
    }
}
