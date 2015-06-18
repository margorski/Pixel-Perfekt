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
using GameStateMachine;

#if !WINDOWS
using Microsoft.Phone.Tasks;
#endif

namespace PixelPerfect
{
    class TitlescreenState : GameState
    {
        GameStateManager gameStateManager;

#if !WINDOWS
        TouchCollection touchState;
#else
        MouseState prevMouseState;
        MouseState currMouseState;
#endif
        GamePadState prevGPState;
        GamePadState currGPState;

        Button playButton;        
        Button musicButton;
        Button soundButton;

        TimeSpan titleTimer = TimeSpan.Zero;
        const string title = "PIXEL PERFEKT";

        public LevelState backgroundLevel { set; private get; }

        public TitlescreenState(GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;

            playButton = new Button("", new Rectangle(88, 72, 96, 32), Globals.textureDictionary["play"], Globals.silkscreenFont, false);
            soundButton = new Button("", new Rectangle(32, 120, 24, 24), Globals.textureDictionary["sound"], Globals.silkscreenFont, true);
            soundButton.value = Globals.soundEnabled;
            musicButton = new Button("", new Rectangle(216, 120, 24, 24), Globals.textureDictionary["music"], Globals.silkscreenFont, true);
            musicButton.value = Globals.musicEnabled;
         }

        public override void Enter(int previousStateId)
        {
            if (Globals.musicEnabled && MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(Globals.backgroundMusicList[Config.Menu.MUSIC]);
#if !WINDOWS
            touchState = TouchPanel.GetState();
#else
            prevMouseState = currMouseState = Mouse.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);
            titleTimer = TimeSpan.Zero;

            if (backgroundLevel != null)
                backgroundLevel.Enter(previousStateId);
        }

        public override void Exit(int nextStateId)
        {
            if (backgroundLevel != null)
                backgroundLevel.Exit(nextStateId);
        }

        public override void Suspend(int pushedStateId)
        {
            MediaPlayer.Pause();
            if (backgroundLevel != null)
                backgroundLevel.Suspend(pushedStateId);
        }

        public override void Resume(int poppedStateId)
        {
            MediaPlayer.Resume();
            if (backgroundLevel != null)
                backgroundLevel.Resume(poppedStateId);
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (backgroundLevel != null)
                backgroundLevel.Draw(spriteBatch, suspended);

            float sineBase = (float)(2 * Math.PI * (titleTimer.TotalMilliseconds / (double)Config.Menu.TITLETIME));
            for (int i = 0; i < title.Length; i++)
            {
                spriteBatch.DrawString(Globals.silkscreenFont, title.Substring(i, 1),
                                       new Vector2(14 + Globals.silkscreenFont.MeasureString(title.Substring(0, i)).X * (Config.Menu.TITLESCALE - 0.5f), 10 + (float)Math.Sin(sineBase + (i / Config.Menu.SINSHIFT) * Math.PI * 2) * Config.Menu.SINPOW),
                                       Config.titleColors[i], 0.0f, Vector2.Zero, Config.Menu.TITLESCALE, SpriteEffects.None, 0.0f);

            }

            playButton.Draw(spriteBatch);
            musicButton.Draw(spriteBatch);
            soundButton.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            titleTimer += gameTime.ElapsedGameTime;
            if (titleTimer.TotalMilliseconds >= Config.Menu.TITLETIME)
                titleTimer = TimeSpan.Zero;


#if WINDOWS
            currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
            if (backgroundLevel != null)
                backgroundLevel.TouchInput(gameTime, touchState);
#endif

            if (backgroundLevel != null)
                backgroundLevel.Update(gameTime, suspended);

#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {

                    if (musicButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        Globals.musicEnabled = musicButton.value;
                        IsolatedStorageSettings.ApplicationSettings["music"] = Globals.musicEnabled;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                        continue;
                    }        
                    else if (soundButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        Globals.soundEnabled = soundButton.value;
                        IsolatedStorageSettings.ApplicationSettings["sound"] = Globals.soundEnabled;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                        continue;
                    }
                    else if (playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        gameStateManager.ChangeState(Config.States.LEVELSELECT, true);
                        break;
                    }
                }
            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (musicButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                {
                    Globals.musicEnabled = musicButton.value;
                    if (Globals.musicEnabled)
                        MediaPlayer.Play(Globals.backgroundMusicList[Globals.rnd.Next(Globals.backgroundMusicList.Count)]);
                    else
                        MediaPlayer.Stop();
                }
                else if (soundButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                {
                    Globals.soundEnabled = soundButton.value;
                }
                else if (playButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                {
                    gameStateManager.ChangeState(Config.States.LEVELSELECT, true);
                }
            }
#endif



#if WINDOWS
            prevMouseState = currMouseState;
#endif      
        }
    }
}
