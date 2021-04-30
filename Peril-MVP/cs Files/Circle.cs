using System;
using Microsoft.Xna.Framework;

namespace Peril_MVP
{
    // Representation of a 2D Circle
    struct Circle
    {
        #region Declarations
        public Vector2 Center;
        public float Radius;
        #endregion

        #region Constructor
        public Circle(Vector2 position, float radius)
        {
            Center = position;
            Radius = radius;
        }
        #endregion

        public bool Intersects(Rectangle rectangle) // Determines if a circle intersects a rectangle.
        {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right),
                                    MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            Vector2 direction = Center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius)); // True if the circle and rectangle overlap. Otherwise returns False.
        }
    }
}
