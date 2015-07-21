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
        public string soundName;
        public string time
        {
            get { return _time.ToString(); }
            set { _time = TimeSpan.Parse(value); }
        }
        public TimeSpan _time;

        private SoundEffectInstance soundEffect;
        private TimeSpan currentTime = TimeSpan.Zero;

        public Sound()
        {
        }

        public Sound (Sound sound)
        {
            this.soundName = sound.soundName;
            this._time = sound._time;
        }

        public void Init()
        {
            soundEffect = Globals.soundsDictionary[soundName];
            currentTime = TimeSpan.Zero;
        }

        public bool Update(GameTime gameTime)
        {
            currentTime += gameTime.ElapsedGameTime;
            if (currentTime >= _time)
            {
                if (Globals.soundEnabled)
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
