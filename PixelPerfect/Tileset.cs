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
    class Tileset
    {
        public Texture2D texture { get; private set; }
        public List<Color[]> tileTextureArray { get; private set; }

        public Tileset (string path, int width = Config.Tile.SIZE, int height = Config.Tile.SIZE)
        {
            texture = Globals.content.Load<Texture2D>(path);
            tileTextureArray = new List<Color[]>();
            int tilesInRow = texture.Width / width;
            int textureNum = tilesInRow * (texture.Height / height);

            for (int i = 0; i < textureNum; i++)
            {
                tileTextureArray.Add(Util.GetTextureArray(Util.BlitTexture(texture, new Rectangle((i % tilesInRow) * width, (i / tilesInRow) * height, width, height)), width, height));
            }
                
        }
    }
}
