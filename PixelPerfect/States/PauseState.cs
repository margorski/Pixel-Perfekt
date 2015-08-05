using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
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
        Button skipButton = new Button("", new Rectangle(72, 92, 24, 24), Globals.textureDictionary["skip"], Globals.silkscreenFont, false);
        Button infoButton = new Button("", new Rectangle(180, 130, 24, 24), Globals.textureDictionary["info"], Globals.silkscreenFont, false);
        Button backButton = new Button("", new Rectangle(72, 120, 24, 24), Globals.textureDictionary["back"], Globals.silkscreenFont, false);
        Button musicButton = new Button("", new Rectangle(240, 130, 24, 24), Globals.textureDictionary["music"], Globals.silkscreenFont, true);
        Button soundButton = new Button("", new Rectangle(210, 130, 24, 24), Globals.textureDictionary["sound"], Globals.silkscreenFont, true);

        bool previousMusicEnabled = false;

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
            soundButton.value = Globals.soundEnabled;
            musicButton.value = Globals.musicEnabled;
            previousMusicEnabled = Globals.musicEnabled;

            if (Savestate.Instance.Skipped() || Globals.worlds[Globals.selectedWorld].LevelSkipped(Globals.selectedLevel) || Globals.selectedLevel >= Globals.worlds[Globals.selectedWorld].levels.Count - 1
                || Globals.worlds[Globals.selectedWorld].LevelCompleted(Globals.selectedLevel))
                skipButton.active = false;
            else
                skipButton.active = true;
        }

        public override void Exit(int nextStateId)
        {
            if (Globals.musicEnabled)
            {
                if (previousMusicEnabled)
                    MediaPlayer.Resume();
                else
                    MediaPlayer.Play(Globals.backgroundMusicList[Globals.CurrentMap.music]);
            }
        }

        public override void Suspend(int pushedStateId)
        {

        }

        public override void Resume(int poppedStateId)
        {
            soundButton.value = Globals.soundEnabled;
            musicButton.value = Globals.musicEnabled;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Color(0, 0, 0, (int)((1.0f - (float)fadeTime.TotalMilliseconds / 500.0f) * 100)));
            spriteBatch.DrawString(Globals.silkscreenFont, "PAUSE", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 44, 6), Color.White, 0.0f, Vector2.Zero, 3.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "CONTINUE", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 39), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "RESTART", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 67), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "SKIP", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 95), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "BACK", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 30, 123), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
            infoButton.Draw(spriteBatch);
            playButton.Draw(spriteBatch);
            restartButton.Draw(spriteBatch);
            backButton.Draw(spriteBatch);
            musicButton.Draw(spriteBatch);
            soundButton.Draw(spriteBatch);
            skipButton.Draw(spriteBatch);
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
                    musicButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                    soundButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                    skipButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
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
                    else if (musicButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                    {
                        Globals.musicEnabled = musicButton.value;
                        IsolatedStorageSettings.ApplicationSettings["music"] = Globals.musicEnabled;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                        continue;
                    }
                    else if (soundButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                    {
                        Globals.soundEnabled = soundButton.value;
                        IsolatedStorageSettings.ApplicationSettings["sound"] = Globals.soundEnabled;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                        continue;
                    }
                    else if (skipButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                    {
                        if (Globals.worlds[Globals.selectedWorld].Skip(Globals.selectedLevel))
                            GoBack();
                        continue;
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
                musicButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                soundButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                skipButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
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
                else if (musicButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                {
                    Globals.musicEnabled = musicButton.value;                    
                }
                else if (soundButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                {
                    Globals.soundEnabled = soundButton.value;
                }
                else if (skipButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                {
                    if (Globals.worlds[Globals.selectedWorld].Skip(Globals.selectedLevel))
                        GoBack();
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
