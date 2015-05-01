using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    enum MovementDirection
    {
        Left,
        Up,
        Right,
        Down
    }

  
    class Emiter
    {
        Texture2D texture;
        List<EmiterPart> emitedParts = new List<EmiterPart>();
        Vector2 startPosition;
        uint distance;
        int emitsDelayMs;
        int emitsDelayMsParts;
        int partsNum;
        float speed;
        Rectangle textureRectangle;
        Color color;
        TimeSpan phaseTimer;
        TimeSpan lastEmit;
        MovementDirection movementDirection;
        bool explode = false;

        bool emmitingPhase = false;
        int emitCounter = 0;
        int animationDelay = Config.DEFAULT_ANIMATION_SPEED;
        bool animationreverse = false;
        readonly int startOffset;
        Color[] partTextureArray;

        public Emiter(Texture2D texture, Vector2 startPosition, uint distance, float speed, MovementDirection movementDirection, Rectangle textureRectangle, int emitsDelayMs, int delayOffsetMs, Color color, int animationSpeed, bool explode = false, int emitsDelayMsParts = 0, int partsNum = 1, bool animationreverse = false)
        {
            this.texture = texture;
            this.startPosition = startPosition;
            this.distance = distance;
            this.textureRectangle = textureRectangle;
            this.emitsDelayMs = emitsDelayMs;
            this.emitsDelayMsParts = emitsDelayMsParts;
            this.partsNum = partsNum;
            this.color = color;
            this.movementDirection = movementDirection;
            this.speed = speed;
            this.explode = explode;
            this.animationDelay = animationSpeed;
            this.animationreverse = animationreverse;
            this.startOffset = delayOffsetMs;
            partTextureArray = Util.GetTextureArray(Util.BlitTexture(texture, new Rectangle(textureRectangle.X, textureRectangle.Y, textureRectangle.Width, Config.ANIM_FRAMES * textureRectangle.Height)), textureRectangle.Width, textureRectangle.Height * Config.ANIM_FRAMES);           
            phaseTimer = TimeSpan.FromMilliseconds(delayOffsetMs);            
        }

        
        public void Update(GameTime gameTime)
        {
            if (!emmitingPhase)
            {                
                phaseTimer += gameTime.ElapsedGameTime;
                if (phaseTimer.TotalMilliseconds > emitsDelayMs)
                {
                    phaseTimer = TimeSpan.Zero;
                    emmitingPhase = true;
                }
            }
            else
            {
                lastEmit += gameTime.ElapsedGameTime;
                if (lastEmit.TotalMilliseconds > emitsDelayMsParts)
                {
                    lastEmit = TimeSpan.Zero;
                    EmitPart();
                    emitCounter++;

                    if (emitCounter >= partsNum)
                    {
                        emitCounter = 0;
                        emmitingPhase = false;
                    }
                }
                
            }
            UpdateParts(gameTime);

        }

        public void UpdateParts(GameTime gameTime)
        {
            for (int i = 0; i < emitedParts.Count; i++)
            {
                if (emitedParts[i].Update(gameTime))
                {
                    emitedParts.RemoveAt(i);
                    i--; continue;
                }
            }
        }

        public void EmitPart()
        {
            emitedParts.Add(new EmiterPart(startPosition, distance, speed, movementDirection, texture, partTextureArray, textureRectangle, color, animationDelay, explode, animationreverse));
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            foreach (EmiterPart emitedPart in emitedParts)
            {
                emitedPart.Draw(spriteBatch, offset);                
            }
        }

        public bool CheckCollision(Rectangle boundingBox, out EmiterPart collidingPart)
        {
            collidingPart = null;

            foreach (EmiterPart emiterPart in emitedParts)
            {
                if (emiterPart.Collide(boundingBox))
                {
                    collidingPart = emiterPart;
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            emitedParts.Clear();
            phaseTimer = TimeSpan.FromMilliseconds(startOffset);
            emmitingPhase = false;
            emitCounter = 0;
            lastEmit = TimeSpan.Zero;
        }
    }
}
