using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

#if !WINDOWS
using Microsoft.Phone.Tasks;
#endif

namespace PixelPerfect
{
    class LevelDetailsState : GameState
    {
        GameStateManager gameStateManager;

#if !WINDOWS
        TouchCollection touchState;
#else
        MouseState prevMouseState;
        MouseState currMouseState;
        KeyboardState prevKeyboardState;
        KeyboardState currKeyboardState;
#endif
        GamePadState prevGPState;
        GamePadState currGPState;

        Texture2D levelTile;

        Button skipButton;
        Button startButton;

        WavyText caption; 

        public LevelDetailsState(GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;

            levelTile = Globals.content.Load<Texture2D>("leveltile");
            skipButton = new Button("SKIP", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 30, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            startButton = new Button("START", new Rectangle(Config.SCREEN_WIDTH_SCALED - 70, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
         }

        public override void Enter(int previousStateId)
        {
            int textstateId = Config.States.TEXT;

            while (gameStateManager.UnregisterState(Config.States.LEVEL)) ;
            while (gameStateManager.UnregisterState(textstateId++)) ;
#if !WINDOWS
            touchState = TouchPanel.GetState();
#else
            prevMouseState = currMouseState = Mouse.GetState();
            prevKeyboardState = currKeyboardState = Keyboard.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);

            if (Savestate.Instance.Skipped() || Globals.worlds[Globals.selectedWorld].LevelSkipped(Globals.selectedLevel) || Globals.selectedLevel >= Globals.worlds[Globals.selectedWorld].levels.Count - 1)
                skipButton.active = false;
            else
                skipButton.active = true;

            if (Globals.worlds[Globals.selectedWorld].LevelSkipped(Globals.selectedLevel) || Globals.worlds[Globals.selectedWorld].LevelActivated(Globals.selectedLevel))
                startButton.active = true;
            else
                startButton.active = false;

            var levelName = Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].levelName;
            var titlex = Config.SCREEN_WIDTH_SCALED / 2.0f - (Globals.silkscreenFont.MeasureString(levelName).X / 2.0f) * 2.0f;
            caption = new WavyText(levelName, new Vector2(titlex, 7), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);

            if (Globals.musicEnabled && MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(Globals.backgroundMusicList[Globals.rnd.Next(Globals.backgroundMusicList.Count)]);
        }

        public override void Exit(int nextStateId)
        {
        }

        public override void Resume(int poppedStateId)
        {
        }

        public override void Suspend(int pushedStateId)
        {            
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            if (caption != null)
                caption.Update(gameTime);
#if WINDOWS
            currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
#endif
            Update_HandleBack();          
            Update_LevelDetails();

#if WINDOWS
            prevMouseState = currMouseState;
#endif       
        }

        private void Update_LevelDetails()
        {

#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    if (startButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                        StartLevel();
                    else if (skipButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        if (Skip())
                            GoBack();
                    }
                }
            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (startButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                    StartLevel();
                else if (skipButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                {
                    if (Skip())
                        GoBack();
                }
            }
#endif 
        }

        private bool Skip()
        {
            return Globals.worlds[Globals.selectedWorld].Skip(Globals.selectedLevel);
        }

        private void StartLevel()
        {
            var levelState = new LevelState(Globals.worlds[Globals.selectedWorld].directory, Globals.worlds[Globals.selectedWorld].GetLevelFile(Globals.selectedLevel));
            levelState.scale = scale;
            levelState.name = Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].levelName; 
            gameStateManager.RegisterState(Config.States.LEVEL, levelState);
            gameStateManager.ChangeState(Config.States.LEVEL);
        }

        public void Update_HandleBack()
        {
            currGPState = GamePad.GetState(PlayerIndex.One);

            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released) 
                GoBack();

            prevGPState = currGPState;
#if WINDOWS
            currKeyboardState = Keyboard.GetState();

            if (currKeyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
                GoBack();
 
            prevKeyboardState = currKeyboardState;
#endif
        }

        private void GoBack()
        {
            gameStateManager.ChangeState(Config.States.LEVELSELECT, true, Config.Menu.TRANSITION_DELAY);
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            Levelsave levelsave;
            bool exist = Savestate.Instance.levelSaves.TryGetValue(Globals.worlds[Globals.selectedWorld].GetLevelFile(Globals.selectedLevel), out levelsave);

            Color nameColor = (Globals.worlds[Globals.selectedWorld].LevelCompleted(Globals.selectedLevel) ? Color.Green : Globals.worlds[Globals.selectedWorld].LevelSkipped(Globals.selectedLevel) ? Color.Gold : Color.White);
            Color deathColor = Color.White;
            Color timeColor = Color.White;

            string deathsString = "TOTAL DEATHS: ";
            string timeString = "BEST TIME: ";
            if (exist)
            {
                deathsString += levelsave.deathCount;
                timeString += levelsave.bestTime.ToString("mm\\:ss\\.f");
            }
            else
            {
                deathsString += "0";
                timeString += "00:00.0";
            }

            if (Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].thumbnail != null)
                spriteBatch.Draw(Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].thumbnail, new Vector2(50, 25), Color.White);

            //Util.DrawStringAligned(spriteBatch, Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].levelName, Globals.silkscreenFont, nameColor, new Rectangle(0, 10, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Center);
            if (caption != null)
                caption.Draw(spriteBatch);
            Util.DrawStringAligned(spriteBatch, deathsString, Globals.silkscreenFont, timeColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Left);
            Util.DrawStringAligned(spriteBatch, timeString, Globals.silkscreenFont, deathColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Right);

            skipButton.Draw(spriteBatch);
            startButton.Draw(spriteBatch);
        }
    }
}
