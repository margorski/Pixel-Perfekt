using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace PixelPerfect.Cutscene
{
    public class Keyframe
    {        
        public Keyframe()
        {
        }

        public Keyframe(Keyframe keyframe)
        {
            this._time = TimeSpan.FromMilliseconds(keyframe._time.TotalMilliseconds);
            this.position = keyframe.position;
            this.origin = keyframe.origin;
            this.scale = keyframe.scale;
            this.rotation = keyframe.rotation;
            this.color = keyframe.color;
            this.printedLetters = keyframe.printedLetters;
            this.spriteEffect = keyframe.spriteEffect;
            this.animated = keyframe.animated;
        }

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
        public int printedLetters;
        public SpriteEffects spriteEffect = SpriteEffects.None;
        public bool animated = true;
    }
}
