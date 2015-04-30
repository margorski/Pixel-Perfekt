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
using GameStateMachine;

#if !WINDOWS
using Microsoft.Phone.Tasks;
#endif

namespace PixelPerfect
{
    class MenuState : GameState
    {
        enum MenuPhase
        {
            MAIN,
            WORLDSELECT,
            LEVELSELECT,
            LEVELDETAILS
        }

        ContentManager content;
        GraphicsDeviceManager graphics;
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

        MenuPhase menuPhase = MenuPhase.MAIN;
        List<World> worlds = new List<World>();
        Texture2D levelTile;
        int selectedWorld = -1;
        int selectedLevel = -1;
        int currentPage = 0;

        Button skipButton;
        Button playButton;
        Button backButton;
        Button sendButton;
        Button resetButton;
        Button soundButton; // temp
        Button prevButton;
        Button nextButton;

        public MenuState(GraphicsDeviceManager graphics, ContentManager content, GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;
            this.content = content;
            this.graphics = graphics;

            levelTile = content.Load<Texture2D>("leveltile");

            worlds = World.LoadWorlds();

            skipButton = new Button("SKIP", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 30, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            playButton = new Button("PLAY", new Rectangle(Config.SCREEN_WIDTH_SCALED - 70, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            backButton = new Button("BACK", new Rectangle(10, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);

            sendButton = new Button("SEND", new Rectangle(Config.SCREEN_WIDTH_SCALED - 70, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            resetButton = new Button("RESET", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 30, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, false);
            soundButton = new Button("SOUND", new Rectangle(10, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, Globals.silkscreenFont, true);
            soundButton.value = Globals.playSounds;
            
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
            
            if (selectedWorld != -1)
                menuPhase = MenuPhase.LEVELSELECT;
            else
                menuPhase = MenuPhase.WORLDSELECT;

            World.RefreshWorldStatus(worlds);

            int levelId = Config.States.LEVEL;
            int textstateId = Config.States.TEXT;
            
            while (gameStateManager.UnregisterState(levelId++)) ;
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

            Update_HandleBack();
           
#if WINDOWS
            currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
#endif

            switch (menuPhase)
            {
                case MenuPhase.MAIN:        
#if WINDOWS            
                    if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                        menuPhase = MenuPhase.WORLDSELECT;
#else                
                    foreach (TouchLocation touch in touchState)
                    {
                        if (touch.State == TouchLocationState.Pressed)
                        {
                            menuPhase = MenuPhase.WORLDSELECT;
                        }
                    }
#endif
                    break;
                case MenuPhase.WORLDSELECT:
                    Update_WorldSelect();
                    break;

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
                    if (playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
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
                if (playButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
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
            return worlds[selectedWorld].Skip(selectedLevel);
        }

        private void StartLevel()
        {
            for (int i = 0; i < worlds[selectedWorld].levels.Count; i++) // initialize levelstates
            {
                var levelState = new LevelState(graphics, content, worlds[selectedWorld].directory, worlds[selectedWorld].GetLevelFile(i));
                levelState.scale = scale;
                gameStateManager.RegisterState(Config.States.LEVEL + i, levelState);
            }
            gameStateManager.ChangeState(Config.States.LEVEL + selectedLevel);
        }

        private void StartPacmanLevel()
        {
          
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
                case MenuPhase.MAIN:
                    gameStateManager.PopState();
                    return;

                case MenuPhase.LEVELSELECT:
                    menuPhase = MenuPhase.WORLDSELECT;
                    break;

                case MenuPhase.WORLDSELECT:
                    menuPhase = MenuPhase.MAIN;
                    break;

                case MenuPhase.LEVELDETAILS:
                    if (worlds[selectedWorld].levels.Count == 1)
                        menuPhase = MenuPhase.WORLDSELECT;
                    else
                        menuPhase = MenuPhase.LEVELSELECT;
                    break;
            }
        }

        public void Update_WorldSelect()
        {
#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    if (sendButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        SendDataEmail();
                        break;
                    }
                    else if (resetButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        ResetSave();
                        break;
                    }
                    else if (soundButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        Globals.playSounds = soundButton.value;
                        break;
                    }
                    else 
                    {
                        int clickedSquare = ClickedSquare((int)touch.Position.X, (int)touch.Position.Y);

                        if (clickedSquare < 0 || clickedSquare > worlds.Count - 1)
                            return;
                    
                        if (!worlds[clickedSquare].active)
                           return;

                        SelectWorld(clickedSquare);
                        break;
                    }
                }
            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (sendButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                {
                    SendDataEmail();
                }
                else if (soundButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale))
                {
                    Globals.playSounds = soundButton.value;
                }
                else
                {
                    int clickedSquare = ClickedSquare(currMouseState.X, currMouseState.Y);

                    if (clickedSquare < 0 || clickedSquare > worlds.Count - 1)
                        return;

                    if (!worlds[clickedSquare].active)
                        return;

                    SelectWorld(clickedSquare);
                }
            }
#endif
        }

        private void ResetSave()
        {
            Savestate.Reset();
            Savestate.Instance.Save();
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

        private void SelectWorld(int world)
        {
            selectedWorld = world;
            if (worlds[selectedWorld].levels.Count == 1)
                SelectLevel(0);
            else
                menuPhase = MenuPhase.LEVELSELECT;            
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

                    if (!worlds[selectedWorld].LevelActivated(clickedSquare))
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

                if (!worlds[selectedWorld].LevelActivated(clickedSquare))
                    return;

                SelectLevel(clickedSquare + currentPage * 10);
            }
#endif
        }

        private void PreviousPage()
        {
            if (--currentPage < 0)
                currentPage = (worlds[selectedWorld].levels.Count - 1) / 10;
        }

        private void NextPage()
        {
            if (++currentPage > ((worlds[selectedWorld].levels.Count - 1) / 10))
                currentPage = 0;
        }

        public void SelectLevel(int level)
        {
            if (level >= worlds[selectedWorld].levels.Count)
                return;

            selectedLevel = level;
            if (Savestate.Instance.Skipped() || worlds[selectedWorld].LevelSkipped(level) || level >= worlds[selectedWorld].levels.Count - 1)
                skipButton.active = false;
            else
                skipButton.active = true;
            menuPhase = MenuPhase.LEVELDETAILS;
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (!upsidedownBatch && Globals.upsideDown)
                return;

            switch (menuPhase)
            {
                case MenuPhase.MAIN:
                    Draw_Main(spriteBatch);
                    break;

                case MenuPhase.WORLDSELECT:
                    Draw_WorldSelect(spriteBatch);
                    break;

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
            bool exist = Savestate.Instance.levelSaves.TryGetValue(worlds[selectedWorld].GetLevelFile(selectedLevel), out levelsave);

            Color nameColor = (worlds[selectedWorld].LevelCompleted(selectedLevel) ? Color.Green : worlds[selectedWorld].LevelSkipped(selectedLevel) ? Color.Gold : Color.White);
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

            if (worlds[selectedWorld].levels[selectedLevel].thumbnail != null)
                spriteBatch.Draw(worlds[selectedWorld].levels[selectedLevel].thumbnail, new Vector2(50, 25), Color.White);

            Util.DrawStringAligned(spriteBatch, worlds[selectedWorld].levels[selectedLevel].levelName, Globals.silkscreenFont, nameColor, new Rectangle(0, 10, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Center);
            Util.DrawStringAligned(spriteBatch, deathsString, Globals.silkscreenFont, timeColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Left);
            Util.DrawStringAligned(spriteBatch, timeString, Globals.silkscreenFont, deathColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Right);

            backButton.Draw(spriteBatch);
            skipButton.Draw(spriteBatch);
            playButton.Draw(spriteBatch);
        }

        public void Draw_Main(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Globals.silkscreenFont, "PIXEL PERFECT", new Vector2(100, 7), Color.White);
        }

        public void Draw_WorldSelect(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Globals.silkscreenFont, "WORLD SELECT", new Vector2(100, 7), Color.White);
            int x, y;
            Color color = Color.White;

            for (int i = 0; i < worlds.Count; i++)
            {
                x = Config.Menu.OFFSET_X + (i % Config.Menu.NUM_IN_ROW) * (levelTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (i / Config.Menu.NUM_IN_ROW) * (levelTile.Height + Config.Menu.VERTICAL_SPACE);

                color = worlds[i].active ? Color.White : Color.Gray;
                spriteBatch.Draw(levelTile, new Vector2(x, y), color);

                var textOffset = Globals.silkscreenFont.MeasureString(worlds[i].name) / 2;
                spriteBatch.DrawString(Globals.silkscreenFont, worlds[i].name, new Vector2(x - textOffset.X + levelTile.Width / 2, y + levelTile.Height + Config.Menu.TEXT_SPACE), color);
            }
            sendButton.Draw(spriteBatch);
            resetButton.Draw(spriteBatch);
            soundButton.Draw(spriteBatch);
        }

        public void Draw_LevelSelect(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Globals.silkscreenFont, "LEVEL SELECT", new Vector2(100, 7), Color.White);

            int x, y;
            Color color = Color.White;

            var currentPageLevels = worlds[selectedWorld].levels.Count - currentPage * 10; // levels amount on current page
            var count = (currentPageLevels < 10 ? currentPageLevels : 10);

            for (int i = currentPage * 10; i < count + currentPage * 10; i++)
            {
                var posI = i - currentPage * 10;

                x = Config.Menu.OFFSET_X + (posI % Config.Menu.NUM_IN_ROW) * (levelTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (posI / Config.Menu.NUM_IN_ROW) * (levelTile.Height + Config.Menu.VERTICAL_SPACE);
                
                color = worlds[selectedWorld].LevelCompleted(i) ? Color.Green : 
                        worlds[selectedWorld].LevelSkipped(i)   ? Color.Gold : 
                        worlds[selectedWorld].LevelActivated(i) ? Color.White : Color.Gray;

                spriteBatch.Draw(levelTile, new Vector2(x, y), color);

                var textOffset = Globals.silkscreenFont.MeasureString(worlds[selectedWorld].levels[i].levelName) / 2;
                spriteBatch.DrawString(Globals.silkscreenFont, worlds[selectedWorld].levels[i].levelName, new Vector2(x - textOffset.X + levelTile.Width / 2, y + levelTile.Height + Config.Menu.TEXT_SPACE), color);
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
