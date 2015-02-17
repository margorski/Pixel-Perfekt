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

namespace PixelPerfect
{
    class MenuState : GameState
    {
        enum MenuPhase
        {
            MAIN,
            WORLDSELECT,
            LEVELSELECT
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

        public void Update_HandleBack()
        {
            currGPState = GamePad.GetState(PlayerIndex.One);
            currKeyboardState = Keyboard.GetState();

            if ((currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released) ||
                (currKeyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape)))
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
                }
            }
            prevGPState = currGPState;
            prevKeyboardState = currKeyboardState;
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

                    selectedWorld = clickedSquare;
                    menuPhase = MenuPhase.LEVELSELECT;
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

                selectedWorld = clickedSquare;
                for (int i = 0; i < worlds[selectedWorld].levelNames.Count; i++) // initialize levelstates
                {
                    var levelState = new LevelState(graphics, content, worlds[selectedWorld].directory, worlds[selectedWorld].GetLevelFile(i), gameStateManager);
                    levelState.scale = scale;
                    gameStateManager.RegisterState(Config.States.LEVEL + i, levelState);
                }
                menuPhase = MenuPhase.LEVELSELECT;
            }
#endif
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

                    for (int i = 0; i < worlds[selectedWorld].levelNames.Count; i++)
                    {
                        var levelState = new LevelState(graphics, content, worlds[selectedWorld].directory, worlds[selectedWorld].GetLevelFile(i), gameStateManager);
                        levelState.scale = scale;
                        gameStateManager.RegisterState(Config.States.LEVEL + i, levelState);
                    }
                    gameStateManager.ChangeState(Config.States.LEVEL + clickedSquare);
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

                gameStateManager.ChangeState(Config.States.LEVEL + clickedSquare);
            }
#endif 
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
            }
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

            for (int i = 0; i < worlds[selectedWorld].levelNames.Count; i++)
            {
                x = Config.Menu.OFFSET_X + (i % Config.Menu.NUM_IN_ROW) * (levelTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (i / Config.Menu.NUM_IN_ROW) * (levelTile.Height + Config.Menu.VERTICAL_SPACE);

                color = worlds[selectedWorld].LevelCompleted(i) ? Color.Gold : worlds[selectedWorld].LevelActivated(i) ? Color.White : Color.Gray;
                spriteBatch.Draw(levelTile, new Vector2(x, y), color);

                var textOffset = menuFont.MeasureString(worlds[selectedWorld].levelNames[i]) / 2;
                spriteBatch.DrawString(menuFont, worlds[selectedWorld].levelNames[i], new Vector2(x - textOffset.X + levelTile.Width / 2, y + levelTile.Height + Config.Menu.TEXT_SPACE), color);
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
