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
    class Tile
    {
        // Static, Enums
        public static class Attributes
        {
            public const UInt32 Solid = 1 << 0;
            public const UInt32 Platform = 1 << 1; // half solid, collision from top,
            public const UInt32 Killing = 1 << 2;
            public const UInt32 NoDraw = 1 << 3;
            public const UInt32 Collectible = 1 << 4;
            public const UInt32 Doors = 1 << 5;
            public const UInt32 DoorsMain = 1 << 6;
            public const UInt32 Moving = 1 << 7;
        }

        // Public
        public Vector2 position         { get; protected set; }
        public UInt32 attributes        { get; protected set; }
        public virtual Rectangle boundingBox
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, Config.Tile.SIZE, Config.Tile.SIZE);
            }
        }

        // Protected
        protected Texture2D texture;
        protected Rectangle sourceRect;
        protected Color color;

        // Methods
        public Tile(Vector2 position, Texture2D texture, UInt32 attributes, Rectangle sourceRect, Color color)
        {
            this.position = position;
            this.texture = texture;
            this.attributes = attributes;
            this.sourceRect = sourceRect;
            this.color = color;
        }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X, boundingBox.Y + Config.DRAW_OFFSET_Y, boundingBox.Width, boundingBox.Height), sourceRect, color);
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }
        public void SetAttributes(UInt32 attributes)
        {
            this.attributes = attributes;
        }

        public Texture2D GetCurrentFrameTexture(GraphicsDeviceManager graphic)
        {
            return Util.BlitTexture(graphic, texture, sourceRect, false);
        }
    }

    class MovingTile : Tile
    {
        // Private
        private Animation animation = new Animation(6, 30, false);

        public float movingSpeed { private set; get; }

        // Public
        public MovingTile(Vector2 position, Texture2D texture, UInt32 attributes, Rectangle sourceRect, Color color, float movingSpeed) : base(position, texture, attributes, sourceRect, color)
        {  this.movingSpeed = movingSpeed; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            animation.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            sourceRect.Y = animation.GetCurrentFrame() * Config.Tile.SIZE;
            base.Draw(spriteBatch);
        }
    }

    class BlinkingTile : Tile
    {
        private int blinkTime = 0;
        private double currentBlinkTime = 0;
        private bool hidden = false;
        private bool solid = false;
        private bool platform = false;
        public override Rectangle boundingBox
        {
            get
            {
                if (hidden)
                    return new Rectangle(0, 0, 0, 0);
                else
                    return new Rectangle((int)position.X, (int)position.Y, Config.Tile.SIZE, Config.Tile.SIZE);
            }
        }

        public BlinkingTile(Vector2 position, Texture2D texture, UInt32 attributes, Rectangle sourceRect, Color color, int blinkTime = Config.Tile.DEFAULT_BLINK_TIME, int offsetTime = 0) : base(position, texture, attributes, sourceRect, color)
        {
            this.blinkTime = blinkTime;
            currentBlinkTime = offsetTime % blinkTime;
            hidden = (((offsetTime / blinkTime) % 2) > 0 ? true : false);
            solid = (attributes & Attributes.Solid) != 0 ? true : false;
            platform = (attributes & Attributes.Platform) != 0 ? true : false;
            SetHidden(hidden);
        }

        public override void Update(GameTime gameTime)
        {
            currentBlinkTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (currentBlinkTime >= blinkTime)
            {
                hidden = !hidden;
                SetHidden(hidden);
                currentBlinkTime = 0;
            }

            base.Update(gameTime);
        }

        private void SetHidden(bool value)
        {
            if (value)
            {
                attributes |= Attributes.NoDraw;
                if (solid)
                    attributes &= ~Attributes.Solid;
                if (platform)
                    attributes &= ~Attributes.Platform;
            }
            else
            {
                attributes &= ~Attributes.NoDraw;                
                attributes |= Attributes.Platform;
                attributes |= Attributes.Solid;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }


    class CrushyTile : Tile
    {

        // Private
        private float tileHeight = (float)Config.Tile.SIZE;
        private bool standing = false;
        private int pixelCount = 0;
        private TimeSpan pixelEmitTime = TimeSpan.Zero;
        private Random rnd = new Random();
        private Texture2D pixelTexture;

        // Public
        public override Rectangle boundingBox
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y + (Config.Tile.SIZE - (int)tileHeight), Config.Tile.SIZE, (int)tileHeight);
            }
        }
        
        // Methods
        public CrushyTile(Vector2 position, Texture2D texture, Texture2D pixelTexture, UInt32 attributes, Rectangle sourceRect, Color color) : base(position, texture, attributes, sourceRect, color)
        { this.pixelTexture = pixelTexture; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (standing)
            {
                float timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                tileHeight -= timeFactor * Config.Tile.CRUSH_SPEED;
                tileHeight = MathHelper.Clamp(tileHeight, 0.0f, (float)Config.Tile.SIZE);
                standing = false;
                pixelCount = 0;
                sourceRect.Height = (int)tileHeight;

                if (sourceRect.Height == 0)
                {
                    attributes &= ~Attributes.Platform; // unset platform flag
                    attributes |= Attributes.NoDraw; //setting nodraw flag
                }

                pixelEmitTime += gameTime.ElapsedGameTime;
                if (pixelEmitTime.TotalMilliseconds > Config.PixelParticle.EMITTIME)
                {
                    pixelEmitTime = TimeSpan.Zero;
                    EmitPixel();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X, boundingBox.Y + Config.DRAW_OFFSET_Y, boundingBox.Width, boundingBox.Height), sourceRect, color);
        }

        public void StandOn(Config.StandingType standingType = Config.StandingType.Player)
        {
            switch (standingType)
            {
                case Config.StandingType.Player:
                    standing = true;
                    break;

                case Config.StandingType.Pixel:
                    pixelCount++;
                    if (pixelCount >= Config.Tile.CRUSH_PIXEL_COUNT)
                        standing = true;
                    break;
            }                            
        }

        public void EmitPixel()
        {
            int x = rnd.Next(Config.Tile.SIZE);
            Globals.CurrentLevelState.AddPixelParticle(new PixelParticle(pixelTexture, 
                               new Vector2(boundingBox.Left + x, boundingBox.Bottom),
                               0.0f,//rnd.Next(Config.PixelParticle.PIXELPARTICLE_LIFETIME_MIN, Config.PixelParticle.PIXELPARTICLE_LIFETIME_MAX), 
                               new Vector2(0.0f, Config.PixelParticle.SPEED),
                               new Vector2(0.0f, 0.0f), color, true, Globals.CurrentMap));
        }
    }

    class CollectibleTile : Tile
    {
        private const int blinkMs = 200;
        private bool active = true;

        private TimeSpan blinkTime = TimeSpan.Zero;
        private Color[] blinkColors = { Color.Red, Color.Violet, Color.Green, Color.Blue, Color.Yellow };

        public CollectibleTile(Vector2 position, Texture2D texture, UInt32 attributes, Rectangle sourceRect, Color color) : base(position, texture, attributes, sourceRect, color) { }
 
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!active)
                return;
         
            blinkTime += gameTime.ElapsedGameTime;
            if (blinkTime.TotalMilliseconds >= (blinkMs * blinkColors.Length))
                blinkTime = TimeSpan.Zero;
            color = blinkColors[(int)(blinkTime.TotalMilliseconds / blinkMs)];
        }

        public void Activate()
        {
            active = true;
        }

        public void Deactivate()
        {
            active = false;
            color = Color.Gray;
        }
    }
}
