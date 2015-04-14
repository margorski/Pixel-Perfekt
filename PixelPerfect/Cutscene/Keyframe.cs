using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PixelPerfect.Cutscene
{
    public class Keyframe
    {        
        public string time
        {
            get { return _time.ToString(); }
            set { _time = TimeSpan.Parse(value); }
        }
        public TimeSpan _time;

        public Vector2 position = Vector2.Zero;
        public Vector2 origin = Vector2.Zero;
        public float scale = 1.0f;
        public float rotation = 0.0f;
        public Color color = Color.White;
    }

    public class TextKeyframe : Keyframe
    {
        public uint printedLetters;
    }

}
