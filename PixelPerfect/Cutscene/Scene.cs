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
        public List<Image> images = new List<Image>();
        public List<Text> texts = new List<Text>();
        public string duration
        {
            get { return _duration.ToString(); }
            set { _duration = TimeSpan.Parse(value); }
        }
        public TimeSpan _duration;

        public void Init()
        {
            foreach (Image image in images)
                image.Init();

            foreach (Text text in texts)
                text.Init();
        }

        public void Update(GameTime gameTime)
        {
            foreach (Image image in images)
                image.Update(gameTime);

            foreach (Text text in texts)
                text.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Image image in images)
                image.Draw(spriteBatch);

            foreach (Text text in texts)
                text.Draw(spriteBatch);
        }
    }
}
