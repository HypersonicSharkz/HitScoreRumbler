using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HitScoreRumbler.Utils
{
    public static class Helper
    {
        public static float GetStrength(List<Vector2> points, float strength, float distance)
        {
            Vector2 start = points.FirstOrDefault();
            Vector2 end = points.LastOrDefault();

            if (points.Count < 2)
                return start.y * strength;

            if (distance <= 0.28f * start.x + 0.01)
                return start.y * strength;
            if (distance >= 0.28f * end.x + 0.01)
                return end.y * strength;

            int i = 0;
            foreach (var p in points)
            {
                float d = p.x * 0.28f + 0.01f;
                if (distance < d)
                {
                    end = p;
                    if (i != 0)
                        start = points[i - 1];

                    break;
                }
                i++;
            }

            float startDistance = start.x * 0.28f + 0.01f;
            float endDistance = end.x * 0.28f + 0.01f;
            float t = (distance - startDistance) / (endDistance - startDistance);

            return Mathf.Lerp(start.y, end.y, t) * strength;
        }
    }
}
