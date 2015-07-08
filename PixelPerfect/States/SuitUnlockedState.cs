using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class SuitUnlockedState : GameState
    {
        private TimeSpan waitTime;    

        public SuitUnlockedState()
        {
        }

        public override void Enter(int previousStateId)
        {
            waitTime = new TimeSpan(0, 0, 0, 0, Config.Menu.SUIT_UNLOCKED_WAIT_TIME);
        }

        public override void Exit(int nextStateId)
        {
            Globals.unlockedSuit = -1;
        }

        public override void Suspend(int pushedStateId)
        {
         
        }

        public override void Resume(int poppedStateId)
        {
            waitTime = new TimeSpan(0, 0, 0, 0, Config.Menu.SUIT_UNLOCKED_WAIT_TIME);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            var buttonX = Config.SCREEN_WIDTH_SCALED / 2 - Globals.textureDictionary["suitbutton"].Width / 2;
            var buttonY = Config.SCREEN_WIDTH_SCALED / 2 - Globals.textureDictionary["suitbutton"].Height / 2 - 68;

            spriteBatch.Draw(Globals.textureDictionary["suitbutton"], new Rectangle(buttonX, buttonY,
                                                                                    Globals.textureDictionary["suitbutton"].Width, Globals.textureDictionary["suitbutton"].Height), Color.White);
            spriteBatch.Draw(Globals.spritesDictionary["player"].texture, new Vector2(buttonX + 8, buttonY + 4),
             new Rectangle((Globals.unlockedSuit * 5 + World.LastActiveWorld()) * Config.Player.WIDTH,
                            0, Config.Player.WIDTH, Config.Player.HEIGHT), Color.White);
            spriteBatch.DrawString(Globals.silkscreenFont, "SUIT UNLOCKED", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 37, Config.SCREEN_HEIGHT_SCALED / 2), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            if (Globals.unlockedSuit == -1)
                Globals.gameStateManager.PopState(false);

            if (waitTime >= TimeSpan.Zero)
            {
                waitTime -= gameTime.ElapsedGameTime;
                if (waitTime < TimeSpan.Zero)
                {
                    waitTime = TimeSpan.Zero;
                    Globals.gameStateManager.PopState(true);
                }
            }
        }
    }
}
