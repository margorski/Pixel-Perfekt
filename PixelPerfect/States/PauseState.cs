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
    class PauseState : GameState
    {
        private TimeSpan fadeTime;

#if WINDOWS
        private MouseState prevMouseState;
        private MouseState currMouseState;
#else
        TouchCollection touchCollection;
#endif
        GamePadState prevGPState;
        GamePadState currGPState;

        Button playButton = new Button("", new Rectangle(72,36, 24, 24), Globals.textureDictionary["play2"], Globals.silkscreenFont, false);
        Button restartButton = new Button("", new Rectangle(72,64, 24, 24), Globals.textureDictionary["restart"], Globals.silkscreenFont, false);
        Button infoButton = new Button("", new Rectangle(72, 92, 24, 24), Globals.textureDictionary["info"], Globals.silkscreenFont, false);
        Button backButton = new Button("", new Rectangle(72, 120, 24, 24), Globals.textureDictionary["back"], Globals.silkscreenFont, false);

        public PauseState()
        {
        }

        public override void Enter(int previousStateId)
        {
            fadeTime = new TimeSpan(0, 0, 0, 0, 500);
            if (Globals.musicEnabled)
                MediaPlayer.Pause();
#if WINDOWS
            currMouseState = prevMouseState = Mouse.GetState();
#else
            touchCollection = TouchPanel.GetState();
#endif
        }

        public override void Exit(int nextStateId)
        {
            if (Globals.musicEnabled)
                MediaPlayer.Resume();
        }

        public override void Suspend(int pushedStateId)
        {

        }

        public override void Resume(int poppedStateId)
        {

        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED + 2, Config.SCREEN_HEIGHT_SCALED), new Color(0, 0, 0, (int)((1.0f - (float)fadeTime.TotalMilliseconds / 500.0f) * 180)));
            spriteBatch.DrawString(Globals.silkscreenFont, "PAUSE", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 44, 6), Color.White, 0.0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "CONTINUE", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 39), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "RESTART", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 67), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "CONTROLS", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 95), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "BACK", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 123), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            infoButton.Draw(spriteBatch);
            playButton.Draw(spriteBatch);
            restartButton.Draw(spriteBatch);
            backButton.Draw(spriteBatch);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            currGPState = GamePad.GetState(PlayerIndex.One);
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
                Globals.gameStateManager.PopState();

            prevGPState = currGPState;

            if (fadeTime > TimeSpan.Zero)
            {
                fadeTime -= gameTime.ElapsedGameTime;
                if (fadeTime < TimeSpan.Zero)
                    fadeTime = TimeSpan.Zero;
            }
#if !WINDOWS
            touchCollection = TouchPanel.GetState();

            foreach (TouchLocation touch in touchCollection)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                restartButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                infoButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                }
                if (touch.State == TouchLocationState.Released)
                {
                    if (playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                        Globals.gameStateManager.PopState();
                    else if (restartButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                    {                    
                        Globals.gameStateManager.PopState();
                        Globals.CurrentLevelState.Reset();
                    }
                    else if (backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                    {
                        GoBack();
                    }
                    else if (infoButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                    {
                        Globals.gameStateManager.PushState(Config.States.CONTROLS);                    
                    }
                }            
            }
#else
            currMouseState = Mouse.GetState();
            if (currMouseState.LeftButton == ButtonState.Pressed)
            {
                restartButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                infoButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                playButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
            }
            else if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                if (playButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                    Globals.gameStateManager.PopState();
                else if (restartButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                {                    
                    Globals.gameStateManager.PopState();
                    Globals.CurrentLevelState.Reset();
                }
                else if (backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                {
                    GoBack();
                }
                else if (infoButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                {
                    Globals.gameStateManager.PushState(Config.States.CONTROLS);                    
                }
            }
            prevMouseState = currMouseState;
#endif 
        }

        public void GoBack()
        {
            Globals.gameStateManager.PopState();
            Globals.gameStateManager.ChangeState(Config.States.LEVELSELECT);
        }
    }
}
