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
        
        protected Animation animation;
        protected Vector2 currentPosition;
        protected Vector2 targetPosition;
        protected Vector2 speed;
        protected Vector2 guardPosition;
        //Vector2 startPosition;
        //Vector2 endPosition;
        protected Vector2 textureSize;
        protected List<Vector2> movepointsList = new List<Vector2>();
        protected int currentPath = 0;
        protected int textureColumn;
        protected bool reverse;
        protected bool blink;
        protected bool guardian;
        protected bool goingBack = false;
        protected bool onGuard = false;
        protected int blinkTime = Config.Enemy.DEFAULT_BLINK_TIME_MS;
        protected int offset = 0;
        protected double currentBlinkTime = 0.0;

        private TimeSpan waitTimer = TimeSpan.Zero;
        private int waitTime = 0;
        private bool waiting = false;
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

        protected Rectangle sourceRectangle
        {
            get
            {
                return new Rectangle(textureColumn * (int)textureSize.X, animation.GetCurrentFrame() * (int)textureSize.Y, (int)textureSize.X, (int)textureSize.Y);
            }
        }

        public Enemy(Texture2D texture, Vector2 speed, Vector2 textureSize, int textureColumn, Vector2 startPosition, bool reverse = true, bool blink = false, bool guardian = false, int offset = 0, int waitTime = 0)
        {
            this.texture = texture;
            this.speed = speed;
            this.textureSize = textureSize;
            this.textureColumn = textureColumn;            
            this.currentPosition = this.targetPosition = startPosition;
            AddMovepoint(startPosition);
            this.reverse = reverse;
            this.blink = blink;
            this.guardian = guardian;
            this.offset = offset;
            this.waitTime = waitTime;
            animation = new Animation(4, (int)(Config.ENEMY_ANIMATION_SPEED_BASE - speed.Length() * Config.ENEMY_ANIMATION_SPEED_FACTOR), false);
 
            AdjustSpeed();
        }

        public void SetBlinkTime(int blinkTime)
        {
            if (blinkTime > 0)
                this.blinkTime = blinkTime;
        }

        protected void SetOffset()
        {
            if (movepointsList.Count < 2)
                return;

            if (offset < 0 || offset > 100)
                return;

            this.currentPosition += (movepointsList[1] - currentPosition) * (offset / 100.0f);
        }

        protected void PrepareGuardian()
        {            
            if (!guardian)
                return;

            guardPosition = movepointsList.Last();
            movepointsList.RemoveAt(movepointsList.Count - 1);
        }

        public void Init()
        {
            PrepareGuardian();
            SetOffset();
            NextPath();
        }

        public void TriggerGuardian()
        {
            if (!guardian)
                return;

            onGuard = true;
            targetPosition = guardPosition;
            AdjustSpeed();
        }

        protected void AdjustSpeed()
        {
            Vector2 tempSpeed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));

            if (currentPosition.X > targetPosition.X)
                tempSpeed.X *= -1;

            if (currentPosition.Y > targetPosition.Y)
                tempSpeed.Y *= -1;

            speed = tempSpeed;
        }

        public void Update(GameTime gameTime)
        {
            if (blink)
                UpdateBlink(gameTime);
            else
                UpdateNormal(gameTime);
        }

        protected void UpdateNormal(GameTime gameTime)
        {
            if (waiting)
            {
                waitTimer += gameTime.ElapsedGameTime;
                if (waitTimer.TotalMilliseconds >= waitTime)
                {
                    waiting = false;
                    waitTimer = TimeSpan.Zero;
                }
            }
            else
            {
                if (currentPosition.Y == targetPosition.Y && currentPosition.X == targetPosition.X && !onGuard)
                {
                    NextPath();
                    waiting = true;
                }

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
            }
        }

        protected void UpdateBlink(GameTime gameTime)
        {
            currentBlinkTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (currentBlinkTime >= blinkTime)
            {                
                NextPath();
                currentBlinkTime = 0.0;
                currentPosition = targetPosition;
            }
        }

        protected void NextPath()
        {
            if (onGuard)
                return;

            if (reverse)
            {
                if (goingBack)
                {
                    if (--currentPath < 0)
                    {
                        goingBack = !goingBack;
                        currentPath = 1;
                    }
                }
                else
                {
                    if (++currentPath >= movepointsList.Count)
                    {
                        goingBack = !goingBack;
                        currentPath = movepointsList.Count - 2;
                    }
                }
            }
            else
            {
                if (++currentPath >= movepointsList.Count)
                    currentPath = 0;
            }
            SetTarget();
            AdjustSpeed();
        }

        protected void SetTarget()
        {
            if (currentPath > movepointsList.Count - 1 || currentPath < 0)
                return;

            targetPosition = movepointsList[currentPath];
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(texture, new Vector2(currentPosition.X + Config.DRAW_OFFSET_X + offset.X, currentPosition.Y + Config.DRAW_OFFSET_Y + offset.Y), 
												sourceRectangle, Color.White, 0.0f, Vector2.Zero,
												1.0f, (leftDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }

        public Texture2D GetCurrentFrameTexture(GraphicsDeviceManager graphic)
        {
            return Util.BlitTexture(graphic, texture, sourceRectangle, leftDirection);
        }

        public void AddMovepoint(Vector2 movePoint)
        {
            movepointsList.Add(movePoint);
        }
    }
}
