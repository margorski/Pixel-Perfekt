using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using GameStateMachine;

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

        private GraphicsDeviceManager graphics;
        private ContentManager content;
        
        private Map map;        
        private Player player;
        private Hud hud;
        private List<PixelParticle> pixelParticles = new List<PixelParticle>();
#if !WINDOWS
        private TouchCollection touchCollection;
        private int touchId = 0; 
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

        public string levelFile { private set; get; }
        private string directory = "";

        public TimeSpan levelTime { private set; get; }

        //Button resetButton;
        
        private Texture2D backgroundTexture = Util.GetGradientTexture(Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED, Color.MidnightBlue, Color.DarkSlateBlue, Util.GradientType.Horizontal);

        private LevelColors levelColors = new LevelColors();
        private bool colors = false;

#if WINDOWS
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
            hud.SetColor(Globals.colorList[levelColors.hudcolor]);
        }
        private void PreviousHudColor()
        {
            if (--levelColors.hudcolor < 0)
                levelColors.hudcolor = Globals.colorList.Count - 1;
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
#endif

        private void ReloadGradientTexture()
        {
            if (levelColors.color1 < 0 || levelColors.color1 > Globals.colorList.Count - 1 || levelColors.color2 < 0 || levelColors.color2 > Globals.colorList.Count - 1)
                return;

            backgroundTexture = Util.GetGradientTexture(Config.SCREEN_WIDTH_SCALED + 2, Config.SCREEN_HEIGHT_SCALED, Globals.colorList[levelColors.color1], Globals.colorList[levelColors.color2], Util.GradientType.Horizontal);
        }

        private void RefreshColors()
        {
            Globals.tilesColor = Globals.colorList[levelColors.tilecolor];
            Globals.emitersColor = Globals.colorList[levelColors.emmitercolor];
            Globals.enemiesColor = Globals.colorList[levelColors.enemycolor];
            hud.SetColor(Globals.colorList[levelColors.hudcolor]);
            ReloadGradientTexture();
        }

        public LevelState(GraphicsDeviceManager graphics, ContentManager content, String directory, String levelFile)
        {
            this.graphics = graphics;
            this.content = content;
            this.levelFile = levelFile;
            this.directory = directory;
            if (levelFile.Length > 4)
            {
                if (!levelFile.Substring(levelFile.Length - 4, 4).ToLower().Equals(".tmx"))
                    levelFile += ".tmx";
            }
            //resetButton = new Button("RESET", new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 12, 60, 12), Globals.pixelTexture, silkscreenFont, false);
            //resetButton.activeColor = Color.Black;
        }

        public override void Enter(int previousStateId)
        {
            InitLevel();     
            ResetInput();
            MediaPlayer.IsRepeating = true;
            if (Globals.musicEnabled)
                MediaPlayer.Play(Globals.backgroundMusicList[map.music]);
        }

        public override void Exit(int nextStateId)
        {
            Globals.CurrentLevelState = null;
            Globals.upsideDown = false;
            Globals.backgroundColor = Color.Black;
            foreach (KeyValuePair<string, SoundEffectInstance> sfinstance in Globals.soundsDictionary)
                sfinstance.Value.Stop();
            MediaPlayer.Pause();
        }

        public override void Resume(int poppedStateId)
        {
            ResetInput();
            MediaPlayer.Resume();
        }

        public override void Suspend(int pushedStateId)
        {
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            currGPState = GamePad.GetState(PlayerIndex.One);
            if ((currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released))
            {
                Globals.gameStateManager.PushState(Config.States.PAUSE);
            }
            prevGPState = currGPState;

#if !WINDOWS
            TouchInput(gameTime);
#else
            MouseInput(gameTime);
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
            
            if (!player.GetState(Player.State.dead) && !player.GetState(Player.State.dying))
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

                if (map.GrabCollectibles(player, graphics))
                {
                    hud.Collect();
                    if (Globals.soundEnabled)
                        Globals.soundsDictionary["coin"].Play();
                }

                if (map.KillThisBastard(player, graphics))
                {
                    player.Die();
                }

                if (map.EnteredDoors(player.boundingBox))
                {
                    Globals.soundsDictionary["doors"].Stop();
                    if (!Savestate.Instance.levelSaves[LevelId()].completed)
                    {
                        Savestate.Instance.levelSaves[LevelId()].completed = true;
                        Savestate.Instance.levelSaves[LevelId()].skipped = false;
                        Savestate.Instance.levelSaves[LevelId()].bestTime = levelTime;
                        Savestate.Instance.Save();
                    }
                    else if (Savestate.Instance.levelSaves[LevelId()].bestTime > levelTime)
                    {
                        Savestate.Instance.levelSaves[LevelId()].bestTime = levelTime;
                        Savestate.Instance.Save();
                    }
                    if (!Globals.gameStateManager.ChangeState(Config.States.MENU))
                        Globals.gameStateManager.EmptyStack();                        
                    return;
                }
            }
            map.Update(gameTime);

            hud.Update(gameTime);
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
            else
            {
                if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed) // mouse button released
                {
                    player.EndOfStop(gameTime);
                }
                else if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released) // mouse button pressed
                {
                    var leftScreenHalf = new Rectangle(0, 0, (int)(Config.SCREEN_WIDTH_SCALED * scale / 2),
                                                             (int)(Config.SCREEN_HEIGHT_SCALED * scale));
                    var rightScreenHalf = new Rectangle((int)(Config.SCREEN_WIDTH_SCALED * scale / 2), 0,
                                                        (int)(Config.SCREEN_WIDTH_SCALED * scale / 2), (int)(Config.SCREEN_HEIGHT_SCALED * scale));
                    var mousePosition = new Point((int)currentMouseState.X, (int)currentMouseState.Y);

                    //if (resetButton.Clicked(mousePosition.X, mousePosition.Y, scale))                    
                      //  InitLevel();
                    //else 
                    if (leftScreenHalf.Contains(mousePosition))
                        player.Stop(gameTime);
                    else if (rightScreenHalf.Contains(mousePosition))
                    {
                        if (player.Jump() && Globals.soundEnabled)
                            Globals.soundsDictionary["jump"].Play();
                    }
                }
            }
            previousMouseState = currentMouseState;
        }

        private void KeyboardInput(GameTime gameTime)
        {
            currentKeyboardState = Keyboard.GetState();

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
            // END DEBUGG

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
        private void TouchInput(GameTime gameTime)
        {
            touchCollection = TouchPanel.GetState();
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

                    //if (resetButton.Clicked((int)tl.Position.X, (int)tl.Position.Y, scale))
                    //    InitLevel();
                    //else 
                    if (new Rectangle((int)(Config.SCREEN_WIDTH_SCALED * scale / 2), 0,
                                      (int)(Config.SCREEN_WIDTH_SCALED * scale / 2), (int)(Config.SCREEN_HEIGHT_SCALED * scale)).Contains(new Point((int)tl.Position.X, (int)tl.Position.Y))
                        && tl.State == TouchLocationState.Pressed)
                    {
                        if (player.Jump() && Globals.soundEnabled)
                            Globals.soundsDictionary["jump"].Play();
                    }

                    else if (new Rectangle(0, 0, (int)(Config.SCREEN_WIDTH_SCALED * scale / 2),
                                                 (int)(Config.SCREEN_HEIGHT_SCALED * scale)).Contains(new Point((int)tl.Position.X, (int)tl.Position.Y)))
                    {
                        if (tl.State == TouchLocationState.Pressed)
                        {
                            touchId = tl.Id;
                            player.Stop(gameTime);
                        }
                    }
                }
            }
        }
#endif

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (backgroundTexture != null)
                spriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);

            if (!upsidedownBatch)
            {
                foreach (PixelParticle pixelParticle in pixelParticles)
                    pixelParticle.Draw(spriteBatch);

                map.Draw(spriteBatch);

                player.Draw(spriteBatch);
            }

            if (!upsidedownBatch && Globals.upsideDown)
                return;
            
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

            hud = new Hud();

            Globals.CurrentMap = map = Map.LoadMap(directory, levelFile + ".tmx", graphics, content, hud, scale);
            Globals.backgroundColor = map.color;
            if (map.upsidedown)
                Globals.upsideDown = true;
                        hud.Init(map.levelName, map.collectiblesCount, Globals.colorList[levelColors.hudcolor]);

            player = new Player(map.startPosition, Globals.spritesDictionary["player"].texture);          
            if (map.moving)
                player.SetMovingMapState(-28.0f);


            //ReloadGradientTexture();

            if (!Savestate.Instance.levelSaves.ContainsKey(LevelId()))
            {
                Savestate.Instance.levelSaves.Add(LevelId(), new Levelsave());
                Savestate.Instance.Save();
            }
        }        

        private void Reset()
        {
            map.Reset();
            player.Reset();
            Globals.soundsDictionary["doors"].Stop();
            if (map.moving)
                player.SetMovingMapState(-28.0f);
            levelTime = TimeSpan.Zero;
            hud.Init(map.levelName, map.collectiblesCount, Globals.colorList[levelColors.hudcolor]);

            foreach (PixelParticle pixelParticle in pixelParticles)
            {
                pixelParticle.map = Globals.CurrentMap;
                pixelParticle.standingType = Config.StandingType.NoImpact;
            }
        }

        private void ResetInput()
        {
#if WINDOWS
            previousMouseState = currentMouseState = Mouse.GetState();
#else
                touchId = 0;
                touchCollection = TouchPanel.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);
        }

        public void AddPixelParticle(PixelParticle pixelParticle)
        {
            pixelParticles.Add(pixelParticle);

            while (pixelParticles.Count > Config.PixelParticle.MAX_PARTICLES)
                pixelParticles.RemoveAt(Globals.rnd.Next(pixelParticles.Count));
        }

        public string LevelId()
        {
            return levelFile;
        }
    }
}
