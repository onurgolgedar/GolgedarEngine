using SFML.System;
using System;

namespace GolgedarEngine
{
   public static class Vector
   {
      public static Vector2f Up { get; } = new Vector2f(0, -1);
      public static Vector2f Down { get; } = new Vector2f(0, 1);
      public static Vector2f Left { get; } = new Vector2f(-1, 0);
      public static Vector2f Right { get; } = new Vector2f(1, 0);

      public static Vector2f Create(float x, float y)
      {
         return new Vector2f(x, y);
      }

      public static float GetDirection(Vector2f vector)
      {
         return Global.ToDegrees((float)Math.Atan2(vector.Y, vector.X));
      }
   }
}