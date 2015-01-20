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
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
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

                selectedWorld = clickedSquare;
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

                for (int i = 0; i < worlds[selectedWorld].levelNames.Count; i++)
                {
                    var levelState = new LevelState(graphics, content, worlds[selectedWorld].directory, worlds[selectedWorld].GetLevelFile(i), gameStateManager);
                    levelState.scale = scale;
                    gameStateManager.RegisterState(Config.States.LEVEL + i, levelState);
                }
                gameStateManager.ChangeState(Config.States.LEVEL + clickedSquare);
            }
#endif 
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended)
        {
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

            for (int i = 0; i < worlds.Count; i++)
            {
                x = (i % 5) * 40;
                y = 25 + (i / 5) * 45;

                spriteBatch.Draw(levelTile, new Vector2(5 + x, y), Color.White);
                spriteBatch.DrawString(menuFont, worlds[i].name, new Vector2(x, y + 32), Color.White);
            }
        }

        public void Draw_LevelSelect(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(menuFont, "LEVEL SELECT", new Vector2(100, 7), Color.White);

            int x, y;

            for (int i = 0; i < worlds[selectedWorld].levelNames.Count; i++)
            {
                x = (i % 5) * 40;
                y = 25 + (i / 5) * 45;

                spriteBatch.Draw(levelTile, new Vector2(5 + x, y), Color.White);
                spriteBatch.DrawString(menuFont, worlds[selectedWorld].levelNames[i], new Vector2(x, y + 32), Color.White);
            }
        }

        public int ClickedSquare(int X, int Y)
        {
            X /= (int)scale;
            Y /= (int)scale;

            if ((Y) < 30 || (Y) > 110)
                return -1;

            int x = (X) / 40; 
            int y = (Y - 25) / 45;

            return x + y * 5;
        }
    }
}
