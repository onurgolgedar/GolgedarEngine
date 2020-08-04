using SFML.System;
using System;

namespace GolgedarEngine
{
   public static class Vector
   {
      public static Vector2f Zero { get; } = new Vector2f(0, 0);
      public static Vector2f Up { get; } = new Vector2f(0, -1);
      public static Vector2f Down { get; } = new Vector2f(0, 1);
      public static Vector2f Left { get; } = new Vector2f(-1, 0);
      public static Vector2f Right { get; } = new Vector2f(1, 0);

      public static Vector2f Create(float x, float y)
      {
         return new Vector2f(x, y);
      }
      public static Vector2f Create(float lenght, double angle)
      {
         return new Vector2f((float)(lenght * Math.Cos(angle)), (float)(lenght * Math.Sin(angle)));
      }

      public static double GetDirection(Vector2f vector)
      {
         return Global.ToDegrees(Math.Atan2(vector.Y, vector.X));
      }
   }
}