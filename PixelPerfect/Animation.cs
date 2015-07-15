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

        public int currentFrame
        {
            set
            {
                if (value >= frameCount)
                    return;
                _currentFrame = value;
            }
            get
            {
                return _currentFrame;
            }
        }

        private int _currentFrame;
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
            _currentFrame++;
            if (_currentFrame >= frameCount)
                _currentFrame = 0;
        }

        private void NextFrameReverse()
        {
            if (backward)
            {
                _currentFrame--;
                if (_currentFrame < 0)
                {
                    backward = false;
                    _currentFrame = 1;
                }
            }
            else
            {
                _currentFrame++;
                if (_currentFrame >= frameCount)
                {
                    _currentFrame = frameCount - 2;
                    backward = true;
                }
            }
        }


        public void Reset()
        {
            _currentFrame = 0;
            currentTime = TimeSpan.Zero;
        }
    }
}
