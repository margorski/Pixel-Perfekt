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
    class Animation
    {
        private int frameCount;
        private int frameTime;
        private bool reverse;

        private int currentFrame;
        private TimeSpan currentTime;
        private bool backward;

        public Animation(int frameCount, int frameTime, bool reverse)
        {
            currentTime = TimeSpan.Zero;
            this.frameCount = frameCount;
            this.frameTime = frameTime;
            this.reverse = reverse;
        }

        public void Update(GameTime gameTime)
        {
            currentTime += gameTime.ElapsedGameTime;
            if (currentTime.TotalMilliseconds > frameTime)
            {
                currentTime = TimeSpan.Zero;
                if (!reverse)
                    NextFrameNormal();
                else
                    NextFrameReverse();
            }
        }

        private void NextFrameNormal()
        {
            currentFrame++;
            if (currentFrame >= frameCount)
                currentFrame = 0;
        }

        private void NextFrameReverse()
        {
            if (backward)
            {
                currentFrame--;
                if (currentFrame < 0)
                {
                    backward = false;
                    currentFrame = 1;
                }
            }
            else
            {
                currentFrame++;
                if (currentFrame >= frameCount)
                {
                    currentFrame = frameCount - 2;
                    backward = true;
                }
            }
        }

        public int GetCurrentFrame()
        {
            return currentFrame;
        }

        public void Reset()
        {
            currentFrame = 0;
            currentTime = TimeSpan.Zero;
        }
    }
}
