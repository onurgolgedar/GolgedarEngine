using SFML.Graphics;
using SFML.System;
using System;

namespace GolgedarEngine
{
    public class CollisionMask : RectangleShape
    {
        public Vector2f Offset { get; private set; } = new Vector2f(0, 0);
        internal new Vector2f Position { get { return base.Position; } set { base.Position = value + Offset; } }

        public CollisionMask(Vector2f size) : base(size)
        {
            OutlineThickness = 4;
            FillColor = Color.Transparent;

            Color color = new Color(Color.Magenta) { A = 100 };
            OutlineColor = color;
        }

        internal void Align(GameObject gameObject, Alignment alignment, bool onOrigin = true)
        {
            switch (alignment)
            {
                case Alignment.TOP:
                    throw new NotImplementedException();
                case Alignment.TOP_LEFT:
                    Offset = new Vector2f(0, 0);
                    break;
                case Alignment.TOP_RIGHT:
                    throw new NotImplementedException();
                case Alignment.CENTER:
                    Offset = new Vector2f(-Size.X / 2f, -Size.Y / 2f);
                    break;
                case Alignment.CENTER_LEFT:
                    throw new NotImplementedException();
                case Alignment.CENTER_RIGHT:
                    throw new NotImplementedException();
                case Alignment.BOTTOM_LEFT:
                    throw new NotImplementedException();
                case Alignment.BOTTOM:
                    throw new NotImplementedException();
                case Alignment.BOTTOM_RIGHT:
                    throw new NotImplementedException();
            }
        }
        internal void Attach(GameObject gameObject, Alignment alignment, bool onOrigin = true)
        {
            Align(gameObject, alignment, onOrigin);
            Position = gameObject.Sprite.Position;

            if (onOrigin)
                Origin = gameObject.Sprite.Position;
        }
    }
}