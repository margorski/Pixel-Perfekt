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
    class Sprite
    {
        public Texture2D texture { get; private set; }
        public List<Color[]> textureArray { get; private set; }

        public Sprite (string path, int width, int height, int frames = Config.ANIM_FRAMES)
        {
            // doprowadzic by util.preparearray przygotowywal kolumny Color arrayow
            texture = Globals.content.Load<Texture2D>("Levels\\fgame\\" + path);
            textureArray = Util.PrepareTextureArray(texture, width, height * frames);
            
            //textureArray = new List<Color[]>();
            //int textureNum = texture.Width / width;

            //for (int i = 0; i < textureNum; i++)
            //{
            //    textureArray.Add(Util.GetTextureArray(Util.BlitTexture(texture, new Rectangle(i * width, 0, width, height * frames)), width, height * frames));
            //}
                
        }
    }
}
