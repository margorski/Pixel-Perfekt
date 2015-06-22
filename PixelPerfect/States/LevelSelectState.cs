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
    class LevelSelectState : GameState
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
        int currentPage = 0;

        WavyText caption;
        public LevelSelectState(GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;

            levelTile = Globals.content.Load<Texture2D>("leveltile");
        }

        public override void Enter(int previousStateId)
        {
#if !WINDOWS
            touchState = TouchPanel.GetState();
#else
            prevMouseState = currMouseState = Mouse.GetState();
            prevKeyboardState = currKeyboardState = Keyboard.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);

            var worldName = Globals.worlds[Globals.selectedWorld].name;
            var titlex = Config.SCREEN_WIDTH_SCALED / 2.0f - Globals.silkscreenFont.MeasureString(worldName).X * 2.0f / 2.0f;
            caption = new WavyText(worldName, new Vector2(titlex, 7), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);
            
            Globals.selectedLevel = -1;            
            
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
            Update_LevelSelect();
#if WINDOWS
            prevMouseState = currMouseState;
#endif       
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
            gameStateManager.ChangeState(Config.States.WORLDSELECT, true, Config.Menu.TRANSITION_DELAY);      
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
                        continue;

                    if (!Globals.worlds[Globals.selectedWorld].LevelActivated(clickedSquare))
                        continue;

                    SelectLevel(clickedSquare + currentPage * 10);
                }

            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                int clickedSquare = ClickedSquare(currMouseState.X, currMouseState.Y);
                
                if (clickedSquare < 0)
                    return;

                if (!Globals.worlds[Globals.selectedWorld].LevelActivated(clickedSquare))
                    return;

                SelectLevel(clickedSquare + currentPage * 10);
            }
#endif
        }

        private void PreviousPage()
        {
            if (--currentPage < 0)
                currentPage = (Globals.worlds[Globals.selectedWorld].levels.Count - 1) / 10;
        }

        private void NextPage()
        {
            if (++currentPage > ((Globals.worlds[Globals.selectedWorld].levels.Count - 1) / 10))
                currentPage = 0;
        }

        public void SelectLevel(int level)
        {
            if (level >= Globals.worlds[Globals.selectedWorld].levels.Count)
                return;

            Globals.selectedLevel = level;
            gameStateManager.ChangeState(Config.States.LEVELDETAILS, true, Config.Menu.TRANSITION_DELAY);
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (caption != null)
                caption.Draw(spriteBatch);
            int x, y;
            Color color = Color.White;

            var currentPageLevels = Globals.worlds[Globals.selectedWorld].levels.Count - currentPage * 10; // levels amount on current page
            var count = (currentPageLevels < 10 ? currentPageLevels : 10);

            for (int i = currentPage * 10; i < count + currentPage * 10; i++)
            {
                var posI = i - currentPage * 10;

                x = Config.Menu.OFFSET_X + (posI % Config.Menu.NUM_IN_ROW) * (levelTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (posI / Config.Menu.NUM_IN_ROW) * (levelTile.Height + Config.Menu.VERTICAL_SPACE);

                color = Globals.worlds[Globals.selectedWorld].LevelCompleted(i) ? Color.Green :
                        Globals.worlds[Globals.selectedWorld].LevelSkipped(i) ? Color.Gold :
                        Globals.worlds[Globals.selectedWorld].LevelActivated(i) ? Color.White : Color.Gray;

                spriteBatch.Draw(levelTile, new Vector2(x, y), color);

                var textOffset = Globals.silkscreenFont.MeasureString(Globals.worlds[Globals.selectedWorld].levels[i].shortName) / 2;
                spriteBatch.DrawString(Globals.silkscreenFont, Globals.worlds[Globals.selectedWorld].levels[i].shortName, new Vector2(x - textOffset.X + levelTile.Width / 2, y + levelTile.Height + Config.Menu.TEXT_SPACE), color);
            }
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
