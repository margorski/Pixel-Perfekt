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
    class Level
    {
        public Texture2D thumbnail = null;
        public String shortName;
        public String levelName;
        public TimeSpan time;

        public Level(String shortName, String levelName, Texture2D thumbnail, TimeSpan time)
        {
            this.shortName = shortName;
            this.levelName = levelName;
            this.thumbnail = thumbnail;
            this.time = time;
        }
    }
}
