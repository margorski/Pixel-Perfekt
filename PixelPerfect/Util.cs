using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
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

        public static Color MultiplyColor(Color color, float value)
        {
            var r = (int)(color.R * value);
            var g = (int)(color.G * value);
            var b = (int)(color.B * value);          

            return new Color(r, g, b); 
        }

        public static List<Color> GetColorList()
        {
            var colorProperties = typeof(Color).GetProperties();
            List<Color> colorList = new List<Color>();
            for (int i = 4; i < 146; i++)
                colorList.Add((Color)colorProperties[i].GetValue(new Color(), null));
            return colorList;
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

        public static Color[] GetTextureArray(Texture2D texture, int width, int height)
        {
            Color[] colorArray = new Color[width * height];
            texture.GetData(colorArray);
            return colorArray;
        }

        public static Color[] GetTextureArray(Texture2D texture)
        {
            return GetTextureArray(texture, texture.Width, texture.Height);
        }

        public static Texture2D BlitTexture(Texture2D texture, Rectangle blitRect)
        {
            Texture2D newTexture = new Texture2D(Globals.graphics.GraphicsDevice, blitRect.Width, blitRect.Height);
            Color[] newTextureColors = new Color[blitRect.Width * blitRect.Height];


            Color[] textureColors = new Color[texture.Width * texture.Height];
            texture.GetData(textureColors);

            int startColumn = blitRect.X;
            int endColumn = (blitRect.X + blitRect.Width);
            int startRow = blitRect.Y;
            int endRow = blitRect.Y + blitRect.Height;
            int counter = 0;

            for (int j = startRow; j < endRow; j++)
            {
                for (int i = startColumn; i < endColumn; i++)
                {
                    newTextureColors[counter++] = textureColors[j * texture.Width + i];
                }
            }

            newTexture.SetData<Color>(newTextureColors);
            return newTexture;
        }
        

        public static List<Color[]> PrepareTextureArray(Texture2D texture, int width, int height)
        {           
            var tileTextureArray = new List<Color[]>();
            int tilesInRow = texture.Width / width;
            int textureNum = tilesInRow * (texture.Height / height);
            Color[] textureColors = new Color[texture.Width * texture.Height];
            texture.GetData(textureColors);

            for (int i = 0; i < textureNum; i++)            
            {
                Color[] colorArray = new Color[width * height];

                int startColumn = (i % tilesInRow) * width;
                int endColumn = startColumn + width;
                int startRow = (i / tilesInRow) * height;
                int endRow = startRow + height;
                int counter = 0;

                for (int j = startRow; j < endRow; j++)
                {
                    for (int k = startColumn; k < endColumn; k++)
                    {
                        colorArray[counter++] = textureColors[j * texture.Width + k];
                    }
                }
                tileTextureArray.Add(colorArray);
            }

            return tileTextureArray;
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
            texture.GetData(textureColors);

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

        public delegate void DrawDelegate(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false);

        public static Texture2D DrawToTexture (DrawDelegate drawDelegate)
        {
            if (drawDelegate == null)
                return null;

            Globals.graphics.GraphicsDevice.SetRenderTarget(Globals.renderTarget);
            Globals.graphics.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            Globals.graphics.GraphicsDevice.Clear(Color.Transparent);


            Matrix matrix = Matrix.Identity;
            //matrix *= Matrix.CreateTranslation(new Vector3(-3, 0, 0)); // position adjusting
            //matrix *= Matrix.CreateScale(state.scale);
            Globals.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, matrix);

            drawDelegate(Globals.spriteBatch, false);
            Globals.spriteBatch.End();

            Globals.graphics.GraphicsDevice.SetRenderTarget(null);

            return (Texture2D)Globals.renderTarget;
        }

        public static Song LoadSong(String songLocation)
        {
#if ANDROID
            songLocation = "Content\\" + songLocation + ".ogg";
            songLocation = songLocation.Replace("\\", "/");
            return Song.FromUri(songLocation, new Uri(songLocation, UriKind.Relative));
#else
            return Globals.content.Load<Song>(songLocation);
#endif
        }

        public static void AdsOff()
        {
#if WINDOWS_PHONE
            GamePage.Instance.AdsOff();
#elif ANDROID
			Activity1.Instance.AdsOff();
#endif
        }


        public static void AdsOn()
        {
            if (Globals.noads)
                return;
#if WINDOWS_PHONE
            GamePage.Instance.AdsOn();
#elif ANDROID
			Activity1.Instance.AdsOn();
#endif
        }


    }
}
