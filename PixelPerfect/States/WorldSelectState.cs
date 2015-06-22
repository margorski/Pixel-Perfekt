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
    class WorldSelectState : GameState
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

        Texture2D worldTile;

        int currentPage = 0;

        Button sendButton;

        WavyText caption = new WavyText("WORLD SELECT", new Vector2(76, 7), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);

        public WorldSelectState(GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;

            worldTile = Globals.content.Load<Texture2D>("leveltile");

            sendButton = new Button("SEND", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 30, Config.SCREEN_HEIGHT_SCALED - 25, 60, 20), worldTile, Globals.silkscreenFont, false);
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

            World.RefreshWorldStatus(Globals.worlds);
            Globals.selectedWorld = -1;
                        
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

            caption.Update(gameTime);
#if WINDOWS
            currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
#endif
            Update_HandleBack();
            Update_WorldSelect();
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
            gameStateManager.ChangeState(Config.States.TITLESCREEN, true, 2000);
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

        public void Update_WorldSelect()
        {            
#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
#if DEBUG
                    if (sendButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale))
                    {
                        SendDataEmail();
                        continue;
                    }
#endif                 
                    int clickedSquare = ClickedSquare((int)touch.Position.X, (int)touch.Position.Y);

                    if (clickedSquare < 0)
                        continue;

                    if (!Globals.worlds[clickedSquare + currentPage * 10].active)
                        continue;

                    SelectWorld(clickedSquare + currentPage * 10);
                }

            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (sendButton.Clicked(currMouseState.X, currMouseState.Y, scale))
                {
                    SendDataEmail();
                    return;
                }

                int clickedSquare = ClickedSquare(currMouseState.X, currMouseState.Y);
                
                if (clickedSquare < 0 || (clickedSquare + currentPage * 10) >= Globals.worlds.Count)
                    return;

                if (!Globals.worlds[clickedSquare + currentPage * 10].active)
                    return;

                SelectWorld(clickedSquare + currentPage * 10);
            }
#endif
        }

        public void SelectWorld(int world)
        {
            if (world >= Globals.worlds.Count)
                return;

            Globals.selectedWorld = world;
            gameStateManager.ChangeState(Config.States.LEVELSELECT, true, 2000);
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            caption.Draw(spriteBatch);

#if DEBUG
            sendButton.Draw(spriteBatch);
#endif

            int x, y;
            Color color = Color.White;

            var currentWorldLevels = Globals.worlds.Count - currentPage * 10; 
            var count = (currentWorldLevels < 10 ? currentWorldLevels : 10);

            for (int i = currentPage * 10; i < count + currentPage * 10; i++)
            {
                var posI = i - currentPage * 10;

                x = Config.Menu.OFFSET_X + (posI % Config.Menu.NUM_IN_ROW) * (worldTile.Width + Config.Menu.HORIZONTAL_SPACE);
                y = Config.Menu.OFFSET_Y + (posI / Config.Menu.NUM_IN_ROW) * (worldTile.Height + Config.Menu.VERTICAL_SPACE);

                color = Globals.worlds[i].Completed() ? Color.Green :
                    Globals.worlds[i].active ? Color.White : Color.Gray;

                spriteBatch.Draw(worldTile, new Vector2(x, y), color);

                var textOffset = Globals.silkscreenFont.MeasureString(Globals.worlds[i].name) / 2;
                spriteBatch.DrawString(Globals.silkscreenFont, Globals.worlds[i].name, new Vector2(x - textOffset.X + worldTile.Width / 2, y + worldTile.Height + Config.Menu.TEXT_SPACE), color);
            }
        }


        public int ClickedSquare(int X, int Y)
        {
            X /= (int)scale;
            Y /= (int)scale;

            if ((Y) < Config.Menu.OFFSET_Y || (Y) > Config.Menu.OFFSET_Y + (worldTile.Height + Config.Menu.VERTICAL_SPACE) * 2)
                return -1;

            int x = (X - Config.Menu.OFFSET_X) / (worldTile.Width + Config.Menu.HORIZONTAL_SPACE); 
            int y = (Y - Config.Menu.OFFSET_Y) / (worldTile.Height + Config.Menu.VERTICAL_SPACE);

            return x + y * Config.Menu.NUM_IN_ROW;
        }
    }
}
