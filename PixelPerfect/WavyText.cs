using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelPerfect
{
    class WavyText 
    {
        private String text = "";
        private TimeSpan waveTimer = TimeSpan.Zero;

        private int timeMilliseconds = 0;
        private float scale = 1.0f;
        private Color[] colors;
        private float sinShift = 1.0f;
        private float sinPow = 1.0f;
        private float scalemodifier;
        private Vector2 position;

        public WavyText (String text, Vector2 position, int timeMilliseconds, float scale, Color[] colors, float sinShift, float sinPow, float scalemodifier = 0.5f)
        {
            this.text = text;
            this.position = position;
            this.scale = scale;
            this.timeMilliseconds = timeMilliseconds;
            this.colors = colors;
            this.sinShift = sinShift;
            this.sinPow = sinPow;
            this.scalemodifier = scalemodifier;
        }

        public void Update (GameTime gameTime)
        {
            waveTimer += gameTime.ElapsedGameTime;
            if (waveTimer.TotalMilliseconds >= timeMilliseconds)
                waveTimer = TimeSpan.Zero;
        }

        public void Draw (SpriteBatch spriteBatch)
        {
            float sineBase = (float)(2 * Math.PI * (waveTimer.TotalMilliseconds / (double)timeMilliseconds));
            for (int i = 0; i < text.Length; i++)
            {
                spriteBatch.DrawString(Globals.silkscreenFont, text.Substring(i, 1),
                       new Vector2(position.X + Globals.silkscreenFont.MeasureString(text.Substring(0, i)).X * (scale - scalemodifier) - 1, position.Y + (float)Math.Sin(sineBase + (i / sinShift) * Math.PI * 2) * sinPow + 1),
                       Color.Black, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(Globals.silkscreenFont, text.Substring(i, 1),
                                       new Vector2(position.X + Globals.silkscreenFont.MeasureString(text.Substring(0, i)).X * (scale - scalemodifier), position.Y + (float)Math.Sin(sineBase + (i / sinShift) * Math.PI * 2) * sinPow),
                                       colors[i], 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);

            }
        }
    }
}
