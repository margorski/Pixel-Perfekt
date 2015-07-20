using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class ControlsState : GameState
    {
        Button playButton = new Button("", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 12, 130, 24, 24), Globals.textureDictionary["play2"], Globals.silkscreenFont, false);

        Animation stopTapAnimation = new Animation(2, 1500, false);
        Animation playerRunAnimation = new Animation(Config.ANIM_FRAMES, Config.DEFAULT_ANIMATION_SPEED, true);

        //reverse
        private bool reverse = false;
        private bool reverseClick = false;        
        private TimeSpan reverseTime = TimeSpan.Zero;

        //jumping
        private float speed = 0.0f;
        private float jumpPosition = 0.0f;
        private bool jumping = false;
        private bool jumpingClicking = false;
        private TimeSpan jumpTime = TimeSpan.Zero;

        GamePadState prevGPState;
        GamePadState currGPState;
#if WINDOWS
        private MouseState previousMouseState;
        private MouseState currentMouseState;
#else
        TouchCollection touchCollection;
#endif

        public ControlsState()
        {
        }

        public override void Enter(int previousStateId)
        {
            reverseTime = TimeSpan.Zero;
            reverseClick = reverse = false;
            jumpPosition = 0.0f;
            jumping = false;
            jumpingClicking = false;
            jumpTime = TimeSpan.Zero;
#if WINDOWS
            currentMouseState = previousMouseState = Mouse.GetState();
#else
            touchCollection = TouchPanel.GetState();
#endif
        }

        public override void Exit(int nextStateId)
        {
        }

        public override void Suspend(int pushedStateId)
        {

        }

        public override void Resume(int poppedStateId)
        {

        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {            
            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED / 2 + 1, Config.SCREEN_HEIGHT_SCALED), new Color(Color.Indigo, 0.9f));
            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 + 1, 0, Config.SCREEN_WIDTH_SCALED / 2 + 1, Config.SCREEN_HEIGHT_SCALED), new Color(Color.SeaGreen, 0.9f));
            
            spriteBatch.DrawString(Globals.silkscreenFont, "LEFT SIDE", new Vector2(Config.SCREEN_WIDTH_SCALED / 4 - 37, 2), Color.White, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);

            spriteBatch.DrawString(Globals.silkscreenFont, "TAP TO", new Vector2(40, 20), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.textureDictionary["tap"], new Vector2(54, 34), new Rectangle(0, (reverseClick ? 24 : 0) , 24, 24), Color.White);
            spriteBatch.DrawString(Globals.silkscreenFont, "REVERSE", new Vector2(40, 64), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.spritesDictionary["player"].texture, new Rectangle(86, 40, 8, 16), new Rectangle(0, (playerRunAnimation.currentFrame + 1) * 16, 8, 16), Color.White, 0.0f, Vector2.Zero, (reverse ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0.0f);

            spriteBatch.DrawString(Globals.silkscreenFont, "HOLD TO", new Vector2(40, 94), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.textureDictionary["tap"], new Vector2(54, 108), new Rectangle(0, 24 * stopTapAnimation.currentFrame, 24, 24), Color.White);
            spriteBatch.DrawString(Globals.silkscreenFont, "STOP", new Vector2(52, 138), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.spritesDictionary["player"].texture, new Vector2(86, 114), new Rectangle(0, stopTapAnimation.currentFrame == 0 ? (playerRunAnimation.currentFrame + 1) * 16 : 0, 8, 16), Color.White);

            spriteBatch.DrawString(Globals.silkscreenFont, "RIGHT SIDE", new Vector2((Config.SCREEN_WIDTH_SCALED / 4) * 3 - 40, 2), Color.White, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "TAP TO", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 44, 50), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.textureDictionary["tap"], new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 54, 64), new Rectangle(0, (jumpingClicking ? 24 : 0), 24, 24), Color.White);
            spriteBatch.DrawString(Globals.silkscreenFont, "JUMP", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 50, 94), Color.White, 0.0f, Vector2.Zero, 1.3f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.spritesDictionary["player"].texture, new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 100, 70 + jumpPosition), new Rectangle(0, 0, 8, 16), Color.White);

            playButton.Draw(spriteBatch);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            currGPState = GamePad.GetState(PlayerIndex.One);
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
                Globals.gameStateManager.PopState();

            prevGPState = currGPState;

            stopTapAnimation.Update(gameTime);
            playerRunAnimation.Update(gameTime);
   
            if (!reverseClick)
            {
                reverseTime += gameTime.ElapsedGameTime;
                if (reverseTime.TotalMilliseconds >= 2000.0)
                {
                    reverseClick = true;
                    reverseTime = TimeSpan.Zero;
                }
            }
            else
            {
                reverseTime += gameTime.ElapsedGameTime;
                if (reverseTime.TotalMilliseconds >= 200.0)
                {
                    reverse = !reverse;
                    reverseTime = TimeSpan.Zero;
                    reverseClick = false;
                }            
            }

            if (!jumpingClicking && !jumping)
            {
                jumpTime += gameTime.ElapsedGameTime;
                if (jumpTime.TotalMilliseconds >= 1000.0)
                {
                    jumpingClicking = true;
                    jumpTime = TimeSpan.Zero;
                }                
            }
            else if (jumpingClicking)
            {
                jumpTime += gameTime.ElapsedGameTime;
                if (jumpTime.TotalMilliseconds >= 200)
                {
                    jumping = true;
                    jumpingClicking = false;
                    jumpTime = TimeSpan.Zero;
                    speed = Config.Player.JUMP_SPEED;
                }
            }
            else if (jumping)
            {
                float timeFactor = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0);
                speed += Config.Player.GRAVITY * timeFactor;
                jumpPosition += speed * timeFactor;
                if (jumpPosition >= 0.0f)
                {
                    jumpPosition = 0.0f;
                    jumping = false;
                }
            }
#if WINDOWS
            currentMouseState = Mouse.GetState();

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                playButton.Clicked(currentMouseState.Position.X, currentMouseState.Position.Y, scale, false);
            }
            else if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
            {
                if (playButton.Clicked(currentMouseState.Position.X, currentMouseState.Position.Y, scale, true))
                    Globals.gameStateManager.PopState();
            }

            previousMouseState = currentMouseState;
#else
            touchCollection = TouchPanel.GetState();

            foreach (TouchLocation touch in touchCollection)
            {
                if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                {
                    playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                }
                if (touch.State == TouchLocationState.Released)
                {
                    if (playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                        Globals.gameStateManager.PopState();
                }            
            }
#endif
        }
    }
}
