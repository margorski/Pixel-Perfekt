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
    abstract class Item
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
    
    class Image : Item
    {
        public string textureName;

        protected Texture2D texture;

        public override void Init()
        {
            Sprite tempSprite;
            Texture2D tempTexture;

            if (!Globals.textureDictionary.TryGetValue(textureName, out tempTexture))
            {
                Globals.spritesDictionary.TryGetValue(textureName, out tempSprite);
                texture = tempSprite.texture;
            }
            else
            {
                texture = tempTexture;
            }
            
            base.Init();
        }

        public override bool Update(GameTime gameTime)
        {
            return base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (progress == 0.0f)
                return;

            if (keyframeList.Count <= 1 || currentKeyframe == keyframeList.Count-1)
            {
                spriteBatch.Draw(texture, keyframeList[currentKeyframe].position, null, keyframeList[currentKeyframe].color, keyframeList[currentKeyframe].rotation, keyframeList[currentKeyframe].origin, keyframeList[currentKeyframe].scale, keyframeList[currentKeyframe].spriteEffect, 0.0f);
                return;
            }

            var transitionKeyframe = TransitionKeyframe();
            spriteBatch.Draw(texture, transitionKeyframe.position, null, transitionKeyframe.color, transitionKeyframe.rotation, transitionKeyframe.origin, transitionKeyframe.scale, keyframeList[currentKeyframe].spriteEffect, 0.0f);            
        }        
    }

    class AnimatedImage : Image
    {
        protected Animation animation;
        public Rectangle startFrame;
        public int frameCount = 0;
        public int frameTime = Config.DEFAULT_ANIMATION_SPEED;
        public bool reverse = false;
        public int staticFrame = 1;

        public static AnimatedImage Player(Vector2 startPosition, Vector2 endPosition, TimeSpan startTime, float pixPerSecond = 5.0f, float scale = 1.0f)
        {
                AnimatedImage player = new Cutscene.AnimatedImage();
                bool goLeft = ((endPosition.X - startPosition.X) < 0.0f ? true : false);


                player.frameCount = Config.Player.ANIM_FRAMES;
                player.frameTime = Config.Player.ANIMATION_DELAY;
                player.reverse = true;
                player.startFrame = new Rectangle(0, Config.Player.HEIGHT, Config.Player.WIDTH * 1, Config.Player.HEIGHT);
                player.textureName = "player";

                var keyframe = new Cutscene.Keyframe();
                keyframe._time = startTime;
                keyframe.position = startPosition;
                keyframe.scale = scale;
                if (goLeft)
                    keyframe.spriteEffect = SpriteEffects.FlipHorizontally;
                player.keyframeList.Add(keyframe);
                keyframe = new Cutscene.Keyframe();
                keyframe.scale = scale;
                keyframe._time = startTime + TimeSpan.FromSeconds((endPosition - startPosition).Length() / pixPerSecond);
                keyframe.position = endPosition;
                if (goLeft)
                    keyframe.spriteEffect = SpriteEffects.FlipHorizontally;
                player.keyframeList.Add(keyframe);

                return player;
        }

        private Rectangle sourceRectangle
        {
            get
            {
                return new Rectangle(startFrame.X, startFrame.Y + (keyframeList[currentKeyframe].animated ? animation.currentFrame : staticFrame) * startFrame.Height, startFrame.Width, startFrame.Height);
            }
        }

        public override void Init()
        {                       
 	        base.Init();
            animation = new Animation(frameCount, frameTime, reverse);
        }

        public override bool Update(GameTime gameTime)
        {
            if (keyframeList[currentKeyframe].animated)
                animation.Update(gameTime);
            return base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (progress == 0.0f)
                return;

            if (keyframeList.Count <= 1 || currentKeyframe == keyframeList.Count - 1)
            {
                spriteBatch.Draw(texture, keyframeList[currentKeyframe].position, sourceRectangle, keyframeList[currentKeyframe].color, keyframeList[currentKeyframe].rotation, keyframeList[currentKeyframe].origin, keyframeList[currentKeyframe].scale, SpriteEffects.None, 0.0f);
                return;
            }

            var transitionKeyframe = TransitionKeyframe();
            spriteBatch.Draw(texture, transitionKeyframe.position, sourceRectangle, transitionKeyframe.color, transitionKeyframe.rotation, transitionKeyframe.origin, transitionKeyframe.scale, SpriteEffects.None, 0.0f);            
        }
    }
    
    class Text : Item
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
