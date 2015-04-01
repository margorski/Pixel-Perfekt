using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
    class LevelState : GameState
    {
        private GraphicsDeviceManager graphics;
        private ContentManager content;
        private GameStateManager gameStateManager;

        private SpriteFont silkscreenFont;
        
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
        public int deathCount { private set; get; }
        //private int i = 0;

        public string levelFile { private set; get; }
        private string directory = "";

        public TimeSpan levelTime { private set; get; }

        Button resetButton;

        public LevelState(GraphicsDeviceManager graphics, ContentManager content, String directory, String levelFile, GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
            this.graphics = graphics;
            this.content = content;
            this.levelFile = levelFile;
            this.directory = directory;
            if (levelFile.Length > 4)
            {
                if (!levelFile.Substring(levelFile.Length - 4, 4).ToLower().Equals(".tmx"))
                    levelFile += ".tmx";
            }
            silkscreenFont = content.Load<SpriteFont>("Silkscreen");
            resetButton = new Button("RESET", new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 12, 60, 12), Globals.pixelTexture, silkscreenFont);
            resetButton.activeColor = Color.Black;
        }

        public override void Enter(int previousStateId)
        {
            hud = new Hud(silkscreenFont);
            Globals.CurrentLevelState = this;
            InitLevel();
            ResetInput();

            if (map.upsidedown)
                Globals.upsideDown = true;            
        }

        public override void Exit(int nextStateId)
        {
            Globals.CurrentLevelState = null;
            Globals.upsideDown = false;
            Globals.backgroundColor = Color.Black;
        }

        public override void Resume(int poppedStateId)
        {
            ResetInput();
        }

        public override void Suspend(int pushedStateId)
        {
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            currGPState = GamePad.GetState(PlayerIndex.One);
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
            {
                gameStateManager.PushState(Config.States.PAUSE);
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
                        player.Jump(true);
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
                    hud.Collect();

                if (map.KillThisBastard(player, graphics))
                {
                    player.SetState(Player.State.dying, true);
                    deathCount++;
                    
                    if (!Savestate.Instance.levelSaves[LevelId()].completed)
                    {
                        Savestate.Instance.levelSaves[LevelId()].completeDeathCount++;                        
                    }
                    Savestate.Instance.levelSaves[LevelId()].deathCount++;
                    Savestate.Instance.Save();
                }

                if (map.EnteredDoors(player.boundingBox))
                {
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
                    if (!gameStateManager.ChangeState(Config.States.MENU))
                        gameStateManager.EmptyStack();                        
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
                    InitLevel();
            }
            else if (player.GetState(Player.State.dying))
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                {
                    player.SetState(Player.State.dead, true);
                    InitLevel();
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

                    if (resetButton.Clicked(mousePosition.X, mousePosition.Y, scale))                    
                        InitLevel();
                    else if (leftScreenHalf.Contains(mousePosition))
                        player.Stop(gameTime);
                    else if (rightScreenHalf.Contains(mousePosition))
                        player.Jump();
                }
            }
            previousMouseState = currentMouseState;
        }

        private void KeyboardInput(GameTime gameTime)
        {
            currentKeyboardState = Keyboard.GetState();

            if (player.GetState(Player.State.dead))
            {
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                    InitLevel();
            }            
            else if (player.GetState(Player.State.dying))
            {
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                {
                    player.SetState(Player.State.dead, true);
                    InitLevel();
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
                    player.Jump();
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
                        InitLevel();
                }                
                else if (player.GetState(Player.State.dying))
                {
                    if (tl.State == TouchLocationState.Pressed)
                    {
                        player.SetState(Player.State.dead, true);
                        InitLevel();
                    }
                }
                else
                {
                    if (tl.State == TouchLocationState.Released && tl.Id == touchId)
                    {
                        touchId = 0;
                        player.EndOfStop(gameTime);
                    }

                    if (resetButton.Clicked((int)tl.Position.X, (int)tl.Position.Y, scale))
                        InitLevel();                    
                    else if (new Rectangle((int)(Config.SCREEN_WIDTH_SCALED * scale / 2), 0,
                                      (int)(Config.SCREEN_WIDTH_SCALED * scale / 2), (int)(Config.SCREEN_HEIGHT_SCALED * scale)).Contains(new Point((int)tl.Position.X, (int)tl.Position.Y))
                        && tl.State == TouchLocationState.Pressed)
                        player.Jump();

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
            resetButton.Draw(spriteBatch);
        }

        private void InitLevel()
        {
            Globals.CurrentMap = map = Map.LoadMap(directory, levelFile + ".tmx", graphics, content, gameStateManager, hud, scale);
            Globals.backgroundColor = map.color;
            hud.Init(map.levelName, map.collectiblesCount);
            player = new Player(map.startPosition, content.Load<Texture2D>(directory + "\\" + "player"), graphics);
            if (map.moving)
                player.SetMovingMapState(-28.0f);
            levelTime = TimeSpan.Zero;

            foreach (PixelParticle pixelParticle in pixelParticles)
            {
                pixelParticle.map = Globals.CurrentMap;
                pixelParticle.standingType = Config.StandingType.NoImpact;
            }

            if (!Savestate.Instance.levelSaves.ContainsKey(LevelId()))
            {
                Savestate.Instance.levelSaves.Add(LevelId(), new Levelsave());
                Savestate.Instance.Save();
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
                pixelParticles.RemoveAt(0);
        }

        public string LevelId()
        {
            return levelFile;
        }
    }
}
