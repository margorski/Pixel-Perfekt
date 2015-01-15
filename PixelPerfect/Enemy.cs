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
    class Enemy
    {
        public Texture2D texture { get; private set; }
        Animation animation;
        Vector2 currentPosition;
        Vector2 targetPosition;
        Vector2 speed;
        Vector2 startPosition;
        Vector2 endPosition;
        Vector2 textureSize;
        
        int textureColumn;

        // debug
        public float x_move = 0;

        public bool leftDirection
        {
            get
            {
                return speed.X < 0;
            }
        }

        public Rectangle boundingBox 
        { 
            get 
            {
                return new Rectangle((int)currentPosition.X, (int)currentPosition.Y, (int)textureSize.X, (int)textureSize.Y);
            }
        }

        private Rectangle sourceRectangle
        {
            get
            {
                return new Rectangle(textureColumn * (int)textureSize.X, animation.GetCurrentFrame() * (int)textureSize.Y, (int)textureSize.X, (int)textureSize.Y);
            }
        }

        public Enemy(Texture2D texture, Vector2 startPosition, Vector2 endPosition, Vector2 speed, Vector2 textureSize, int textureColumn)
        {
            this.texture = texture;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.speed = speed;
            this.textureSize = textureSize;
            this.textureColumn = textureColumn;
            this.currentPosition = startPosition;
            this.targetPosition = endPosition;

            animation = new Animation(4, (int)(5000 - speed.Length() * Config.ANIMATION_SPEED_FACTOR) / 2, false);
 
            AdjustSpeed();
        }

        private void AdjustSpeed()
        {
            Vector2 tempSpeed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));

            if (startPosition.X > endPosition.X)
                tempSpeed.X *= -1;

            if (startPosition.Y > endPosition.Y)
                tempSpeed.Y *= -1;

            speed = tempSpeed;
        }

        public void Update(GameTime gameTime)
        {
            currentPosition.X += (float)(gameTime.ElapsedGameTime.TotalSeconds * speed.X);
            currentPosition.Y += (float)(gameTime.ElapsedGameTime.TotalSeconds * speed.Y);

            animation.Update(gameTime);

            if (speed.X > 0)
            {
                if (currentPosition.X > targetPosition.X)
                    currentPosition.X = targetPosition.X;
            }
            else
            {
                if (currentPosition.X < targetPosition.X)
                    currentPosition.X = targetPosition.X;
            }

            if (speed.Y > 0)
            {
                if (currentPosition.Y > targetPosition.Y)
                    currentPosition.Y = targetPosition.Y;
            }
            else
            {
                if (currentPosition.Y < targetPosition.Y)
                    currentPosition.Y = targetPosition.Y;
            }

            if (currentPosition.Y == targetPosition.Y && currentPosition.X == targetPosition.X)
                Bounce();
        }

        private void Bounce()
        {
            if (targetPosition == endPosition)
                targetPosition = startPosition;
            else
                targetPosition = endPosition;

            speed *= -1;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(currentPosition.X + Config.DRAW_OFFSET_X, currentPosition.Y + Config.DRAW_OFFSET_Y), 
												sourceRectangle, Color.White, 0.0f, Vector2.Zero,
												1.0f, (leftDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }

        public Texture2D GetCurrentFrameTexture(GraphicsDeviceManager graphic)
        {
            return Util.BlitTexture(graphic, texture, sourceRectangle, leftDirection);
        }
    }
}
