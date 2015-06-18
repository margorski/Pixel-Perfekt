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
using GameStateMachine;

#if !WINDOWS
using Microsoft.Phone.Tasks;
#endif

namespace PixelPerfect
{
    class LevelSelectState : GameState
    {
        enum MenuPhase
        {
            LEVELSELECT,
            LEVELDETAILS
        }

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

        MenuPhase menuPhase = MenuPhase.LEVELSELECT;
        List<World> worlds = new List<World>();
        Texture2D levelTile;
        int selectedLevel = -1;
        int currentPage = 0;

        Button skipButton;
        Button backButton;
        Button startButton;
        Button prevButton;
        Button nextButton;

        public LevelSelectState(GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;

            levelTile = Globals.content.Load<Texture2D>("leveltile");

            worlds = World.LoadWorlds();

            skipButton = new Button("SKIP", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 30, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            startButton = new Button("START", new Rectangle(Config.SCREEN_WIDTH_SCALED - 70, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            backButton = new Button("BACK", new Rectangle(10, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);

            prevButton = new Button("PREV", new Rectangle(10, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            nextButton = new Button("NEXT", new Rectangle(Config.SCREEN_WIDTH_SCALED - 70, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
        }

        public override void Enter(int previousStateId)
        {
#if !WINDOWS
            touchState = TouchPanel.GetState();
#else
            prevMouseState = currMouseState = Mouse.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);


            World.RefreshWorldStatus(worlds);

            int textstateId = Config.States.TEXT;

            while (gameStateManager.UnregisterState(Config.States.LEVEL)) ;
            while (gameStateManager.UnregisterState(textstateId++)) ;

            
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

#if WINDOWS
            currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
#endif


            Update_HandleBack();          

            switch (menuPhase)
            {
                case MenuPhase.LEVELSELECT:
                    Update_LevelSelect();
                    break;

                case MenuPhase.LEVELDETAILS:
                    Update_LevelDetails();
                    break;
            }

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
                    else if (backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                        GoBack();
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
                else if (backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                    GoBack();
            }
#endif 
        }

        private bool Skip()
        {
            return worlds[0].Skip(selectedLevel);
        }

        private void StartLevel()
        {
            var levelState = new LevelState(worlds[0].directory, worlds[0].GetLevelFile(selectedLevel));
            levelState.scale = scale;
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
            switch (menuPhase)
            {
                case MenuPhase.LEVELSELECT:
                    gameStateManager.ChangeState(Config.States.TITLESCREEN, true);
                    break;

                case MenuPhase.LEVELDETAILS:
                    menuPhase = MenuPhase.LEVELSELECT;
                    break;
            }
        }


        private void ResetSave()
        {
            Savestate.Reset();
        }

        private void SendDataEmail()
        {
#if WINDOWS
            return;
#else
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "[PIXEL PERFECT] Stats from " + Convert.ToBase64String((byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId"));
            emailComposeTask.To = "takashivip@gmail.com";           
            emailComposeTask.Body = "Level key\tTime\t Deaths\t Deaths till completed\n";
            foreach (KeyValuePair<string, Levelsave> levelsave in Savestate.Instance.levelSaves)
            {
                if (levelsave.Value.completed)
                    emailComposeTask.Body += levelsave.Key + "\t" + levelsave.Value.bestTime.ToString() + "\t" + levelsave.Value.deathCount + "\t" + levelsave.Value.completeDeathCount + "\n";
            }
            emailComposeTask.Show();
#endif
        }

        public void Update_LevelSelect()
        {            
#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    if (prevButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        PreviousPage();
                        continue;
                    }

                    if (nextButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        NextPage();
                        continue;
                    }

                    int clickedSquare = ClickedSquare((int)touch.Position.X, (int)touch.Position.Y);

                    if (clickedSquare < 0)
                        continue;

                    if (!worlds[0].LevelActivated(clickedSquare))
                        continue;

                    SelectLevel(clickedSquare + currentPage * 10);
                }

            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (prevButton.Clicked(currMouseState.X, currMouseState.Y, scale))
                {
                    PreviousPage();
                    return;
                }

                if (nextButton.Clicked(currMouseState.X, currMouseState.Y, scale))
                {
                    NextPage();
                    return;
                }

                int clickedSquare = ClickedSquare(currMouseState.X, currMouseState.Y);
                
                if (clickedSquare < 0)
                    return;

                if (!worlds[0].LevelActivated(clickedSquare))
                    return;

                SelectLevel(clickedSquare + currentPage * 10);
            }
#endif
        }

        private void PreviousPage()
        {
            if (--currentPage < 0)
                currentPage = (worlds[0].levels.Count - 1) / 10;
        }

        private void NextPage()
        {
            if (++currentPage > ((worlds[0].levels.Count - 1) / 10))
                currentPage = 0;
        }

        public void SelectLevel(int level)
        {
            if (level >= worlds[0].levels.Count)
                return;

            selectedLevel = level;
            if (Savestate.Instance.Skipped() || worlds[0].LevelSkipped(level) || level >= worlds[0].levels.Count - 1)
                skipButton.active = false;
            else
                skipButton.active = true;

            if (worlds[0].LevelSkipped(level) || worlds[0].LevelActivated(level))
                startButton.active = true;
            else
                startButton.active = false;

            menuPhase = MenuPhase.LEVELDETAILS;
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            switch (menuPhase)
            {
                case MenuPhase.LEVELSELECT:
                    Draw_LevelSelect(spriteBatch);
                    break;

                case MenuPhase.LEVELDETAILS:
                    Draw_LevelDetails(spriteBatch);
                    break;
            }
        }

        private void Draw_LevelDetails(SpriteBatch spriteBatch)
        {
            Levelsave levelsave;
            bool exist = Savestate.Instance.levelSaves.TryGetValue(worlds[0].GetLevelFile(selectedLevel), out levelsave);

            Color nameColor = (worlds[0].LevelCompleted(selectedLevel) ? Color.Green : worlds[0].LevelSkipped(selectedLevel) ? Color.Gold : Color.White);
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

            if (worlds[0].levels[selectedLevel].thumbnail != null)
                spriteBatch.Draw(worlds[0].levels[selectedLevel].thumbnail, new Vector2(50, 25), Color.White);

            Util.DrawStringAligned(spriteBatch, worlds[0].levels[selectedLevel].levelName, Globals.silkscreenFont, nameColor, new Rectangle(0, 10, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Center);
            Util.DrawStringAligned(spriteBatch, deathsString, Globals.silkscreenFont, timeColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Left);
            Util.DrawStringAligned(spriteBatch, timeString, Globals.silkscreenFont, deathColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Right);

            backButton.Draw(spriteBatch);
            skipButton.Draw(spriteBatch);
            startButton.Draw(spriteBatch);
        }

        public void Draw_LevelSelect(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Globals.silkscreenFont, "LEVEL SELECT", new Vector2(100, 7), Color.White);

            int x, y;
            Color color = Color.White;

            var currentPageLevels = worlds[0].levels.Count - currentPage * 10; // levels amount on current page
            var count = (currentPageLevels < 10 ? currentPageLevels : 10);

            for (int i = currentPage * 10; i < count + currentPage * 10; i++)
            {
                var posI = i - currentPage * 10;

                x = Config.Menu.OFFSET_X + (posI % Config.Menu.NUM_IN_ROW) * (levelTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (posI / Config.Menu.NUM_IN_ROW) * (levelTile.Height + Config.Menu.VERTICAL_SPACE);
                
                color = worlds[0].LevelCompleted(i) ? Color.Green : 
                        worlds[0].LevelSkipped(i)   ? Color.Gold : 
                        worlds[0].LevelActivated(i) ? Color.White : Color.Gray;

                spriteBatch.Draw(levelTile, new Vector2(x, y), color);

                var textOffset = Globals.silkscreenFont.MeasureString(worlds[0].levels[i].levelName) / 2;
                spriteBatch.DrawString(Globals.silkscreenFont, worlds[0].levels[i].levelName, new Vector2(x - textOffset.X + levelTile.Width / 2, y + levelTile.Height + Config.Menu.TEXT_SPACE), color);
            }
            prevButton.Draw(spriteBatch);
            nextButton.Draw(spriteBatch);
        }

        public int ClickedSquare(int X, int Y)
        {
            X /= (int)scale;
            Y /= (int)scale;

            if ((Y) < Config.Menu.OFFSET_Y || (Y) > Config.Menu.OFFSET_Y + (levelTile.Height + Config.Menu.VERTICAL_SPACE) * 2)
                return -1;

            int x = (X - Config.Menu.OFFSET_X) / (levelTile.Width + Config.Menu.HORIZONTAL_SPACE); 
            int y = (Y - Config.Menu.OFFSET_Y) / (levelTile.Height + Config.Menu.VERTICAL_SPACE);

            return x + y * Config.Menu.NUM_IN_ROW;
        }
    }
}
