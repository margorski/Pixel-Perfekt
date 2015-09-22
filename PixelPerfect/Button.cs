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
    class Button
    {
        private string text = "";
        public Rectangle rectangle;        
        private Texture2D texture;
        public Color activeColor = Color.White;
        public Color notactiveColor = new Color(0.25f, 0.25f, 0.25f);
        public Color toggleColor = Color.DimGray;
        private SpriteFont font;
        public bool active = true;
        public bool value = false;
        public bool clicked { private set; get; }
        private bool toggleable = false;
        public bool visible = true;

        public int X { get { return rectangle.X; } }
        public int Y { get { return rectangle.Y; } }
        public int Width { get { return rectangle.Width; } }
        public int Height { get { return rectangle.Height; } }

        public Button (string text, Rectangle rectangle, Texture2D texture, SpriteFont font, bool toggleable)
        {
            this.text = text;
            this.rectangle = rectangle;
            this.texture = texture;
            this.font = font;
            this.toggleable = toggleable;   
            clicked = false;
        }
        
        public bool Clicked(int x, int y, bool release)
        {
            if (!visible)
                return false;

            clicked = false;

            if (!active)
                return false;

            if (rectangle.Contains(x, y))
            {
                if (release)
                    clicked = false;
                else
                    clicked = true;

                if (toggleable && release)
                    Toggle();
                return true;
            }
            return false;
        }
        
        public void setPosition (Vector2 position)
        {
            rectangle = new Rectangle((int)position.X, (int)position.Y, rectangle.Width, rectangle.Height);
        }
            
        public bool Clicked(int x, int y, Vector2 scale, bool release)
        {
            x = (int)(x / scale.X);
            y = (int)(y / scale.Y);

            return Clicked(x, y, release);
        }

        public void Toggle()
        {
            if (!visible)
                return;

            if (!toggleable)
                return;

            value = !value;
        }

        public void Draw (SpriteBatch spriteBatch)
        {
            if (!visible)
                return;

            var color = (active ? activeColor : notactiveColor);
            if (toggleable && !value)
                color = toggleColor;

            if (clicked)
            {
                color = (toggleable ? value ? Color.LightGray : Color.DarkGray : Color.LightGray);
            }

            Draw(spriteBatch, color);
        }

        public void Draw(SpriteBatch spriteBatch, Color color)
        {
            if (!visible)
                return;

            spriteBatch.Draw(texture, rectangle, color);

            var dimensions = font.MeasureString(text);
            Vector2 centerVector = new Vector2(rectangle.Center.X, rectangle.Center.Y);

            spriteBatch.DrawString(font, text, centerVector - dimensions / 2, color);
        }
    }    
}
