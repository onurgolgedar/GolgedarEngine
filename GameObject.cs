using SFML.Graphics;
using SFML.System;

namespace GolgedarEngine
{
   public abstract class GameObject
   {
      public GameObject(string imageName = "", bool canCollide = true)
      {
         Sprite = imageName == "" ? new Sprite(new Texture(new Image(50, 50))) : new Sprite(Global.GetSprite(imageName));
         Initialization(canCollide);
      }

      public abstract void Draw();
      public abstract void Loop();

      public virtual void Collision(GameObject gameObject)
      {
      }
      public void Move(Vector2f displacement)
      {
         Position += new Vector2f(displacement.X, displacement.Y) / 1000 * Game.DeltaTime;
      }
      public void DrawSelf()
      {
         Game.Window.Draw(Sprite);
         Game.Window.Draw(CollisionMask);
      }
      public void SetCollisionMask(CollisionMask collisionMask, Alignment alignment = Alignment.CENTER)
      {
         CollisionMask = collisionMask;
         collisionMask.Attach(this, alignment);
      }

      private void Initialization(bool canCollide)
      {
         Game = Game.ActiveGame;

         CollisionMask = new CollisionMask(canCollide ? new Vector2f(Sprite.Texture.Size.X, Sprite.Texture.Size.Y) : Vector.Zero);
         SetCollisionMask(CollisionMask);
      }
      public int CompareTo(GameObject obj)
      {
         return Depth > obj.Depth ? 1 : 0;
      }

      public Vector2f Position
      {
         get { return Sprite.Position; }
         set
         {
            Sprite.Position = value;
            CollisionMask.Position = value;
         }
      }
      public int Depth { get; set; } = 0;
      public Sprite Sprite { get; set; }
      public CollisionMask CollisionMask { get; internal set; }

      public Game Game { get; internal set; }
   }
}