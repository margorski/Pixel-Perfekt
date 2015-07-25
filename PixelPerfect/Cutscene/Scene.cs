using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect.Cutscene
{
    class Scene
    {
        public List<Item> items = new List<Item>();
        //public List<Text> texts = new List<Text>();
        public List<Sound> sounds = new List<Sound>();

        public string duration
        {
            get { return _duration.ToString(); }
            set { _duration = TimeSpan.Parse(value); }
        }
        public TimeSpan _duration;
        public Color backgroundColor = Color.Transparent;
        public Texture2D gradientTexture = null;
        private Song music;

        public void Init()
        {
            foreach (Item item in items)
                item.Init();

            //foreach (Text text in texts)
            //    text.Init();
            
            foreach (Sound sound in sounds)
                sound.Init();
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                if (!sounds[i].Update(gameTime))                
                    sounds.RemoveAt(i--);                
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].Update(gameTime))
                    items.RemoveAt(i--);
            }

            //for (int i = 0; i < texts.Count; i++)
            //{
            //    if (!texts[i].Update(gameTime))
            //        texts.RemoveAt(i--);
            //}
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (gradientTexture != null)
                spriteBatch.Draw(gradientTexture, new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), Color.White);
            else
                spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), backgroundColor);

            foreach (Item item in items)
                item.Draw(spriteBatch);
            //foreach (Image image in images)
            //    image.Draw(spriteBatch);

            //foreach (Text text in texts)
            //    text.Draw(spriteBatch);
        }
    }
}
