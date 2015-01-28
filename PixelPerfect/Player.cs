﻿using System;
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
    class Player
    {
        // state const
        public static class State 
        {
            public const UInt32 directionLeft = 1 << 0;
            public const UInt32 jumping = 1 << 1;
            public const UInt32 falling = 1 << 2;
            public const UInt32 stopped = 1 << 3;
            public const UInt32 dead = 1 << 4;
            public const UInt32 stoppedTemp = 1 << 5;
            public const UInt32 tryJump = 1 << 6;
            public const UInt32 onMovingPlatform = 1 << 7;
            public const UInt32 dying = 1 << 8;            
            public const UInt32 jumpStopped = 1 << 9;  // wtf hack for jumping from stopped position
        }
        // Public
        public Rectangle boundingBox
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, Config.Player.WIDTH, Config.Player.HEIGHT);
            }
        }
        public Vector2 speed;
        public Vector2 baseSpeed;
        public Vector2 enviroSpeed = Vector2.Zero;

        // Private
        private Animation animation;
        private Texture2D texture;
        private Vector2 position;
        private Vector2 acc;
        private UInt32 state;
        public float jumpY;
        private Color[] boomColors = { Color.White, Color.Red, Color.Blue, Color.LightSeaGreen, Color.OrangeRed, Color.Crimson, 
                                       Color.SpringGreen, Color.Teal, Color.RoyalBlue, Color.AntiqueWhite, Color.Chocolate, 
                                       Color.HotPink, Color.Honeydew, Color.PaleVioletRed, Color.SteelBlue, Color.Indigo,
                                       Color.Orange, Color.Yellow, Color.OldLace, Color.MediumPurple, Color.Azure, Color.Red};

        private TimeSpan blinkTime = TimeSpan.Zero;
        private TimeSpan tryJumpTime = TimeSpan.Zero;
        private TimeSpan stopTimeForReverse = TimeSpan.Zero;

        private Rectangle sourceRectangle
        {
            get
            {
                return new Rectangle(0, animation.GetCurrentFrame() * Config.Player.HEIGHT, Config.Player.WIDTH, Config.Player.HEIGHT);
            }
        }
        private GraphicsDeviceManager graphics;
        private Texture2D pixel;

        private int boomColorIndex = 0;
        private TimeSpan boomColorTime = TimeSpan.Zero;

        public Player(Vector2 position, Texture2D texture, GraphicsDeviceManager graphics, Texture2D pixel)
        {
            speed = baseSpeed = new Vector2(Config.Player.MOVE_SPEED, 0.0f);
            acc = new Vector2(0.0f, Config.Player.GRAVITY);
            this.position = position;
            this.texture = texture;
            animation = new Animation(4, Config.Player.ANIMATION_DELAY, false);
            this.graphics = graphics;
            this.pixel = pixel;
            state = 0x0;
            boomColorIndex = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (GetState(Player.State.dead))
                return;

            if (GetState(Player.State.dying))
            {
                boomColorTime += gameTime.ElapsedGameTime;
                if (boomColorTime.TotalMilliseconds > Config.Player.BOOMCOLOR_TIME_MS)
                {
                    boomColorTime = TimeSpan.Zero;
                    if (++boomColorIndex >= boomColors.Length)
                    {
                        PixelExplosion();
                        SetState(State.dying, false);
                        SetState(State.dead, true);
                    }
                }
                return;
            }

            if (!GetState(State.jumping) && !GetState(State.stopped))
                animation.Update(gameTime);

            if (GetState(Player.State.tryJump))
            {
                tryJumpTime += gameTime.ElapsedGameTime;
                if (tryJumpTime.TotalMilliseconds > Config.Player.TRYJUMP_RESETTIME)
                {
                    SetState(State.tryJump, false);
                    tryJumpTime = TimeSpan.Zero;
                }
            }

            float timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            speed.Y += Config.Player.GRAVITY * timeFactor;
            speed.Y = MathHelper.Clamp(speed.Y, Config.Player.JUMP_SPEED, Config.Player.MAX_FALL_SPEED);

            if ((GetState(State.stopped) && !GetState(State.jumping)) ||
                (GetState(State.jumping) && GetState(State.jumpStopped)))
                speed.X = 0.0f + enviroSpeed.X;
            //else if (GetState(State.jumping))
            //    speed.X += timeFactor * Config.Player.HTORQUE_INAIR * -Math.Sign(speed.X);
            else
                speed.X = (GetState(State.directionLeft) ? -1 : 1) * baseSpeed.X + enviroSpeed.X;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (GetState(Player.State.dead))
                return;

            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X, boundingBox.Y + Config.DRAW_OFFSET_Y, boundingBox.Width, boundingBox.Height),
                            sourceRectangle, boomColors[boomColorIndex], 0.0f, Vector2.Zero, 
							(GetState(State.directionLeft) ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }

        public void MoveHorizontally(GameTime gameTime)
        {
            if (GetState(Player.State.dead))
                return;

            if (((GetState(State.stopped) || GetState(State.stoppedTemp)) && 
                                                 !GetState(State.jumping) && 
                                                 !GetState(State.onMovingPlatform))) 
                //|| (GetState(State.stoppedTemp) && !GetState(State.onMovingPlatform)))
                return;

            if ((state & State.falling) > 0)
                return;

            float timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            position.X += speed.X * timeFactor;
        }

        public void MoveVertically(GameTime gameTime)
        {
            if (GetState(Player.State.dead))
                return;

            float timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            position.Y += speed.Y * timeFactor;

            if (position.Y > Config.SCREEN_HEIGHT_SCALED)
                SetState(Player.State.dead, true);

            if (GetState(State.jumping) && position.Y > jumpY)
                SetState(State.falling, true);
        }

        public void SetHorizontalPositionOnTile(Rectangle tileBox)
        {
            if (speed.X > 0.0f)
                position.X = (float)(tileBox.Left - Config.Player.WIDTH);
            else
                position.X = (float)tileBox.Right;
        }

        public void SetVerticalPositionOnTile(Rectangle tileBox)
        {
            if (speed.Y > 0.0f)
                position.Y = (float)(tileBox.Top - Config.Player.HEIGHT);
            else
                position.Y = (float)tileBox.Bottom;
        }

        public void SetState(UInt32 state, bool value) 
        {
            if (value)
                this.state |= state;
            else
                this.state &= (~state);
        }

        public bool GetState(UInt32 state) { return (this.state & state) == state; }

        public void SetSpeedY(float value) { speed.Y = value; }

        public void SetSpeedX(float value) { speed.X = value; }

        public void Reverse()
        {
            //if (GetState(State.jumping))
            //    return;

            SetState(State.directionLeft, !GetState(State.directionLeft));
            if (GetState(State.directionLeft))
                speed.X = -Config.Player.MOVE_SPEED;
            else
                speed.X = Config.Player.MOVE_SPEED;
        }

        public void Jump()
        {
            if (GetState(State.jumping))
            {
                SetState(State.tryJump, true);
                return;
            }

            if (GetState(State.stopped))
            {
                SetState(State.stoppedTemp, true);
                SetState(State.jumpStopped, true);
            }
            SetState(State.jumping, true);
            SetSpeedY(Config.Player.JUMP_SPEED);
            jumpY = position.Y;
        }

        public void Stop(GameTime gameTime)
        {
            //if (GetState(Player.State.jumping) && !GetState(Player.State.falling))
            //    return;

            SetState(Player.State.stopped, true);
            if (stopTimeForReverse == TimeSpan.Zero)
                stopTimeForReverse = gameTime.TotalGameTime;
        }

        public void EndOfStop(GameTime gameTime)
        {
            SetState(Player.State.stopped, false);
            if ((gameTime.TotalGameTime - stopTimeForReverse).TotalMilliseconds < Config.Player.STOPTIME_REVERSE_MS)
            {
                if (GetState(State.jumping) && GetState(State.falling)) // when jumping, reverse only on falling
                    Reverse();
                else if (!GetState(State.jumping))  // reverse when not jumping
                    Reverse();
            }
            stopTimeForReverse = TimeSpan.Zero;
        }

        public void HitTheCeiling(Rectangle tileBox)
        {
            SetVerticalPositionOnTile(tileBox);
            SetState(State.falling, true);
            SetSpeedY(10.0f);
        }

        public void HitTheGround(Rectangle tileBox)
        {
            if (GetState(Player.State.falling))
            {
                if ((boundingBox.Y - jumpY) > Config.Player.MAX_FALL_DISTANCE)
                {
                    SetState(State.dying, true);
                    return;
                }
            }
            SetVerticalPositionOnTile(tileBox);

            if (GetState(State.tryJump))
            {
                Jump();
                SetState(Player.State.tryJump, false);
            }

            SetState(Player.State.stoppedTemp, false);
            SetState(Player.State.jumping, false);
            SetState(Player.State.falling, false);
            SetState(Player.State.jumpStopped, false);
        }

        public void HitTheWall(Rectangle tileBox)
        {
            SetHorizontalPositionOnTile(tileBox);
            if (!GetState(Player.State.jumping))
                Reverse();
        }

        public void PixelExplosion()
        {
            Texture2D texture = GetCurrentFrameTexture(graphics);
            Random rnd = new Random();

            Color[] textureColors = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(textureColors);

            for (int i = 0; i < textureColors.Length; i++)
            {
                if (textureColors[i].A == 255)
                {
                    Vector2 boomCenter = position + new Vector2(texture.Width / 2, texture.Height / 2);
                    Vector2 pixPos = position + new Vector2(i % texture.Width, i / texture.Width);
                    Vector2 pixSpeed = (pixPos - boomCenter) * rnd.Next(0, Config.PixelParticle.MAX_EXPLOSION_MAGNITUDE);
                    Vector2 acc = Vector2.Zero;// new Vector2(rnd.Next(-100, 100), rnd.Next(-100, 100));

                    Globals.CurrentLevelState.AddPixelParticle(new PixelParticle(pixel, pixPos,
                                    0.0f,//Config.PixelParticle.PIXELPARTICLE_PLAYER_LIFETIME_MAX,
                                    pixSpeed, acc, boomColors[rnd.Next(boomColors.Length)], true, Globals.CurrentMap));
                }
            }
        }



        public void SetMovingPlatformState(float modifyValue)
        {
            if (modifyValue == 0.0f)
                return;

            enviroSpeed.X = modifyValue;
            SetState(State.onMovingPlatform, true);
        }

        public void ResetMovingPlatformState()
        {
            enviroSpeed.X = 0.0f;
            SetState(State.onMovingPlatform, false);
        }

        public Texture2D GetCurrentFrameTexture(GraphicsDeviceManager graphic)
        {
            return Util.BlitTexture(graphic, texture, sourceRectangle, GetState(State.directionLeft));
        }
    }
}
