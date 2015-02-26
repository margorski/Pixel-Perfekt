using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    enum MovementDirection
    {
        Left,
        Up,
        Right,
        Down
    }

  
    class Emiter
    {
        Texture2D texture;
        List<EmiterPart> emitedParts = new List<EmiterPart>();
        Vector2 startPosition;
        uint distance;
        int emitsDelayMs;
        float speed;
        Rectangle textureRectangle;
        Color color;
        TimeSpan lastEmit;
        MovementDirection movementDirection;

        public Emiter(Texture2D texture, Vector2 startPosition, uint distance, float speed, MovementDirection movementDirection, Rectangle textureRectangle, int emitsDelayMs, int delayOffsetMs, Color color)
        {
            this.texture = texture;
            this.startPosition = startPosition;
            this.distance = distance;
            this.textureRectangle = textureRectangle;
            this.emitsDelayMs = emitsDelayMs;
            this.color = color;
            this.movementDirection = movementDirection;
            this.speed = speed;
            lastEmit = TimeSpan.FromMilliseconds(delayOffsetMs);
        }

        public void Update(GameTime gameTime)
        {
            double deltaTimeSeconds = gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;

            lastEmit += gameTime.ElapsedGameTime;
            if (lastEmit.TotalMilliseconds > emitsDelayMs)
            {
                lastEmit = TimeSpan.Zero;
                EmitPart();
            }
            UpdateParts(gameTime);
        }

        public void UpdateParts(GameTime gameTime)
        {
            for (int i = 0; i < emitedParts.Count; i++)
            {
                if (emitedParts[i].Update(gameTime))
                {
                    emitedParts.RemoveAt(i);
                    i--; continue;
                }
            }
        }

        public void EmitPart()
        {
            emitedParts.Add(new EmiterPart(startPosition, distance, speed, movementDirection, texture, textureRectangle, color));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (EmiterPart emitedPart in emitedParts)
            {
                emitedPart.Draw(spriteBatch);
            }
        }

        public bool CheckCollision(Rectangle boundingBox, out EmiterPart collidingPart)
        {
            collidingPart = null;

            foreach (EmiterPart emiterPart in emitedParts)
            {
                if (emiterPart.Collide(boundingBox))
                {
                    collidingPart = emiterPart;
                    return true;
                }
            }
            return false;
        }
    }
}
