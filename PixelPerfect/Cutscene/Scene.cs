using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelPerfect.Cutscene
{
    public class Scene
    {
        public List<Item> itemList = new List<Item>();
        public string duration
        {
            get { return _duration.ToString(); }
            set { _duration = TimeSpan.Parse(value); }
        }
        public TimeSpan _duration;

        public void Init()
        {
            foreach (Item item in itemList)
                item.Init();
        }

        public void Update(GameTime gameTime)
        {
            foreach (Item item in itemList)
                item.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Item item in itemList)
                item.Draw(spriteBatch);
        }
    }
}
