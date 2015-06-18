using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfect
{
    interface IBackground
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spritebatch);
    }

    class SolidBackground : IBackground
    {
        public Color color = Color.Black;
        private Rectangle screenRectangle = new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED);

        public void Update(GameTime gameTime)
        {        
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(Globals.textureDictionary["pixel"], screenRectangle, color);            
        }
    }

    class RainbowBackground : IBackground
    {

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spritebatch)
        {
            throw new NotImplementedException();
        }
    }
}
