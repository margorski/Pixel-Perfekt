using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace PixelPerfect.Cutscene
{
    public class Sound
    {
        public string soundPath;
        public string time
        {
            get { return _time.ToString(); }
            set { _time = TimeSpan.Parse(value); }
        }
        public TimeSpan _time;

        private SoundEffect soundEffect;
        private TimeSpan currentTime = TimeSpan.Zero;

        public void Init()
        {
            soundEffect = Globals.content.Load<SoundEffect>(soundPath);
        }

        public bool Update(GameTime gameTime)
        {
            currentTime += gameTime.ElapsedGameTime;
            if (currentTime >= _time)
            {
                Play();
                return false;
            }
            return true;
        }

        private void Play()
        {
            soundEffect.Play();
        }
    }
}
