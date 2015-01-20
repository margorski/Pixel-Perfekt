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

        private int i = 0;
        private string levelFile = "";
        private string directory = "";

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
        }

        public override void Enter(int previousStateId)
        {
            silkscreenFont = content.Load<SpriteFont>("Silkscreen");
            hud = new Hud(silkscreenFont);
            InitLevel();
            ResetInput();
        }

        public override void Exit(int nextStateId)
        {
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
            player.Update(gameTime);
            if (!player.GetState(Player.State.dead) && !player.GetState(Player.State.dying))
            {
                Rectangle tempRectangle;
                player.MoveHorizontally(gameTime);
                if (map.CheckCollisions(player.boundingBox, Tile.Attributes.Solid, out tempRectangle))
                {
                    player.HitTheWall(tempRectangle);
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
                else if (player.speed.Y > 0.0f && map.CheckPlatformCollisions(player.boundingBox, out tempRectangle))
                {
                    player.HitTheGround(tempRectangle);
                }
                else
                {
                    if (player.speed.Y > 0.0f && !player.GetState(Player.State.jumping))
                    {
                        var playerBoxMovedDown = player.boundingBox;
                        playerBoxMovedDown.Y += 1;

                        if (!map.CheckCollisions(playerBoxMovedDown, Tile.Attributes.Solid, out tempRectangle) &&  // check if there is not collision from bottom
                            !map.CheckPlatformCollisions(playerBoxMovedDown, out tempRectangle))
                        {
                            player.SetState(Player.State.jumping, true);
                            player.SetState(Player.State.falling, true);
                            player.jumpY = player.boundingBox.Y;
                        }
                    }
                }

                player.SetMovingPlatformState(map.movingModifier);

                map.CheckTriggers(player.boundingBox);

                if (map.GrabCollectibles(player, graphics))
                    hud.Collect();

                if (map.KillThisBastard(player, graphics))
                    player.StartDying();

                if (map.EnteredDoors(player.boundingBox))
                {
                    if (!gameStateManager.ChangeState(gameStateManager.CurrentState() + 1))
                        gameStateManager.EmptyStack();
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

                    if (leftScreenHalf.Contains(mousePosition))
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

                    if (new Rectangle((int)(Config.SCREEN_WIDTH_SCALED * scale / 2), 0,
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

        public override void Draw(SpriteBatch spriteBatch, bool suspended)
        {
            map.Draw(spriteBatch);
            player.Draw(spriteBatch);
            hud.Draw(spriteBatch);
        }

        private void InitLevel()
        {
            map = Map.LoadMap(directory, levelFile + ".tmx", graphics, content, gameStateManager, hud, scale);
            hud.Init(map.levelName, map.collectiblesCount);
            player = new Player(map.startPosition, content.Load<Texture2D>(directory + "\\" + "player"), graphics, content.Load<Texture2D>("pixel"));
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
    }
}
