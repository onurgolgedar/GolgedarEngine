using SFML.System;

namespace GolgedarEngine
{
   public static class Vectors
   {
      public static Vector2f Up { get; } = new Vector2f(0, -1);
      public static Vector2f Down { get; } = new Vector2f(0, 1);
      public static Vector2f Left { get; } = new Vector2f(-1, 0);
      public static Vector2f Right { get; } = new Vector2f(1, 0);

      public static Vector2f Create(float x, float y)
      {
         return new Vector2f(x, y);
      }
   }
}