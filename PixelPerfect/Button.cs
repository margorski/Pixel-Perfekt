using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class Button
    {
        private string text = "";
        private Rectangle rectangle;        
        private Texture2D texture;
        public Color activeColor = Color.White;
        public Color notactiveColor = Color.Gray;
        private SpriteFont font;
        public bool active = true;

        public Button (string text, Rectangle rectangle, Texture2D texture, SpriteFont font)
        {
            this.text = text;
            this.rectangle = rectangle;
            this.texture = texture;
            this.font = font;
        }
        
        public bool Clicked(int x, int y)
        {
            if (!active)
                return false;
            
            return rectangle.Contains(x, y);
        }

        public bool Clicked(int x, int y, float scale)
        {
            x = (int)(x / scale);
            y = (int)(y / scale);

            return Clicked(x, y);
        }

        public void Draw (SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, (active ? Color.White : notactiveColor));
            
            var dimensions = font.MeasureString(text);
            Vector2 centerVector = new Vector2(rectangle.Center.X, rectangle.Center.Y);            

            spriteBatch.DrawString(font, text, centerVector - dimensions / 2, (active ? activeColor : notactiveColor));
        }

    }
}
