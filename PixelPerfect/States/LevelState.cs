using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#if ANDROID
using IsolatedStorageSettings = CustomIsolatedStorageSettings.IsolatedStorageSettings;
#endif

namespace PixelPerfect
{
    class LevelState : GameState
    {
        public class LevelColors
        {
            public int color1 = 11;
            public int color2 = 135;
            public int hudcolor = 13;
            public int enemycolor = 3;
            public int emmitercolor = 3;
            public int tilecolor = 3;
        }
        
        private Map map;        
        private Player player;
        private Hud hud;
        public List<PixelParticle> pixelParticles = new List<PixelParticle>();
#if !WINDOWS
        //private TouchCollection touchCollection;
        private int touchId = 0;
        private TimeSpan adTimer = TimeSpan.FromSeconds(Config.AD_TIMER);
#else
        private MouseState previousMouseState;
        private MouseState currentMouseState;
        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;
#endif
        private GamePadState prevGPState;
        private GamePadState currGPState;
        public int deathCount;
        //private int i = 0;
        public string name = "";

        public string levelFile { private set; get; }
        private string directory = "";

        public TimeSpan levelTime { private set; get; }
        
        
        private Texture2D backgroundTexture = Util.GetGradientTexture(1, Config.SCREEN_HEIGHT_SCALED, Color.MidnightBlue, Color.DarkSlateBlue, Util.GradientType.Horizontal);

        private LevelColors levelColors = new LevelColors();
        private bool colors = false;
        private bool menuLevel = false;
        private bool doorGravity = false;
        private TimeSpan endDelay = TimeSpan.Zero;
#if WINDOWS
        #region CHANGE_COLORS
        private void PreviousColor1()
        {
            if (--levelColors.color1 < 0)
                levelColors.color1 = Globals.colorList.Count - 1;
            ReloadGradientTexture();
        }
        private void NextColor1()
        {
            if (++levelColors.color1 > Globals.colorList.Count - 1)
                levelColors.color1 = 0;
            ReloadGradientTexture();            
        }

        private void NextColor2()
        {
            if (++levelColors.color2 > Globals.colorList.Count - 1)
                levelColors.color2 = 0;
            ReloadGradientTexture();
        }
        private void PreviousColor2()
        {
            if (--levelColors.color2 < 0)
                levelColors.color2 = Globals.colorList.Count - 1;
            ReloadGradientTexture();
        }
        private void NextMusic()
        {
            if (++map.music > Globals.backgroundMusicList.Count - 1)
                map.music = 0;
            ReloadMusic();
        }
        private void PreviousMusic()
        {
            if (--map.music < 0)
                map.music = Globals.backgroundMusicList.Count - 1;
            ReloadMusic();
        }

        private void ReloadMusic()
        {
            //MediaPlayer.Stop();
            if (Globals.musicEnabled)
                MediaPlayer.Play(Globals.backgroundMusicList[map.music]);
        }

        private void SwapGradientColors()
        {
            var temp = levelColors.color1;
            levelColors.color1 = levelColors.color2;
            levelColors.color2 = temp;
            ReloadGradientTexture();
        }
        
        private void SwapEnemiesEmitersColor()
        {
            var temp = levelColors.emmitercolor;
            levelColors.emmitercolor = levelColors.enemycolor;
            levelColors.enemycolor = temp;
            RefreshColors();
        }

        private void EmiterasEnemies()
        {
            levelColors.emmitercolor = levelColors.enemycolor;
            RefreshColors();
        }

        private void EmiterEnemiesAsTiles()
        {
            levelColors.emmitercolor = levelColors.enemycolor = levelColors.tilecolor;
            RefreshColors();
        }

        private void HudAsTiles()
        {
            levelColors.hudcolor = levelColors.tilecolor;
            RefreshColors();
        }
        
        private void NextHudColor()
        {
            if (++levelColors.hudcolor > Globals.colorList.Count - 1)
                levelColors.hudcolor = 0;
            if (!menuLevel)
                hud.SetColor(Globals.colorList[levelColors.hudcolor]);
        }
        private void PreviousHudColor()
        {
            if (--levelColors.hudcolor < 0)
                levelColors.hudcolor = Globals.colorList.Count - 1;
            if (!menuLevel)
                hud.SetColor(Globals.colorList[levelColors.hudcolor]);
        }

        private void NextEnemyColor()
        {
            if (++levelColors.enemycolor > Globals.colorList.Count - 1)
                levelColors.enemycolor = 0;
            Globals.enemiesColor = Globals.colorList[levelColors.enemycolor];
        }
        private void PreviousEnemyColor()
        {
            if (--levelColors.enemycolor < 0)
                levelColors.enemycolor = Globals.colorList.Count - 1;
            Globals.enemiesColor = Globals.colorList[levelColors.enemycolor];
        }
        private void NextEmitersColor()
        {
            if (++levelColors.emmitercolor > Globals.colorList.Count - 1)
                levelColors.emmitercolor = 0;
            Globals.emitersColor = Globals.colorList[levelColors.emmitercolor];
        }
        private void PreviousEmitersColor()
        {
            if (--levelColors.emmitercolor < 0)
                levelColors.emmitercolor = Globals.colorList.Count - 1;
            Globals.emitersColor = Globals.colorList[levelColors.emmitercolor];
        }
        private void NextTilesColor()
        {
            if (++levelColors.tilecolor > Globals.colorList.Count - 1)
                levelColors.tilecolor = 0;
            Globals.tilesColor = Globals.colorList[levelColors.tilecolor];
        }
        private void PreviousTilesColor()
        {
            if (--levelColors.tilecolor < 0)
                levelColors.tilecolor = Globals.colorList.Count - 1;
            Globals.tilesColor = Globals.colorList[levelColors.tilecolor];
        }
        #endregion
#endif

        private void ReloadGradientTexture()
        {
            if (levelColors.color1 < 0 || levelColors.color1 > Globals.colorList.Count - 1 || levelColors.color2 < 0 || levelColors.color2 > Globals.colorList.Count - 1)
                return;

            backgroundTexture = Util.GetGradientTexture(1, Config.SCREEN_HEIGHT_SCALED, Globals.colorList[levelColors.color1], Globals.colorList[levelColors.color2], Util.GradientType.Horizontal);
        }

        private void RefreshColors()
        {
            Globals.tilesColor = Globals.colorList[levelColors.tilecolor];
            Globals.emitersColor = Globals.colorList[levelColors.emmitercolor];
            Globals.enemiesColor = Globals.colorList[levelColors.enemycolor];
            if (!menuLevel)
                hud.SetColor(Globals.colorList[levelColors.hudcolor]);
            ReloadGradientTexture();
        }

        public LevelState(String directory, String levelFile, bool menuLevel, Vector2 scale)
        {
            this.levelFile = levelFile;
            this.directory = directory;
            if (levelFile.Length > 4)
            {
                if (!levelFile.Substring(levelFile.Length - 4, 4).ToLower().Equals(".tmx"))
                    levelFile += ".tmx";
            }
            Globals.graphics.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
            this.menuLevel = menuLevel;
            this.scale = scale;
        }

        void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            if (!menuLevel)
                ReloadGradientTexture();
        }

        public override void Enter(int previousStateId)
        {
            InitLevel();
            ResetInput();
            //if (!menuLevel)
            //    Reset(true);

            if (!menuLevel)
            {
                if (Globals.selectedLevel == 0 && Globals.selectedWorld == 0)
                {
                    Globals.gameStateManager.PushState(Config.States.CONTROLS);
                    if (!Globals.firstcutscene)
                    {
                        Globals.firstcutscene = true;
#if !WINDOWS
                        IsolatedStorageSettings.ApplicationSettings["firstcutscene"] = Globals.firstcutscene;
                        IsolatedStorageSettings.ApplicationSettings.Save();
                        Globals.gameStateManager.PushState(Config.States.FIRST_CUTSCENE);
#endif
                    }
                }
                else
                    Globals.gameStateManager.PushState(Config.States.TAP);

                if (Globals.musicEnabled && !menuLevel)
                    MediaPlayer.Play(Globals.backgroundMusicList[map.music]);
            }
        }

        public override void Exit(int nextStateId)
        {
            Globals.backgroundColor = Color.Black;
            foreach (KeyValuePair<string, SoundEffectInstance> sfinstance in Globals.soundsDictionary)
                sfinstance.Value.Stop();
            if (!menuLevel)
                MediaPlayer.Stop();

            if (!menuLevel)
                Util.AdsOff();
        }

        public override void Resume(int poppedStateId)
        {
            ResetInput();
            if (Globals.musicEnabled)
                MediaPlayer.Resume();
            if (Globals.soundEnabled && Globals.soundsDictionary["doors"].State == SoundState.Paused)
                Globals.soundsDictionary["doors"].Resume();
            if (menuLevel)
            {
                InitLevel();                
            }
        }

        public override void Suspend(int pushedStateId)
        {         
            /*
            if (!menuLevel && pushedStateId != Config.States.TAP)
            {
                Util.AdsOff();
                if (adTimer == TimeSpan.Zero)
                    adTimer = TimeSpan.FromMilliseconds(1.0);
            }
            */
            if (menuLevel)
                InitLevel();
            else
            {
                if (Globals.musicEnabled && pushedStateId == -1)
                    MediaPlayer.Pause();                
                if (Globals.soundEnabled && Globals.soundsDictionary["doors"].State == SoundState.Playing)
                    Globals.soundsDictionary["doors"].Pause();
            }
        }

        public override void Update(GameTime gameTime, bool suspended)
        {            
            if (suspended)            
                return;
			/*
            if (adTimer > TimeSpan.Zero && !menuLevel)
            {
                adTimer -= gameTime.ElapsedGameTime;
                if (adTimer <= TimeSpan.Zero)
                {
                    adTimer = TimeSpan.Zero;
                    Util.AdsOn();
                }
            }
            */
            if (!menuLevel)
            {
                currGPState = GamePad.GetState(PlayerIndex.One);
                if ((currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released))
                {
                    Globals.gameStateManager.PushState(Config.States.PAUSE);
                }
                prevGPState = currGPState;

                if (doorGravity)
                    GoPixelsToDoors();
                else
                    StopPixels();
            }
#if !WINDOWS
            if (!menuLevel)
                TouchInput(gameTime, TouchPanel.GetState());
#else
            MouseInput(gameTime);
            if (!menuLevel)
                KeyboardInput(gameTime);
#endif

            for (int i = 0; i < pixelParticles.Count; i++)
            {
                if (pixelParticles[i].Update(gameTime))
                {
                    pixelParticles.RemoveAt(i);
                    i--;
                }
            }

            player.Update(gameTime);
            
            if (!player.GetState(Player.State.dead) && !player.GetState(Player.State.dying) && !player.GetState(Player.State.hiding) 
                && !player.GetState(Player.State.hidden) && !player.GetState(Player.State.entered))
            {
                levelTime += gameTime.ElapsedGameTime;
                float movingModifier = 0.0f;
                bool springy = false;
                Rectangle tempRectangle;
                player.MoveHorizontally(gameTime);

                if (map.CheckCollisions(player.boundingBox, Tile.Attributes.Solid, out tempRectangle))
                {
                    player.HitTheWall(tempRectangle);
                }
                else if (!player.GetState(Player.State.jumping) && !player.GetState(Player.State.falling))
                {
                    // hack for curshy platforms to go up one pixel
                    var playerBoxMovedUp = player.boundingBox;
                    playerBoxMovedUp.Y -= 1;
                    if (map.CheckPlatformCollisions(playerBoxMovedUp, out tempRectangle, out movingModifier, out springy)) // crushy platform 1 pixel crushed
                        player.HitTheGround(tempRectangle);
                }
                player.ResetMovingPlatformState();

                player.MoveVertically(gameTime);
                if (map.CheckCollisions(player.boundingBox, Tile.Attributes.Solid, out tempRectangle))
                {
                    if (player.speed.Y > 0.0f)
                        player.HitTheGround(tempRectangle);
                    else
                        player.HitTheCeiling(tempRectangle);
                }
                else if (player.speed.Y > 0.0f && map.CheckPlatformCollisions(player.boundingBox, out tempRectangle, out movingModifier, out springy))
                {
                    player.HitTheGround(tempRectangle);
                    if (springy)
                    {
                        if (player.Jump(true) && Globals.soundEnabled)
                            Globals.soundsDictionary["jump"].Play();                        
                    }
                }
                else
                {
                    if (player.speed.Y > 0.0f && !player.GetState(Player.State.jumping))
                    {
                        var playerBoxMovedDown = player.boundingBox;
                        playerBoxMovedDown.Y += 1;

                        if (!map.CheckCollisions(playerBoxMovedDown, Tile.Attributes.Solid, out tempRectangle) &&  // check if there is no collision from bottom
                            !map.CheckPlatformCollisions(playerBoxMovedDown, out tempRectangle, out movingModifier, out springy))
                        {
                            playerBoxMovedDown.Y += 1; // for crushy platforms hack
                            if (map.CheckPlatformCollisions(playerBoxMovedDown, out tempRectangle, out movingModifier, out springy)) // crushy platform 1 pixel crushed
                                player.HitTheGround(tempRectangle);
                            else // falling
                            {
                                player.SetState(Player.State.jumping, true);
                                player.SetState(Player.State.falling, true);
                                player.jumpY = player.boundingBox.Y;
                            }
                        }
                    }
                }

                player.SetMovingPlatformState(movingModifier);

                map.CheckTriggers(player.boundingBox);

                if (map.GrabCollectibles(player, Globals.graphics))
                {
                    if (!menuLevel)
                        hud.Collect();
                    if (map.collectiblesCount == 0)
                        doorGravity = true;
                    if (Globals.soundEnabled)
                        Globals.soundsDictionary["coin"].Play();
                }

                if (map.KillThisBastard(player, Globals.graphics))
                {
                    player.Die();
                }

                if (map.EnteredDoors(player.boundingBox))
                {
                    player.SetState(Player.State.hiding, true);
                }
            }
            else if (player.GetState(Player.State.hidden))
            {
                GoPixelsToDoors();
                player.SetState(Player.State.hidden, false);
                player.SetState(Player.State.entered, true);
            }
            else if (player.GetState(Player.State.entered))
            {
                endDelay += gameTime.ElapsedGameTime;
                if (endDelay.TotalMilliseconds > Config.LEVEL_END_DELAY)
                {
                    Globals.soundsDictionary["doors"].Stop();
                    ((WinState)Globals.gameStateManager.GetState(Config.States.WIN)).SetStats(LevelId(), levelTime, deathCount);
                    Globals.gameStateManager.PushState(Config.States.WIN, true);
                    return;
                }
            }
            map.Update(gameTime);

            if (!menuLevel)
                hud.Update(gameTime);
        }

        private void GoPixelsToDoors()
        {
            var doorCenter = map.GetDoorCenter();
            doorCenter.X += 4;
            doorCenter.Y += 4;            
            foreach (PixelParticle pixel in pixelParticles.Where(p => !p.gravityPointEnabled))
            {
                pixel.SetGravityPoint(doorCenter, Config.BLACK_HOLE_FORCE);
            }
        }

        private void StopPixels()
        {
            foreach (PixelParticle pixel in pixelParticles.Where(p => p.gravityPointEnabled))
            {
                pixel.ClearGravityPoint();
            }
        }
#if WINDOWS
        private void MouseInput(GameTime gameTime)
        {
 	        currentMouseState = Mouse.GetState();
            
            if (player.GetState(Player.State.dead))
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)                
                    Reset();
            }
            else if (player.GetState(Player.State.dying))
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    player.SetState(Player.State.dead, true);
                    Reset();
                }
            }
            else if (!player.GetState(Player.State.hiding) && !player.GetState(Player.State.hidden) && !player.GetState(Player.State.entered))
            {
                if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed) // mouse button released
                {
                    player.EndOfStop(gameTime);
                }
                else if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released) // mouse button pressed
                {
                    var leftScreenHalf = new Rectangle(0, 0, (int)(Config.SCREEN_WIDTH_SCALED * scale.X / 2),
                                                             (int)(Config.SCREEN_HEIGHT_SCALED * scale.Y));
                    var rightScreenHalf = new Rectangle((int)(Config.SCREEN_WIDTH_SCALED * scale.X / 2), 0,
                                                        (int)(Config.SCREEN_WIDTH_SCALED * scale.X / 2), (int)(Config.SCREEN_HEIGHT_SCALED * scale.Y));
                    var mousePosition = new Point((int)currentMouseState.X, (int)currentMouseState.Y);

                    if (!Globals.swappedControls)
                    {
                        if (leftScreenHalf.Contains(mousePosition))
                            player.Stop(gameTime);
                        else if (rightScreenHalf.Contains(mousePosition))
                        {
                            if (player.Jump() && Globals.soundEnabled)
                                Globals.soundsDictionary["jump"].Play();
                        }
                    }
                    else
                    {
                        if (rightScreenHalf.Contains(mousePosition))
                            player.Stop(gameTime);
                        else if (leftScreenHalf.Contains(mousePosition))
                        {
                            if (player.Jump() && Globals.soundEnabled)
                                Globals.soundsDictionary["jump"].Play();
                        }
                    }
                }
            }
            previousMouseState = currentMouseState;
        }

        private void KeyboardInput(GameTime gameTime)
        {
            currentKeyboardState = Keyboard.GetState();

            #region CHANGE_COLORS
            // DEBUGGGGG
            // color1
            if (currentKeyboardState.IsKeyDown(Keys.A) && previousKeyboardState.IsKeyUp(Keys.A))            
                PreviousColor1();
            if (currentKeyboardState.IsKeyDown(Keys.Q) && previousKeyboardState.IsKeyUp(Keys.Q))
                NextColor1();
            // color2
            if (currentKeyboardState.IsKeyDown(Keys.S) && previousKeyboardState.IsKeyUp(Keys.S))
                PreviousColor2();
            if (currentKeyboardState.IsKeyDown(Keys.W) && previousKeyboardState.IsKeyUp(Keys.W))
                NextColor2();
            //hud
            if (currentKeyboardState.IsKeyDown(Keys.D) && previousKeyboardState.IsKeyUp(Keys.D))
                PreviousHudColor();
            if (currentKeyboardState.IsKeyDown(Keys.E) && previousKeyboardState.IsKeyUp(Keys.E))
                NextHudColor();
            //enemy
            if (currentKeyboardState.IsKeyDown(Keys.F) && previousKeyboardState.IsKeyUp(Keys.F))
                PreviousEnemyColor();
            if (currentKeyboardState.IsKeyDown(Keys.R) && previousKeyboardState.IsKeyUp(Keys.R))
                NextEnemyColor();
            //emiter
            if (currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G))
                PreviousEmitersColor();
            if (currentKeyboardState.IsKeyDown(Keys.T) && previousKeyboardState.IsKeyUp(Keys.T))
                NextEmitersColor();
            //tiles
            if (currentKeyboardState.IsKeyDown(Keys.H) && previousKeyboardState.IsKeyUp(Keys.H))
                PreviousTilesColor();
            if (currentKeyboardState.IsKeyDown(Keys.Y) && previousKeyboardState.IsKeyUp(Keys.Y))
                NextTilesColor();
            // swaps
            if (currentKeyboardState.IsKeyDown(Keys.Z) && previousKeyboardState.IsKeyUp(Keys.Z))
                SwapGradientColors();
            if (currentKeyboardState.IsKeyDown(Keys.X) && previousKeyboardState.IsKeyUp(Keys.X))
                SwapEnemiesEmitersColor();
            // same colors
            if (currentKeyboardState.IsKeyDown(Keys.D1) && previousKeyboardState.IsKeyUp(Keys.D1))
                HudAsTiles();
            if (currentKeyboardState.IsKeyDown(Keys.D2) && previousKeyboardState.IsKeyUp(Keys.D2))
                EmiterEnemiesAsTiles();
            if (currentKeyboardState.IsKeyDown(Keys.D3) && previousKeyboardState.IsKeyUp(Keys.D3))
                EmiterasEnemies(); 
            // colors printing
            colors = currentKeyboardState.IsKeyDown(Keys.Tab);     
           // music changing
            if (currentKeyboardState.IsKeyDown(Keys.N) && previousKeyboardState.IsKeyUp(Keys.N))
                PreviousMusic();
            if (currentKeyboardState.IsKeyDown(Keys.M) && previousKeyboardState.IsKeyUp(Keys.M))
                NextMusic();
            if (currentKeyboardState.IsKeyDown(Keys.F10) && previousKeyboardState.IsKeyUp(Keys.F10))
                Globals.graphics.ToggleFullScreen();
            // END DEBUGG
            #endregion
            if ((currentKeyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape)))
                Globals.gameStateManager.PushState(Config.States.PAUSE);

            if (player.GetState(Player.State.dead))
            {
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                    Reset();
            }            
            else if (player.GetState(Player.State.dying))
            {
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                {
                    player.SetState(Player.State.dead, true);
                    Reset();
                }
            }
            else
            {
                if (currentKeyboardState.IsKeyUp(Keys.LeftShift) && previousKeyboardState.IsKeyDown(Keys.LeftShift)) // left shift
                {
                    player.EndOfStop(gameTime);
                }
                else if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && previousKeyboardState.IsKeyUp(Keys.LeftShift))
                {
                    player.Stop(gameTime);
                }

                if (currentKeyboardState.IsKeyDown(Keys.RightShift) && previousKeyboardState.IsKeyUp(Keys.RightShift)) // right shift
                {
                    if (player.Jump() && Globals.soundEnabled)
                        Globals.soundsDictionary["jump"].Play();
                }
            }
            previousKeyboardState = currentKeyboardState;
        }
#else
        public void TouchInput(GameTime gameTime, TouchCollection touchCollection)
        {
            foreach (TouchLocation tl in touchCollection)
            {
                if (player.GetState(Player.State.dead))
                {
                    if (tl.State == TouchLocationState.Pressed)
                        Reset();
                }                
                else if (player.GetState(Player.State.dying))
                {
                    if (tl.State == TouchLocationState.Pressed)
                    {
                        player.SetState(Player.State.dead, true);
                        Reset();
                    }
                }
                else
                {
                    if (tl.State == TouchLocationState.Released && tl.Id == touchId)
                    {
                        touchId = 0;
                        player.EndOfStop(gameTime);
                    }
                    else if (tl.State == TouchLocationState.Pressed)
                    {
                        var leftScreenHalf = new Rectangle(0, 0, (int)(Config.SCREEN_WIDTH_SCALED * scale.X / 2),
                                                            (int)(Config.SCREEN_HEIGHT_SCALED * scale.Y));
                        var rightScreenHalf = new Rectangle((int)(Config.SCREEN_WIDTH_SCALED * scale.X / 2), 0,
                                                            (int)(Config.SCREEN_WIDTH_SCALED * scale.X / 2), (int)(Config.SCREEN_HEIGHT_SCALED * scale.Y));
                        var touchPosition = new Point((int)tl.Position.X, (int)tl.Position.Y);
                        if (!Globals.swappedControls)
                        {
                            if (leftScreenHalf.Contains(touchPosition))
                            {
                                touchId = tl.Id;
                                player.Stop(gameTime);
                            }
                            else if (rightScreenHalf.Contains(touchPosition))
                            {
                                if (player.Jump() && Globals.soundEnabled)
                                    Globals.soundsDictionary["jump"].Play();
                            }
                        }
                        else
                        {
                            if (rightScreenHalf.Contains(touchPosition))
                            {
                                touchId = tl.Id;
                                player.Stop(gameTime);
                            }
                            else if (leftScreenHalf.Contains(touchPosition))
                            {
                                if (player.Jump() && Globals.soundEnabled)
                                    Globals.soundsDictionary["jump"].Play();
                            }
                        }
                    }                    
                }
            }
        }
#endif

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (backgroundTexture != null && !menuLevel)
                spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), Color.White);


            foreach (PixelParticle pixelParticle in pixelParticles)
                pixelParticle.Draw(spriteBatch);

            map.Draw(spriteBatch);

            player.Draw(spriteBatch);

            if (!menuLevel)
                hud.Draw(spriteBatch);

            if (colors)
            {
                spriteBatch.DrawString(Globals.silkscreenFont, "c1: " + levelColors.color1 + " c2: " +  levelColors.color2 + " h: " +  levelColors.hudcolor + " en: " +  levelColors.enemycolor + " em: " +  levelColors.emmitercolor + " t: " +  levelColors.tilecolor + "MUS: " + map.music, new Vector2(10,150), Color.White);
            }
            //resetButton.Draw(spriteBatch);
        }

        public void ReloadColors(LevelColors levelcolors)
        {
            this.levelColors = levelcolors;
            RefreshColors();
        }
        private void InitLevel()
        {
            levelTime = TimeSpan.Zero;
            Globals.CurrentLevelState = this;

            if (!menuLevel)
                hud = new Hud();
            else
                hud = null;

            if (menuLevel)
            {
                Config.Map.WIDTH = Config.Map.MENUMAP_WIDTH;
                Config.Map.HEIGHT = Config.Map.MENUMAP_HEIGHT;
            }
            else
            {
                Config.Map.WIDTH = Config.Map.NORMAL_WIDTH;
                Config.Map.HEIGHT = Config.Map.NORMAL_HEIGHT;
            }

            Globals.CurrentMap = map = Map.LoadMap(directory, levelFile + ".tmx", Globals.graphics, Globals.content, hud, scale);
            Globals.backgroundColor = map.color;

            if (!menuLevel)
                hud.Init(name, map.collectiblesCount, Globals.colorList[levelColors.hudcolor]);

            player = new Player(map.startPosition, Globals.spritesDictionary["player"].texture, (menuLevel ? Globals.suit * 5 + World.LastActiveWorld() : Globals.selectedWorld + Globals.suit * 5));          
            if (map.moving)
                player.SetMovingMapState(-28.0f);         

            if (!Savestate.Instance.levelSaves.ContainsKey(LevelId()))
            {
                Savestate.Instance.levelSaves.Add(LevelId(), new Levelsave());
                Savestate.Instance.Save();
            }

            if (menuLevel)
                pixelParticles.Clear();
        }        

        public void Reset()
        {
            doorGravity = false;
            endDelay = TimeSpan.Zero;
            if (!menuLevel)
                map.Reset();
            player.Reset();
            Globals.soundsDictionary["doors"].Stop();
            if (map.moving)
                player.SetMovingMapState(-28.0f);
            levelTime = TimeSpan.Zero;

            if (!menuLevel)
                hud.Init(name, map.collectiblesCount, Globals.colorList[levelColors.hudcolor]);

            foreach (PixelParticle pixelParticle in pixelParticles)
            {
                pixelParticle.map = Globals.CurrentMap;
                pixelParticle.standingType = Config.StandingType.NoImpact;
            }

            if (!menuLevel)
            {
                Globals.gameStateManager.PushState(Config.States.TAP);
            }
        }

        private void ResetInput()
        {
#if WINDOWS
            previousMouseState = currentMouseState = Mouse.GetState();
            previousKeyboardState = currentKeyboardState = Keyboard.GetState();
#else
                touchId = 0;
                //touch = TouchPanel.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);
        }

        public string LevelId()
        {
            return levelFile;
        }

        public void AddPixelParticle(PixelParticle pixelParticle)
        {
            pixelParticles.Add(pixelParticle);

            while (pixelParticles.Count > Config.PixelParticle.MAX_PARTICLES_LEVEL)
                pixelParticles.RemoveAt(Globals.rnd.Next(pixelParticles.Count));
        }
    }
}
