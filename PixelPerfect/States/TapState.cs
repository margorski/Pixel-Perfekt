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
    class TapState : GameState
    {
        private TimeSpan fadeTime;
        
        Animation tapAnimation = new Animation(2, 500, false);
                   
#if WINDOWS
        private MouseState previousMouseState;
        private MouseState currentMouseState;
#else
        TouchCollection touchCollection;
#endif

        public TapState()
        {
        }

        public override void Enter(int previousStateId)
        {
            fadeTime = new TimeSpan(0, 0, 0, 0, 500);
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
            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED + 2, Config.SCREEN_HEIGHT_SCALED), new Color(0, 0, 0, (int)((1.0f - (float)fadeTime.TotalMilliseconds / 500.0f) * 100)));
            spriteBatch.DrawString(Globals.silkscreenFont, "GO!", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 22, Config.SCREEN_HEIGHT_SCALED / 2 - 34), Color.White, 0.0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.textureDictionary["tap"], new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 12, Config.SCREEN_HEIGHT_SCALED / 2 - 10), new Rectangle(0, tapAnimation.GetCurrentFrame() * 24, 24, 24), Color.White);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;
            tapAnimation.Update(gameTime);

            if (fadeTime > TimeSpan.Zero)
            {
                fadeTime -= gameTime.ElapsedGameTime;
                if (fadeTime < TimeSpan.Zero)
                    fadeTime = TimeSpan.Zero;
            }
#if WINDOWS
            currentMouseState = Mouse.GetState();

            if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
            {
                Globals.gameStateManager.PopState(false);
            }

            previousMouseState = currentMouseState;
#else
            touchCollection = TouchPanel.GetState();

            foreach (TouchLocation tl in touchCollection)
            {
                if (tl.State == TouchLocationState.Pressed)
                {
                    Globals.gameStateManager.PopState();
                    return;
                }
            }
#endif
        }
    }
}
