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
using GameStateMachine;

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
        SpriteFont menuFont;

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
        int selectedWorld = 0;
        int selectedLevel = 0;

        Button skipButton;
        Button playButton;
        Button backButton;

        public MenuState(GraphicsDeviceManager graphics, ContentManager content, GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;
            this.content = content;
            this.graphics = graphics;
        }

        public override void Enter(int previousStateId)
        {
            menuFont = content.Load<SpriteFont>("Silkscreen");
            levelTile = content.Load<Texture2D>("leveltile");
#if !WINDOWS
            touchState = TouchPanel.GetState();
#else
            prevMouseState = currMouseState = Mouse.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);
            worlds = World.LoadWorlds();
            menuPhase = MenuPhase.WORLDSELECT;
            int levelId = Config.States.LEVEL;
            int textstateId = Config.States.TEXT;
            while (gameStateManager.UnregisterState(levelId++)) ;
            while (gameStateManager.UnregisterState(textstateId++)) ;

            skipButton = new Button("SKIP", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 30, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, menuFont);
            playButton = new Button("PLAY", new Rectangle(Config.SCREEN_WIDTH_SCALED - 70, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, menuFont);
            backButton = new Button("BACK", new Rectangle(10, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), levelTile, menuFont);
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

            switch (menuPhase)
            {
                case MenuPhase.MAIN:                    
                    if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                        menuPhase = MenuPhase.WORLDSELECT;
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

            prevMouseState = currMouseState;
#else
            touchState = TouchPanel.GetState();
            

            switch (menuPhase)
            {
                case MenuPhase.MAIN:
                    foreach (TouchLocation touch in touchState)
                    {
                        if (touch.State == TouchLocationState.Pressed)
                        {
                            menuPhase = MenuPhase.WORLDSELECT;
                        }
                    }
                    break;

                case MenuPhase.WORLDSELECT:
                    Update_WorldSelect();
                    break;

                case MenuPhase.LEVELSELECT:
                    Update_LevelSelect();
                    break;
            }
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
                var levelState = new LevelState(graphics, content, worlds[selectedWorld].directory, worlds[selectedWorld].GetLevelFile(i), gameStateManager);
                levelState.scale = scale;
                gameStateManager.RegisterState(Config.States.LEVEL + i, levelState);
            }
            gameStateManager.ChangeState(Config.States.LEVEL + selectedLevel);
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
                    int clickedSquare = ClickedSquare((int)touch.Position.X, (int)touch.Position.Y);

                    if (clickedSquare < 0 || clickedSquare > worlds.Count - 1)
                        return;
                    
                    if (!worlds[clickedSquare].active)
                       return;

                    SelectWorld(clickedSquare);
                    break;
                }
            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                int clickedSquare = ClickedSquare(currMouseState.X, currMouseState.Y);

                if (clickedSquare < 0 || clickedSquare > worlds.Count - 1)
                    return;

                if (!worlds[clickedSquare].active)
                    return;

                SelectWorld(clickedSquare);
            }
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
                    int clickedSquare = ClickedSquare((int)touch.Position.X, (int)touch.Position.Y);

                    if (clickedSquare < 0)
                        return;

                    if (!worlds[selectedWorld].LevelActivated(clickedSquare))
                        return;

                    SelectLevel(clickedSquare);
                }

            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                int clickedSquare = ClickedSquare(currMouseState.X, currMouseState.Y);
                
                if (clickedSquare < 0)
                    return;

                if (!worlds[selectedWorld].LevelActivated(clickedSquare))
                    return;

                SelectLevel(clickedSquare);
            }
#endif 
        }

        public void SelectLevel(int level)
        {
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

            Color nameColor = (worlds[selectedWorld].LevelCompleted(selectedLevel) ? Color.Gold : worlds[selectedWorld].LevelSkipped(selectedLevel) ? Color.Green : Color.White);
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
            
            Util.DrawStringAligned(spriteBatch, worlds[selectedWorld].levels[selectedLevel].levelName, menuFont, nameColor, new Rectangle(0, 10, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Center);            
            Util.DrawStringAligned(spriteBatch, deathsString, menuFont, timeColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Left);            
            Util.DrawStringAligned(spriteBatch, timeString, menuFont, deathColor, new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 40, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Vector2(10, 0), Util.Align.Right);

            backButton.Draw(spriteBatch);
            skipButton.Draw(spriteBatch);
            playButton.Draw(spriteBatch);
        }

        public void Draw_Main(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(menuFont, "PIXEL PERFECT", new Vector2(100, 7), Color.White);
        }

        public void Draw_WorldSelect(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(menuFont, "WORLD SELECT", new Vector2(100, 7), Color.White);
            int x, y;
            Color color = Color.White;

            for (int i = 0; i < worlds.Count; i++)
            {
                x = Config.Menu.OFFSET_X + (i % Config.Menu.NUM_IN_ROW) * (levelTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (i / Config.Menu.NUM_IN_ROW) * (levelTile.Height + Config.Menu.VERTICAL_SPACE);

                color = worlds[i].active ? Color.White : Color.Gray;
                spriteBatch.Draw(levelTile, new Vector2(x, y), color);

                var textOffset = menuFont.MeasureString(worlds[i].name) / 2;
                spriteBatch.DrawString(menuFont, worlds[i].name, new Vector2(x - textOffset.X + levelTile.Width / 2, y + levelTile.Height + Config.Menu.TEXT_SPACE), color);
            }
        }

        public void Draw_LevelSelect(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(menuFont, "LEVEL SELECT", new Vector2(100, 7), Color.White);

            int x, y;
            Color color = Color.White;

            for (int i = 0; i < worlds[selectedWorld].levels.Count; i++)
            {
                x = Config.Menu.OFFSET_X + (i % Config.Menu.NUM_IN_ROW) * (levelTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (i / Config.Menu.NUM_IN_ROW) * (levelTile.Height + Config.Menu.VERTICAL_SPACE);

                color = worlds[selectedWorld].LevelCompleted(i) ? Color.Gold  : 
                        worlds[selectedWorld].LevelSkipped(i)   ? Color.Green : 
                        worlds[selectedWorld].LevelActivated(i) ? Color.White : Color.Gray;

                spriteBatch.Draw(levelTile, new Vector2(x, y), color);

                var textOffset = menuFont.MeasureString(worlds[selectedWorld].levels[i].levelName) / 2;
                spriteBatch.DrawString(menuFont, worlds[selectedWorld].levels[i].levelName, new Vector2(x - textOffset.X + levelTile.Width / 2, y + levelTile.Height + Config.Menu.TEXT_SPACE), color);
            }
        }

        public int ClickedSquare(int X, int Y)
        {
            X /= (int)scale;
            Y /= (int)scale;

            if ((Y) < Config.Menu.OFFSET_Y || (Y) > Config.SCREEN_HEIGHT_SCALED)
                return -1;

            int x = (X - Config.Menu.OFFSET_X) / (levelTile.Width + Config.Menu.HORIZONTAL_SPACE); 
            int y = (Y - Config.Menu.OFFSET_Y) / (levelTile.Height + Config.Menu.VERTICAL_SPACE);

            return x + y * Config.Menu.NUM_IN_ROW;
        }
    }
}
