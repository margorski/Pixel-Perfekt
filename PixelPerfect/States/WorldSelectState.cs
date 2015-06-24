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
        enum MenuState
        {
            IDLE,
            CLICKING,
            SLIDING,
            FLOATING
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

        Texture2D worldTile;

        Button sendButton;
        List<Button> worldButtons = new List<Button>();

        int selectedWorld = 0;        
        MenuState state = MenuState.IDLE;

        TimeSpan clickTime = TimeSpan.Zero;

        WavyText caption = new WavyText("WORLD SELECT", new Vector2(76, 7), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);

        int touchId = -1;
        float startPositionX = 0.0f;

        float menuSlidingShift
        {
            get 
            {
                if (state == MenuState.SLIDING)
                {
                    float inputX = 0.0f;
#if WINDOWS
                    inputX = currMouseState.Position.X;
#else
                    TouchLocation tl;
                    touchState.FindById(touchId, out tl);
                    inputX = tl.Position.X;
#endif
                    return -(startPositionX - inputX) * Config.Menu.SLIDE_FACTOR;
                }
                else if (state == MenuState.FLOATING)
                {
                    return -startPositionX;
                }
                else
                {
                    return 0.0f;
                }
            }
        }
        public WorldSelectState(GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;

            worldTile = Globals.content.Load<Texture2D>("leveltile");

            foreach (World world in Globals.worlds)
            {
                var icon = Globals.textureDictionary[world.icon];
                worldButtons.Add(new Button("", new Rectangle(0, 0, icon.Width, icon.Height), icon, Globals.silkscreenFont, false));
            }
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

            for (int i = 0; i < worldButtons.Count; i++ )
            {
                worldButtons[i].active = Globals.worlds[i].active;                    
            }                

            if (Globals.musicEnabled && MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(Globals.backgroundMusicList[Globals.rnd.Next(Globals.backgroundMusicList.Count)]);
            selectedWorld = Globals.worlds.IndexOf(Globals.worlds.Last(world => world.active));
            state = MenuState.IDLE;
            touchId = -1;
        }

        public override void Exit(int nextStateId)
        {
        }

        public override void Resume(int poppedStateId)
        {
            touchId = -1;
            state = MenuState.IDLE;
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
            Update_WorldSelect(gameTime);
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
            gameStateManager.ChangeState(Config.States.TITLESCREEN, true, Config.Menu.TRANSITION_DELAY);
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

        public void Update_WorldSelect(GameTime gameTime)
        {            
#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touchId != -1 && touchId != touch.Id)
                    continue;

                switch (state)
                {
                    case MenuState.IDLE:
                        if (touch.State == TouchLocationState.Pressed)
                        {
                            touchId = touch.Id;
                            state = MenuState.CLICKING;
                            foreach (Button worldButton in worldButtons)
                            {
                                if (worldButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false))
                                    break;
                            }
                        }
                        break;

                    case MenuState.CLICKING:   
                        if (Math.Abs(touch.Position.X - startPositionX) > Config.Menu.INACTIVE_AREA)
                        {
                            foreach (Button worldButton in worldButtons)
                                worldButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true);                            
                            startPositionX = touch.Position.X;
                            state = MenuState.SLIDING;                        
                        }
                        else if (touch.State == TouchLocationState.Released)
                        {                        
                            foreach (Button worldButton in worldButtons)
                            {
                                if (worldButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                                {
                                    SelectWorld(worldButtons.IndexOf(worldButton));
                                    break;
                                }
                            }
                            if (sendButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                            {
                                SendDataEmail();
                                return;
                            }
                            touchId = -1;
                            state = MenuState.IDLE;
                        }
                        break;

                    case MenuState.SLIDING:
                        if (touch.State == TouchLocationState.Released)
                        {
                            if (touch.Position.X - startPositionX < -1.0f)
                            {
                                if (++selectedWorld > Globals.worlds.Count - 1)
                                {
                                    selectedWorld = Globals.worlds.Count - 1;
                                    startPositionX = (startPositionX - touch.Position.X) * Config.Menu.SLIDE_FACTOR;
                                }
                                else
                                {
                                    startPositionX = (startPositionX - touch.Position.X) * Config.Menu.SLIDE_FACTOR - (worldButtons[selectedWorld].Width + +Config.Menu.HORIZONTAL_SPACE);
                                }
                                state = MenuState.FLOATING;
                            }
                            else if (touch.Position.X - startPositionX > 1.0f)
                            {
                                if (--selectedWorld < 0)
                                {
                                    selectedWorld = 0;
                                    startPositionX = (startPositionX - touch.Position.X) * Config.Menu.SLIDE_FACTOR;
                                }
                                else
                                {
                                    startPositionX = (startPositionX - touch.Position.X) * Config.Menu.SLIDE_FACTOR + (worldButtons[selectedWorld].Width + Config.Menu.HORIZONTAL_SPACE);
                                }
                                state = MenuState.FLOATING;
                            }
                            else
                            {
                                state = MenuState.IDLE;
                            }
                            touchId = -1;
                        }
                        break;            

                }

                    
            }                
#else
            switch (state)
            {
                case MenuState.IDLE:
                    if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                    {
                        state = MenuState.CLICKING;
                        foreach (Button worldButton in worldButtons)
                        {
                            if (worldButton.Clicked(currMouseState.X, currMouseState.Y, scale, false))
                                break;
                        }
                    }
                    break;

                case MenuState.CLICKING:
                    clickTime += gameTime.ElapsedGameTime;
                    if (clickTime.TotalMilliseconds >= Config.Menu.CLICK_TIME)
                    {
                        startPositionX = currMouseState.Position.X;
                        state = MenuState.SLIDING;                        
                    }
                    else if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
                    {                        
                        foreach (Button worldButton in worldButtons)
                        {
                            if (worldButton.Clicked(currMouseState.X, currMouseState.Y, scale, true))
                                break;
                        }
                        if (sendButton.Clicked(currMouseState.X, currMouseState.Y, scale, true))
                        {
                            SendDataEmail();
                            return;
                        }
                        state = MenuState.IDLE;
                    }
                    break;

                case MenuState.SLIDING:
                    if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (currMouseState.Position.X - startPositionX < -10.0f)
                        {
                            if (++selectedWorld > Globals.worlds.Count - 1)
                            {
                                selectedWorld = Globals.worlds.Count - 1;
                                startPositionX = (startPositionX - currMouseState.X) * Config.Menu.SLIDE_FACTOR;
                            }
                            else
                            {
                                startPositionX = (startPositionX - currMouseState.X) * Config.Menu.SLIDE_FACTOR - (worldButtons[selectedWorld].Width + +Config.Menu.HORIZONTAL_SPACE);
                            }            
                            state = MenuState.FLOATING;
                        }
                        else if (currMouseState.Position.X - startPositionX > 10.0f)
                        {
                            if (--selectedWorld < 0)
                            {
                                selectedWorld = 0;
                                startPositionX = (startPositionX - currMouseState.X) * Config.Menu.SLIDE_FACTOR;
                            }
                            else
                            {
                                startPositionX = (startPositionX - currMouseState.X) * Config.Menu.SLIDE_FACTOR + (worldButtons[selectedWorld].Width + Config.Menu.HORIZONTAL_SPACE);
                            }            
                            state = MenuState.FLOATING;
                        }
                    }
                    break;            
            }            
#endif
            if (state == MenuState.FLOATING)
            {
                float timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                if (startPositionX > 0.0f)
                {
                    startPositionX -= timeFactor * Config.Menu.SLIDE_SPEED;
                    if (startPositionX < 0.0f)
                    {
                        startPositionX = 0.0f;
                        state = MenuState.IDLE;
                    }
                }
                else if (startPositionX < 0.0f)
                {
                    startPositionX += timeFactor * Config.Menu.SLIDE_SPEED;
                    if (startPositionX > 0.0f)
                    {
                        startPositionX = 0.0f;
                        state = MenuState.IDLE;
                    }
                }
            }    
        }

        public void SelectWorld(int world)
        {
            if (world >= Globals.worlds.Count)
                return;

            Globals.selectedWorld = world;
            gameStateManager.ChangeState(Config.States.LEVELSELECT, true, Config.Menu.TRANSITION_DELAY);
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            caption.Draw(spriteBatch);
#if DEBUG
            sendButton.Draw(spriteBatch);
#endif

            int x, y;
            Color color = Color.White;

            int centeringX = (Config.SCREEN_WIDTH_SCALED / 2 - worldButtons[selectedWorld].Width / 2) - selectedWorld * (worldButtons[selectedWorld].Width + Config.Menu.HORIZONTAL_SPACE);

            for (int i = 0; i < Globals.worlds.Count; i++)
            {
                x = centeringX + i * (worldButtons[i].Width + Config.Menu.HORIZONTAL_SPACE) + (int)menuSlidingShift;
                y = Config.Menu.OFFSET_Y;

                color = Globals.worlds[i].Completed() ? Color.Green : Color.White;                    
                
                worldButtons[i].setPosition(new Vector2(x, y));
                if (Globals.worlds[i].Completed())
                    worldButtons[i].Draw(spriteBatch, Color.Green);
                else
                    worldButtons[i].Draw(spriteBatch);


                if (!Globals.worlds[i].active)
                    spriteBatch.Draw(Globals.textureDictionary["keylock"], new Vector2(x, y), Color.White);

                var textOffset = Globals.silkscreenFont.MeasureString(Globals.worlds[i].name) / 2;
                spriteBatch.DrawString(Globals.silkscreenFont, Globals.worlds[i].name, new Vector2(x - textOffset.X + Globals.textureDictionary[Globals.worlds[i].icon].Width / 2, y + Globals.textureDictionary[Globals.worlds[i].icon].Height + Config.Menu.TEXT_SPACE), color);
            }
        }


        public int ClickedSquare(int X, int Y)
        {
            X /= (int)scale;
            Y /= (int)scale;

            if ((Y) < Config.Menu.OFFSET_Y || (Y) > Config.Menu.OFFSET_Y + (Globals.textureDictionary[Globals.worlds[0].icon].Height + Config.Menu.VERTICAL_SPACE) * 2)
                return -1;

            int x = (X - Config.Menu.OFFSET_X) / (Globals.textureDictionary[Globals.worlds[0].icon].Width + Config.Menu.HORIZONTAL_SPACE);
            int y = (Y - Config.Menu.OFFSET_Y) / (Globals.textureDictionary[Globals.worlds[0].icon].Height + Config.Menu.VERTICAL_SPACE);

            //return x + y * Config.Menu.NUM_IN_ROW;
            return -1;
        }
    }
}
