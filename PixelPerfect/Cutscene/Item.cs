using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PixelPerfect.Cutscene
{
    public abstract class Item
    {
        public List<Keyframe> keyframeList = new List<Keyframe>();

        protected int currentKeyframe = 0;
        protected TimeSpan currentTime = TimeSpan.Zero;
        protected virtual float progress
        {
            get
            {
                if (keyframeList.Count <= 1)
                    return 0.0f;
                var _progress =  (float)((currentTime - keyframeList[currentKeyframe]._time).TotalMilliseconds / (keyframeList[currentKeyframe + 1]._time - keyframeList[currentKeyframe]._time).TotalMilliseconds);

                _progress = MathHelper.Clamp(_progress, 0.0f, 1.0f);
                
                return _progress;
            }
            private set {}
        }

        public virtual void Init()
        {            
            keyframeList.Sort((kf1, kf2) => kf1._time.CompareTo(kf2._time));
        }

        public virtual bool Update(GameTime gameTime)
        {
            currentTime += gameTime.ElapsedGameTime;

            if (currentTime >= keyframeList[keyframeList.Count - 1]._time)
                return false;

            if (keyframeList.Count <= 1 || currentKeyframe >= keyframeList.Count - 1)
                return true;

            if (currentTime >= keyframeList[currentKeyframe+1]._time)
                currentKeyframe++;

            return true;
        }

        protected virtual Keyframe TransitionKeyframe()
        {
            Keyframe transitionKeyframe = new Keyframe();

            if (keyframeList.Count <= 1)
                return transitionKeyframe;

            transitionKeyframe.position = keyframeList[currentKeyframe].position + (keyframeList[currentKeyframe + 1].position - keyframeList[currentKeyframe].position) * progress;
            transitionKeyframe.color = Color.Lerp(keyframeList[currentKeyframe].color, keyframeList[currentKeyframe + 1].color, progress);
            transitionKeyframe.rotation = keyframeList[currentKeyframe].rotation + (keyframeList[currentKeyframe + 1].rotation - keyframeList[currentKeyframe].rotation) * progress;
            transitionKeyframe.origin = keyframeList[currentKeyframe].origin + (keyframeList[currentKeyframe + 1].origin - keyframeList[currentKeyframe].origin) * progress;
            transitionKeyframe.scale = keyframeList[currentKeyframe].scale + (keyframeList[currentKeyframe + 1].scale - keyframeList[currentKeyframe].scale) * progress;
            transitionKeyframe.printedLetters = (int)(keyframeList[currentKeyframe].printedLetters + (keyframeList[currentKeyframe + 1].printedLetters - keyframeList[currentKeyframe].printedLetters) * progress);

            return transitionKeyframe;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    
    }
    
    public class Image : Item
    {
        public string textureFile;

        protected Texture2D texture;

        public override void Init()
        {
            texture = Globals.content.Load<Texture2D>(textureFile);
            base.Init();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (progress == 0.0f)
                return;

            if (keyframeList.Count <= 1 || currentKeyframe == keyframeList.Count-1)
            {
                spriteBatch.Draw(texture, keyframeList[currentKeyframe].position, null, keyframeList[currentKeyframe].color, keyframeList[currentKeyframe].rotation, keyframeList[currentKeyframe].origin, keyframeList[currentKeyframe].scale, SpriteEffects.None, 0.0f);
                return;
            }

            var transitionKeyframe = TransitionKeyframe();
            spriteBatch.Draw(texture, transitionKeyframe.position, null, transitionKeyframe.color, transitionKeyframe.rotation, transitionKeyframe.origin, transitionKeyframe.scale, SpriteEffects.None, 0.0f);            
        }        
    }

    public class Text : Item
    {
        public string text = "";

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (progress == 0.0f)
                return;

            if (keyframeList.Count <= 1 || currentKeyframe == keyframeList.Count - 1)
            {
                spriteBatch.DrawString(Globals.silkscreenFont, text.Substring(0, keyframeList[currentKeyframe].printedLetters), keyframeList[currentKeyframe].position, keyframeList[currentKeyframe].color, keyframeList[currentKeyframe].rotation, keyframeList[currentKeyframe].origin, keyframeList[currentKeyframe].scale, SpriteEffects.None, 0.0f);
                return;
            }

            var transitionKeyframe = TransitionKeyframe();            
            spriteBatch.DrawString(Globals.silkscreenFont, text.Substring(0, transitionKeyframe.printedLetters), transitionKeyframe.position, transitionKeyframe.color, transitionKeyframe.rotation, transitionKeyframe.origin, transitionKeyframe.scale, SpriteEffects.None, 0.0f);
        }
    }
}
