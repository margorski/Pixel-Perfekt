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
        private TouchCollection touchCollection;
        private GamePadState prevGPState;
        private GamePadState currGPState;
        private int touchId = 0;
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
            ResetTouch();
        }

        public override void Exit(int nextStateId)
        {
        }

        public override void Resume(int poppedStateId)
        {
            ResetTouch();
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
            
            touchCollection = TouchPanel.GetState();
            foreach (TouchLocation tl in touchCollection)
            {
                if (player.GetState(Player.State.dead))
                {
                    if (tl.State == TouchLocationState.Pressed)
                        InitLevel();
                }
                    /*
                else if (player.GetState(Player.State.dying))
                {
                    if (tl.State == TouchLocationState.Released)
                    {
                        player.SetState(Player.State.dead, true);
                    }
                }*/
                else
                {
                    if (tl.State == TouchLocationState.Released && tl.Id == touchId)
                    {
                        touchId = 0;
                        player.EndOfStop(gameTime);
                    }

                    if (new Rectangle(Config.SCREEN_WIDTH / 2, 0, Config.SCREEN_WIDTH / 2, Config.SCREEN_HEIGHT).Contains(new Point((int)tl.Position.X, (int)tl.Position.Y))
                        && tl.State == TouchLocationState.Pressed)
                        player.Jump();

                    else if (new Rectangle(0, 0, Config.SCREEN_WIDTH / 2, Config.SCREEN_HEIGHT).Contains(new Point((int)tl.Position.X, (int)tl.Position.Y)))
                    {
                        if (tl.State == TouchLocationState.Pressed)
                        {
                            touchId = tl.Id;
                            player.Stop(gameTime);
                        }
                    }
                }
            }

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
                    if (i++ > 0 && !player.GetState(Player.State.jumping))
                    {
                        i = 0;
                        player.SetState(Player.State.jumping, true);
                        player.SetState(Player.State.falling, true);
                        player.jumpY = player.boundingBox.Y;
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

        public override void Draw(SpriteBatch spriteBatch, bool suspended)
        {
            map.Draw(spriteBatch);
            player.Draw(spriteBatch);
            hud.Draw(spriteBatch);
        }

        private void InitLevel()
        {
            map = Map.LoadMap(directory, levelFile + ".tmx", graphics, content, gameStateManager, hud);
            hud.Init(map.levelName, map.collectiblesCount);
            player = new Player(map.startPosition, content.Load<Texture2D>(directory + "\\" + "player"), graphics, content.Load<Texture2D>("pixel"));
        }

        private void ResetTouch()
        {
            touchId = 0;
            touchCollection = TouchPanel.GetState();
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);
        }
    }
}
