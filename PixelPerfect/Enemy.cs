using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
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
        protected bool teleport = false;
        protected int blinkTime = Config.Enemy.DEFAULT_BLINK_TIME_MS;
        protected int delayTime = 0;
        protected int offset = 0;
        protected double currentBlinkTime = 0.0;
        protected double currentDelayTime = 0.0;
        protected Color[] textureArray;

        private TimeSpan waitTimer = TimeSpan.Zero;
        private TimeSpan delayTimer = TimeSpan.Zero;
        private int waitTime = 0;
        private bool waiting = false;
        private bool started = true;

        private readonly Vector2 startPosition;
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
                return new Rectangle(textureColumn * (int)textureSize.X, animation.currentFrame * (int)textureSize.Y, (int)textureSize.X, (int)textureSize.Y);
            }
        }

        public Enemy(string textureName, Vector2 speed, Vector2 textureSize, int textureColumn, Vector2 startPosition, int animationDelay, bool reverse = true, bool blink = false, bool guardian = false, int offset = 0, int waitTime = 0, bool teleport = false, bool animationreverse = true, int frames = 4)
        {
            this.startPosition = startPosition;
            this.texture = Globals.spritesDictionary[textureName].texture;
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
            this.teleport = teleport;

            textureArray = Globals.spritesDictionary[textureName].textureArray[textureColumn];//Util.GetTextureArray(Util.BlitTexture(texture, new Rectangle((int)(textureColumn * textureSize.X), 0, (int)textureSize.X, (int)(Config.ANIM_FRAMES * textureSize.Y))), (int)textureSize.X, (int)(Config.ANIM_FRAMES * textureSize.Y));

            if (teleport)
                reverse = false;

            animation = new Animation(frames, animationDelay, animationreverse);
        }

        public void SetBlinkTeleportTime(int blinkTime)
        {
            if (blinkTime >= 0)
                this.blinkTime = blinkTime;
        }

        public void SetDelay(int delayTime)
        {
            this.delayTime = delayTime;
        }

        public void PrepareDelay()
        {
            if (delayTime > 0)
            {
                started = false;
            }
            else
            {
                started = true;
                Init();
            }
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
            NextPath(new GameTime());
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
            if (!started)
            {
                currentDelayTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if ((int)currentDelayTime >= delayTime)
                {
                    started = true;
                    Init();                    
                }
                return;
            }

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
                    NextPath(gameTime);
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
                NextPath(gameTime);
                currentBlinkTime = 0.0;
                currentPosition = targetPosition;
            }
        }

        protected void NextPath(GameTime gameTime)
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
                {
                    if (!teleport)
                        currentPath = 0;
                    else
                    {
                        currentPath--;
                        currentBlinkTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (currentBlinkTime >= blinkTime)
                        {
                            currentBlinkTime = 0.0;
                            currentPosition = movepointsList[0];
                            currentPath = 1;
                        }
                    }
                }
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
            if (!started)
                return;

            spriteBatch.Draw(texture, new Vector2(currentPosition.X + Config.DRAW_OFFSET_X + offset.X, currentPosition.Y + Config.DRAW_OFFSET_Y + offset.Y),
                                                sourceRectangle, Globals.enemiesColor, 0.0f, Vector2.Zero,
												1.0f, (leftDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }

        public Color[] GetCurrentFrameArray()
        {            
            int frameSizeInArray = (int)(textureSize.X * textureSize.Y);
            Color[] currentFrameArray = new Color[frameSizeInArray];

            if (leftDirection)
            {
                for (int i = 0; i < frameSizeInArray; i++)
                    currentFrameArray[i] = textureArray[i + (int)textureSize.X - 1 - 2 * (i % (int)textureSize.X)];
            }
            else
            {
                Array.Copy(textureArray, frameSizeInArray * animation.currentFrame, currentFrameArray, 0, frameSizeInArray);
            }
            
            return currentFrameArray;
        }

        public void AddMovepoint(Vector2 movePoint)
        {
            movepointsList.Add(movePoint);
        }

        public void Prepare()
        {
            AdjustSpeed();
            Reset();
        }

        public void Reset()
        {
            if (guardPosition != Vector2.Zero && guardian)
                movepointsList.Add(guardPosition);           
            currentPath = 0;
            currentBlinkTime = 0;
            currentDelayTime = 0;
            waitTimer = TimeSpan.Zero;
            waiting = false;
            goingBack = false;
            onGuard = false;
            animation.Reset();
            currentPosition = targetPosition = startPosition;
            PrepareDelay();
        }

        public void PixelExplosion()
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 boomCenter = currentPosition + new Vector2(textureSize.X / 2, textureSize.Y / 2);
                Vector2 pixPos = currentPosition + new Vector2(i % textureSize.X, i / textureSize.X);
                //pixPos.Y -= 4;
                Vector2 pixSpeed = (pixPos - boomCenter) * Globals.rnd.Next(0, Config.PixelParticle.MAX_EXPLOSION_MAGNITUDE);
                Vector2 acc = new Vector2(Globals.rnd.Next(-1000, 1000), Globals.rnd.Next(-1000, 1000));

                Globals.CurrentLevelState.AddPixelParticle(new PixelParticle(pixPos,
                                0.0f,//Config.PixelParticle.PIXELPARTICLE_PLAYER_LIFETIME_MAX,
                                pixSpeed, acc, Globals.enemiesColor, true, Globals.CurrentMap));
            }
        }
    }
}
