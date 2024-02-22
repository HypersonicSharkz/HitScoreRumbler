using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace HitScoreRumbler.Utils
{
    public static class Helper
    {
        public static float GetStrength(List<PointF> points, float strength, float distance)
        {
            PointF start = points.FirstOrDefault();
            PointF end = points.LastOrDefault();

            if (points.Count < 2)
                return start.Y * strength;

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
    }
}
