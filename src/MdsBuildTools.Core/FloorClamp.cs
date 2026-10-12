namespace MdsBuildTools.Core
{
    /// <summary>
    /// Keeps placed objects from sinking through the floor: if the object's lowest point is below
    /// the floor surface, returns the upward correction needed to rest it on the surface.
    /// </summary>
    public static class FloorClamp
    {
        /// <param name="boundsMinY">World-space Y of the object's lowest point (renderer/collider bounds.min.y).</param>
        /// <param name="floorY">World-space Y of the floor surface under the object.</param>
        /// <param name="tolerance">Penetration ignored (avoids jitter on objects already flush).</param>
        /// <returns>Vertical offset to add to the object's position (0 when no correction is needed).</returns>
        public static float Correction(float boundsMinY, float floorY, float tolerance = 0.001f)
        {
            var penetration = floorY - boundsMinY;
            return penetration > tolerance ? penetration : 0f;
        }
    }
}
