using System;

namespace MdsBuildTools.Core
{
    /// <summary>Pure grid/angle snapping math. No Unity dependency so it is unit-testable.</summary>
    public static class SnapMath
    {
        /// <summary>Rounds <paramref name="value"/> to the nearest multiple of <paramref name="step"/>, offset by <paramref name="origin"/>.</summary>
        public static float Snap(float value, float step, float origin = 0f)
        {
            if (step <= 0f || float.IsNaN(step) || float.IsInfinity(step))
                return value;

            return (float)(Math.Round((value - origin) / step, MidpointRounding.AwayFromZero) * step + origin);
        }

        /// <summary>Snaps an angle in degrees to <paramref name="stepDegrees"/>, normalised to [0, 360).</summary>
        public static float SnapAngle(float degrees, float stepDegrees)
        {
            var snapped = Snap(degrees, stepDegrees);
            var normalised = snapped % 360f;
            if (normalised < 0f) normalised += 360f;
            // Guard against -0 / 360 float noise.
            return Math.Abs(normalised - 360f) < 1e-4f ? 0f : normalised;
        }
    }
}
