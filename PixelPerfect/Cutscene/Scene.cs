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
    public class Scene
    {
        public List<Image> images = new List<Image>();
        public List<Text> texts = new List<Text>();
        public List<Sound> sounds = new List<Sound>();

        public string duration
        {
            get { return _duration.ToString(); }
            set { _duration = TimeSpan.Parse(value); }
        }
        public TimeSpan _duration;
        public string backroundMusic = "";
        public Color backgroundColor = Color.Transparent;

        private Song music;

        public void Init()
        {                        
            foreach (Image image in images)
                image.Init();

            foreach (Text text in texts)
                text.Init();
            
            foreach (Sound sound in sounds)
                sound.Init();

            if (backroundMusic != "")
            {
                music = Globals.content.Load<Song>(backroundMusic);
                if (music != null)
                    MediaPlayer.Play(music);
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                if (!sounds[i].Update(gameTime))                
                    sounds.RemoveAt(i--);                
            }

            for (int i = 0; i < images.Count; i++)
            {
                if (!images[i].Update(gameTime))
                    images.RemoveAt(i--);
            }

            for (int i = 0; i < texts.Count; i++)
            {
                if (!texts[i].Update(gameTime))
                    texts.RemoveAt(i--);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), backgroundColor);

            foreach (Image image in images)
                image.Draw(spriteBatch);

            foreach (Text text in texts)
                text.Draw(spriteBatch);
        }
    }
}
