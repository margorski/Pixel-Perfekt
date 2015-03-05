using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameStateMachine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect.Bosses.Capman
{
    class PacmanLevelState : GameState
    {
        private GraphicsDeviceManager graphics;
        private ContentManager content;
        private GameStateManager gameStateManager;
        private SpriteFont silkscreenFont;

        PacmanBoss pacmanBoss;

        public PacmanLevelState(GraphicsDeviceManager graphics, ContentManager content, GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
            this.graphics = graphics;
            this.content = content;

            pacmanBoss = new PacmanBoss(content.Load<Texture2D>(@"pacman\pacman_13x13"), new Vector2(100.0f, 100.0f), new Vector2(13.0f, 13.0f), 0, new Vector2(-15.0f, 15.0f),
                                        false, false, false, 0);
            pacmanBoss.Init();            
        }

        public override void Enter(int previousStateId)
        {
            
        }

        public override void Exit(int nextStateId)
        {
            
        }

        public override void Suspend(int pushedStateId)
        {
            
        }

        public override void Resume(int poppedStateId)
        {
            
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            pacmanBoss.Update(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            pacmanBoss.Draw(spriteBatch);
        }
    }
}
