using SFML.Graphics;
using SFML.System;

namespace GolgedarEngine
{
   public abstract class GameObject
   {
      public GameObject(string imageName)
      {
         Initialization();
         Sprite = new Sprite(Global.GetSprite(imageName));
      }

      public GameObject()
      {
         Initialization();
         Sprite = new Sprite();
         if (true)
         {

         }
      }

      public abstract void Draw();
      public abstract void Loop();

      public void Move(Vector2f displacement)
      {
         Sprite.Position += new Vector2f(displacement.X, displacement.Y) / 1000 * Game.DeltaTime;
      }
      public void DrawSelf()
      {
         Game.Window.Draw(Sprite);
      }

      private void Initialization()
      {
         Game = Game.ActiveGame;
      }
      public int CompareTo(GameObject obj)
      {
         return Depth > obj.Depth ? 1 : 0;
      }

      public Vector2f Position { get { return Sprite.Position; } set { Sprite.Position = value; } }
      public int Depth { get; set; } = 0;
      public Sprite Sprite { get; set; }

      public Game Game { get; internal set; }
   }
}