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
    public class Item
    {
        public List<Keyframe> keyframeList = new List<Keyframe>();
        public string textureFile;        

        private Texture2D texture;
        private int currentKeyframe = 0;
        private TimeSpan currentTime = TimeSpan.Zero;

        public void Init()
        {
            texture = Globals.content.Load<Texture2D>(textureFile);
            keyframeList.Sort((kf1, kf2) => kf1._time.CompareTo(kf2._time));
        }

        public void Update(GameTime gameTime)
        {
            currentTime += gameTime.ElapsedGameTime;

            if (keyframeList.Count <= 1 || currentKeyframe >= keyframeList.Count - 1)
                return;

            if (currentTime >= keyframeList[currentKeyframe+1]._time)
                currentKeyframe++;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (keyframeList.Count <= 1)
            {
                spriteBatch.Draw(texture, keyframeList[0].position, null,  keyframeList[0].color,  keyframeList[0].rotation,  keyframeList[0].origin,  keyframeList[0].scale, SpriteEffects.None, 0.0f);
                return;
            }

            var startTime = keyframeList[currentKeyframe]._time;
            var endTime = keyframeList[currentKeyframe + 1]._time;

            float keyframeProgress = (float)((currentTime - startTime).TotalMilliseconds / (endTime - startTime).TotalMilliseconds);

            var position = keyframeList[currentKeyframe].position + (keyframeList[currentKeyframe + 1].position - keyframeList[currentKeyframe].position) * keyframeProgress;
            var color = Color.Lerp(keyframeList[currentKeyframe].color, keyframeList[currentKeyframe + 1].color, keyframeProgress);
            var rotation = keyframeList[currentKeyframe].rotation + (keyframeList[currentKeyframe + 1].rotation - keyframeList[currentKeyframe].rotation) * keyframeProgress;
            var origin = keyframeList[currentKeyframe].origin + (keyframeList[currentKeyframe + 1].origin - keyframeList[currentKeyframe].origin) * keyframeProgress;
            var scale = keyframeList[currentKeyframe].scale + (keyframeList[currentKeyframe + 1].scale - keyframeList[currentKeyframe].scale) * keyframeProgress;

            spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, SpriteEffects.None, 0.0f);
        }

    }
}
