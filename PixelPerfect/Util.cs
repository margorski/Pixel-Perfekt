using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    static class Util
    {
        public enum Align
        {
            Left,
            Right,
            Center
        }

        public enum GradientType
        {
            Horizontal,
            Vertical
        }

        public static Color GetColorFromName(String name)
        {
            var color_props = typeof(Color).GetProperties();
            foreach (var c in color_props)
                if (name.Equals(c.Name, StringComparison.OrdinalIgnoreCase))
                    return (Color)c.GetValue(new Color(), null);
            return Color.Black;
        }

        public static Rectangle GetSharedRectangle(Rectangle rect1, Rectangle rect2)
        {
            if (!rect1.Intersects(rect2))
                return new Rectangle(0,0,0,0);

            Rectangle shareRect = new Rectangle();

            if (rect1.Left < rect2.Left)
            {
                shareRect.X = rect2.Left;
            }
            else
            {
                shareRect.X = rect1.Left;
            }

            if (rect1.Top < rect2.Top)
            {
                shareRect.Y = rect2.Top;
            }
            else
            {
                shareRect.Y = rect1.Top;
            }

            if (rect1.Right > rect2.Right)
            {
                shareRect.Width = rect2.Right - shareRect.X;
            }
            else
            {
                shareRect.Width = rect1.Right - shareRect.X;
            }

            if (rect1.Bottom > rect2.Bottom)
            {
                shareRect.Height = rect2.Bottom - shareRect.Y;
            }
            else
            {
                shareRect.Height = rect1.Bottom - shareRect.Y;
            }

            return shareRect;
        }

        public static Rectangle NormalizeToBase(Rectangle sourceRect, Rectangle baseRect)
        {
            if (!baseRect.Contains(sourceRect))
                return new Rectangle(0, 0, 0, 0);

            return new Rectangle(sourceRect.X - baseRect.X,
                                 sourceRect.Y - baseRect.Y,
                                 sourceRect.Width, sourceRect.Height);
        }

        public static Texture2D BlitTexture(Texture2D texture, Rectangle blitRect, bool horizontalFlip)
        {
            Texture2D newTexture = new Texture2D(Globals.graphics.GraphicsDevice, blitRect.Width, blitRect.Height);
            Color[] newTextureColors = new Color[blitRect.Width * blitRect.Height];

            Color[] textureColors = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(textureColors);

            int startColumn = blitRect.X;
            int endColumn = (blitRect.X + blitRect.Width);
            int startRow = blitRect.Y;
            int endRow = blitRect.Y + blitRect.Height;
            int counter = 0;

            if (horizontalFlip)
            {
                for (int j = startRow; j < endRow; j++)
                {
                    for (int i = endColumn - 1; i >= startColumn; i--)
                    {
                        newTextureColors[counter++] = textureColors[j * texture.Width + i];
                    }
                }
            }
            else
            {
                for (int j = startRow; j < endRow; j++)
                {
                    for (int i = startColumn; i < endColumn; i++)
                    {
                        newTextureColors[counter++] = textureColors[j * texture.Width + i];
                    }
                }
            }

            newTexture.SetData<Color>(newTextureColors);
            return newTexture;
        }
        
        public static Texture2D GetGradientTexture(int width, int height, Color color1, Color color2, GradientType gradientType)
        {
            Texture2D gradientTexture = new Texture2D(Globals.graphics.GraphicsDevice, width, height);
            Color[] gradientTextureColors = new Color[width * height];

            switch (gradientType)
            {
                case GradientType.Horizontal:
                    for (int i = 0; i < height; i++)
                    {
                        Color gradientColor = Color.Lerp(color1, color2, i / (float)height);
                        for (int j = 0; j < width; j++)
                            gradientTextureColors[i * width + j] = gradientColor;
                    }
                    break;

                case GradientType.Vertical:
                    for (int i = 0; i < width; i++)
                    {
                        Color gradientColor = Color.Lerp(color1, color2, i / (float)width);
                        for (int j = 0; j < height; j++)
                            gradientTextureColors[j * width + i] = gradientColor;
                    }
                    break;
            }
            gradientTexture.SetData<Color>(gradientTextureColors);
            return gradientTexture;
        }

        public static int PixelCount(Texture2D texture)
        {
            int count = 0;

            Color[] textureColors = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(textureColors);

            foreach (Color color in textureColors)
            {
                if (color.A == 255)
                    count++;
            }

            return count;
        }

        public static void DrawStringAligned(SpriteBatch spriteBatch, String text, SpriteFont spriteFont, Color color, Rectangle alignArea, Vector2 margin, Align align)
        {
            Vector2 position = new Vector2(alignArea.X, alignArea.Y);                        

            switch (align)
            {
                case Align.Center:
                    position.X += alignArea.Width / 2 - spriteFont.MeasureString(text).X / 2;
                    break;

                case Align.Left:
                    position.X += margin.X; 
                    break;

                case Align.Right:
                    position.X += alignArea.Width - margin.X - spriteFont.MeasureString(text).X;
                    break;
            }

            spriteBatch.DrawString(spriteFont, text, position, color);
        }
    }
}
