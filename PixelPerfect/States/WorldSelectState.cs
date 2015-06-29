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
            FLOATING,
            EXPLODE,
            EXPLODING
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
        Button backButton;
        List<Button> worldButtons = new List<Button>();

        int selectedWorld = 0;        
        MenuState state = MenuState.IDLE;

        TimeSpan clickTime = TimeSpan.Zero;

        WavyText caption = new WavyText("MOOD SELECT", new Vector2(76, 7), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);

#if !WINDOWS
        int touchId = -1;
#endif

        float startPositionX = 0.0f;
        Animation playerAnimation = new Animation(Config.ANIM_FRAMES, Config.DEFAULT_ANIMATION_SPEED, true);
        TimeSpan boomDelay = TimeSpan.Zero;
        TimeSpan boomColorTime = TimeSpan.Zero;
        int boomColorIndex = 0;

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
            backButton = new Button("", new Rectangle(Config.Menu.BACK_X, Config.Menu.BACK_Y, 24, 24), Globals.textureDictionary["back"], Globals.silkscreenFont, false);
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

            state = MenuState.FLOATING;
            startPositionX = -(selectedWorld + 1) * (worldButtons[0].Width + Config.Menu.WORLD_HORIZONTAL_SPACE);
            Globals.selectedWorld = -1;
            if (Globals.detonateWorldKeylock > -1)
                selectedWorld = Globals.detonateWorldKeylock;
            else
                selectedWorld = Globals.worlds.IndexOf(Globals.worlds.Last(world => world.active));

            for (int i = 0; i < worldButtons.Count; i++ )
            {
                if (Globals.detonateWorldKeylock == i)
                    continue;
                worldButtons[i].active = Globals.worlds[i].active;                    
            }                
                        
            if (Globals.musicEnabled && MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(Globals.backgroundMusicList[Theme.CurrentTheme.music]);       
#if !WINDOWS     
            touchId = -1;
#endif
        }

        private void PixelExplosion(int world)
        {
            int centeringX = (Config.SCREEN_WIDTH_SCALED / 2 - worldButtons[world].Width / 2) - selectedWorld * (worldButtons[world].Width + Config.Menu.WORLD_HORIZONTAL_SPACE);

            var keylockTexture = Globals.textureDictionary["keylock"];
            var textureArray = Util.GetTextureArray(keylockTexture, keylockTexture.Width, keylockTexture.Height);
            var position = new Vector2(centeringX + world * (worldButtons[world].Width + Config.Menu.WORLD_HORIZONTAL_SPACE),
                                       Config.Menu.WORLD_OFFSET_Y);

            for (int i = 0; i < textureArray.Length; i++)
            {
                if (textureArray[i].A == 255)
                {
                    Vector2 boomCenter = position + new Vector2(keylockTexture.Width / 2, keylockTexture.Height / 2);
                    Vector2 pixPos = position + new Vector2(i % keylockTexture.Width, i / keylockTexture.Width);
                    Vector2 pixSpeed = (pixPos - boomCenter) * Globals.rnd.Next(0, Config.PixelParticle.MAX_EXPLOSION_MAGNITUDE);
                    Vector2 acc = Vector2.Zero;// new Vector2(rnd.Next(-100, 100), rnd.Next(-100, 100));

                    Globals.pixelParticles.Add(new PixelParticle(pixPos,
                                    0.0f,//Config.PixelParticle.PIXELPARTICLE_PLAYER_LIFETIME_MAX,
                                    pixSpeed, acc, Config.boomColors[Globals.rnd.Next(Config.boomColors.Length)], true, null));
                }
            }
            if (Globals.soundEnabled)
                Globals.soundsDictionary["explosion"].Play();
        }

        public override void Exit(int nextStateId)
        {
        }

        public override void Resume(int poppedStateId)
        {
#if !WINDOWS
            touchId = -1;
#endif
            state = MenuState.IDLE;
        }

        public override void Suspend(int pushedStateId)
        {            
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;
            
            playerAnimation.Update(gameTime);            
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
                            startPositionX = touch.Position.X;
                            touchId = touch.Id;
                            state = MenuState.CLICKING;
                            foreach (Button worldButton in worldButtons)
                            {
                                if (worldButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false))
                                    break;
                            }
                            sendButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                            backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);    
                        }
                        break;

                    case MenuState.CLICKING:   
                        if (Math.Abs(touch.Position.X - startPositionX) > Config.Menu.INACTIVE_AREA)
                        {
                            foreach (Button worldButton in worldButtons)
                                worldButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true);
                            sendButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true);
                            backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true);          
                            state = MenuState.SLIDING;                        
                        }
                        else if (touch.State == TouchLocationState.Released)
                        {                        
                            foreach (Button worldButton in worldButtons)
                            {
                                if (worldButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                                {
                                    SelectWorld(worldButtons.IndexOf(worldButton));
                                }
                            }
                            if (sendButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                            {
                                SendDataEmail();
                            }
                            else if (backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                            {
                                GoBack();
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
                                    startPositionX = (startPositionX - touch.Position.X) * Config.Menu.SLIDE_FACTOR - (worldButtons[selectedWorld].Width + +Config.Menu.WORLD_HORIZONTAL_SPACE);
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
                                    startPositionX = (startPositionX - touch.Position.X) * Config.Menu.SLIDE_FACTOR + (worldButtons[selectedWorld].Width + Config.Menu.WORLD_HORIZONTAL_SPACE);
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
                        startPositionX = currMouseState.Position.X;
                        foreach (Button worldButton in worldButtons)
                        {
                            if (worldButton.Clicked(currMouseState.X, currMouseState.Y, scale, false))
                                break;

                        }
                        sendButton.Clicked(currMouseState.X, currMouseState.Y, scale, false);
                        backButton.Clicked(currMouseState.X, currMouseState.Y, scale, false);
                    }
                    break;

                case MenuState.CLICKING:
                    if (Math.Abs(currMouseState.Position.X - startPositionX) > Config.Menu.INACTIVE_AREA)
                    {
                        foreach (Button worldButton in worldButtons)
                            worldButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true);
                        sendButton.Clicked(currMouseState.X, currMouseState.Y, scale, true);
                        backButton.Clicked(currMouseState.X, currMouseState.Y, scale, true);
                        state = MenuState.SLIDING;                        
                    }
                    else if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
                    {                        
                        foreach (Button worldButton in worldButtons)
                        {
                            if (worldButton.Clicked(currMouseState.X, currMouseState.Y, scale, true))
                            {
                                SelectWorld(worldButtons.IndexOf(worldButton));
                            }
                        }
                        if (sendButton.Clicked(currMouseState.X, currMouseState.Y, scale, true))
                        {
                            SendDataEmail();
                            return;
                        }
                        else if (backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                        {
                            GoBack();
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
                                startPositionX = (startPositionX - currMouseState.X) * Config.Menu.SLIDE_FACTOR - (worldButtons[selectedWorld].Width + Config.Menu.WORLD_HORIZONTAL_SPACE);
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
                                startPositionX = (startPositionX - currMouseState.X) * Config.Menu.SLIDE_FACTOR + (worldButtons[selectedWorld].Width + Config.Menu.WORLD_HORIZONTAL_SPACE);                                                            
                            }
                            state = MenuState.FLOATING;
                        }
                        else
                        {
                            state = MenuState.IDLE;
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
                        if (Globals.detonateWorldKeylock > -1)
                            state = MenuState.EXPLODE;
                        else
                            state = MenuState.IDLE;                        
                    }
                }                
                else if (startPositionX <= 0.0f)
                {
                    startPositionX += timeFactor * Config.Menu.SLIDE_SPEED;
                    if (startPositionX > 0.0f)
                    {
                        startPositionX = 0.0f;
                        if (Globals.detonateWorldKeylock > -1)                        
                            state = MenuState.EXPLODE;
                        else
                            state = MenuState.IDLE;                        
                    }
                }
            }
            else if (state == MenuState.EXPLODE)
            {
                boomDelay += gameTime.ElapsedGameTime;
                if (boomDelay.TotalMilliseconds > 500)
                {
                    boomDelay = TimeSpan.Zero;
                    if (Globals.soundEnabled)
                        Globals.soundsDictionary["randomize"].Play();
                    state = MenuState.EXPLODING;
                }
            }
            else if (state == MenuState.EXPLODING)
            {
                boomColorTime += gameTime.ElapsedGameTime;
                if (boomColorTime.TotalMilliseconds > Config.Player.BOOMCOLOR_TIME_MS)
                {
                    boomColorTime = TimeSpan.Zero;
                    if (++boomColorIndex >= Config.boomColors.Length)
                    {
                        PixelExplosion(Globals.detonateWorldKeylock);
                        worldButtons[Globals.detonateWorldKeylock].active = true;
                        Globals.detonateWorldKeylock = -1;
                        boomColorIndex = 0;
                        state = MenuState.IDLE;
                    }
                }
                return;
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
            backButton.Draw(spriteBatch);
#if DEBUG
            sendButton.Draw(spriteBatch);
#endif

            int x, y;
            Color color = Color.White;

            int centeringX = (Config.SCREEN_WIDTH_SCALED / 2 - worldButtons[selectedWorld].Width / 2) - selectedWorld * (worldButtons[selectedWorld].Width + Config.Menu.WORLD_HORIZONTAL_SPACE);

            for (int i = 0; i < Globals.worlds.Count; i++)
            {
                x = centeringX + i * (worldButtons[i].Width + Config.Menu.WORLD_HORIZONTAL_SPACE) + (int)menuSlidingShift;
                y = Config.Menu.WORLD_OFFSET_Y;

                color = Globals.worlds[i].Completed() ? Color.Gold: Color.White;                                   
                
                worldButtons[i].setPosition(new Vector2(x, y));
                if (color != Color.White)
                    worldButtons[i].Draw(spriteBatch, color);
                else
                    worldButtons[i].Draw(spriteBatch);


                if (!Globals.worlds[i].active || i == Globals.detonateWorldKeylock)
                {
                    Color keylockColor = Color.White;
                    if (state == MenuState.EXPLODING && i == Globals.detonateWorldKeylock)
                        keylockColor = Config.boomColors[boomColorIndex];

                    spriteBatch.Draw(Globals.textureDictionary["keylock"], new Vector2(x, y), keylockColor);
                }

                var textOffset = Globals.silkscreenFont.MeasureString(Globals.worlds[i].name) / 2;
                spriteBatch.DrawString(Globals.silkscreenFont, Globals.worlds[i].name, new Vector2(x - textOffset.X + Globals.textureDictionary[Globals.worlds[i].icon].Width / 2, y + Globals.textureDictionary[Globals.worlds[i].icon].Height + Config.Menu.TEXT_SPACE), color);
                if (Globals.worlds[i].BeatWorldPerfektTime())
                    spriteBatch.Draw(Globals.textureDictionary["trophy"], new Vector2(x, y), Color.White);

                spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - (Config.Menu.BOINGS_SIZE * 5 + Config.Menu.BOINGS_SPACE * 4) / 2 + (Config.Menu.BOINGS_SIZE + Config.Menu.BOINGS_SPACE) * i, Config.Menu.BOINGS_OFFSET_Y, Config.Menu.BOINGS_SIZE, Config.Menu.BOINGS_SIZE), (i == selectedWorld ? Color.Blue : Color.White));
                for (int j = 0; j < Config.Menu.SMALL_BOINGS_QTY; j++)
                {
                    if (i == Globals.worlds.Count - 1)
                        break;

                    float smallBoingX = Config.SCREEN_WIDTH_SCALED / 2 - (Config.Menu.BOINGS_SIZE * 5 + Config.Menu.BOINGS_SPACE * 4) / 2 + (Config.Menu.BOINGS_SIZE + Config.Menu.BOINGS_SPACE) * i + Config.Menu.BOINGS_SIZE / 2; // boing base
                    smallBoingX += (Config.Menu.BOINGS_SIZE + Config.Menu.BOINGS_SPACE) / (Config.Menu.SMALL_BOINGS_QTY + 1) * (j + 1);
                    spriteBatch.Draw(Globals.textureDictionary["pixel"], new Vector2(smallBoingX, Config.Menu.BOINGS_OFFSET_Y + 1), Color.White);
                }
            }
            float progressBetweenLevels = - (menuSlidingShift / (worldButtons[selectedWorld].Width + Config.Menu.WORLD_HORIZONTAL_SPACE));
            if (progressBetweenLevels > 1.0f)
                progressBetweenLevels = 1.0f;
            else if (progressBetweenLevels < -1.0f)
                progressBetweenLevels = -1.0f;
            if (selectedWorld == 0 && progressBetweenLevels < 0.0f)
                progressBetweenLevels = 0.0f;
            else if (selectedWorld == Globals.worlds.Count - 1 && progressBetweenLevels > 0.0f)
                progressBetweenLevels = 0.0f;

            bool walking = (state == MenuState.FLOATING || state == MenuState.SLIDING);
            spriteBatch.Draw(Globals.spritesDictionary["player"].texture,
                             new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - (Config.Menu.BOINGS_SIZE * 5 + Config.Menu.BOINGS_SPACE * 4) / 2 - 3 + (Config.Menu.BOINGS_SIZE + Config.Menu.BOINGS_SPACE) * selectedWorld + (int)(progressBetweenLevels * (Config.Menu.BOINGS_SIZE + Config.Menu.BOINGS_SPACE)), 
                                          Config.Menu.BOINGS_OFFSET_Y - 20, 8, 16), new Rectangle(0, walking ? (playerAnimation.GetCurrentFrame() + 1) * 16 : 0, 8, 16), Color.White);
        }
    }
}
