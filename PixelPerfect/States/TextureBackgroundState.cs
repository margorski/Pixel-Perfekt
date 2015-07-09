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
    class TextureBackgroundState : GameState
    {
        int color1;
        int color2;
        private Texture2D background;

        public TextureBackgroundState(int color1, int color2)
        {
            ChangeColors(color1, color2);
            Globals.graphics.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
        }

        void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            ReloadGradient();
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

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            spriteBatch.Draw(background, Vector2.Zero, Color.White);
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
        }

        public void ChangeColors(int color1, int color2)
        {
            this.color1 = color1;
            this.color2 = color2;
            ReloadGradient();
        }

        public void ReloadGradient()
        {
            background = Util.GetGradientTexture(Config.SCREEN_WIDTH_SCALED + 2, Config.SCREEN_HEIGHT_SCALED, Globals.colorList[color1], Globals.colorList[color2], Util.GradientType.Horizontal);
        }
    }
}
