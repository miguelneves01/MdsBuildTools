using System;
using System.Collections.Generic;

namespace MdsBuildTools.Core
{
    /// <summary>
    /// 1-D alignment along one axis: given the moving object's key coordinates (min edge, centre, max edge)
    /// and the same coordinates of nearby objects/walls, finds the smallest shift that lines one up.
    /// Run once per horizontal axis (X and Z).
    /// </summary>
    public static class Alignment
    {
        public readonly struct Span
        {
            public Span(float min, float max) { Min = min; Max = max; }
            public float Min { get; }
            public float Max { get; }
            public float Center => (Min + Max) * 0.5f;
        }

        /// <returns>Shift to apply on this axis, or null when nothing is within <paramref name="threshold"/>.</returns>
        public static float? FindShift(Span moving, IEnumerable<Span> targets, float threshold)
        {
            float? best = null;
            var movingKeys = new[] { moving.Min, moving.Center, moving.Max };

            foreach (var target in targets)
            {
                var targetKeys = new[] { target.Min, target.Center, target.Max };
                foreach (var m in movingKeys)
                foreach (var t in targetKeys)
                {
                    var delta = t - m;
                    if (Math.Abs(delta) <= threshold && (best == null || Math.Abs(delta) < Math.Abs(best.Value)))
                        best = delta;
                }
            }

            return best;
        }
    }
}
