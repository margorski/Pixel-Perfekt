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
            public const UInt32 Background = 1 << 8;
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
        protected Color[] textureArray;

        // Methods
        public Tile(Vector2 position, Texture2D texture, UInt32 attributes, int type, Color color)
        {
            this.position = position;
            this.texture = texture;
            this.attributes = attributes;
            this.sourceRect = CalculateSourceRectangle(type);
            this.color = color;

            if (((attributes & Attributes.NoDraw) > 0) || (type == 0))
                textureArray = new Color[Config.Tile.SIZE * Config.Tile.SIZE];
            else
                textureArray = Globals.tileset.tileTextureArray[type - 1];
        }
        public virtual void Update(GameTime gameTime) 
        { 
        }
        public virtual void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            if ((attributes & (UInt32)Tile.Attributes.Killing) > 0)
                this.color = Globals.enemiesColor;
            else
               this.color = Globals.tilesColor;

            if ((attributes & (UInt32)Tile.Attributes.Background) > 0)
                this.color.A = (byte)(this.color.A * 0.25);

            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X + (int)offset.X, boundingBox.Y + Config.DRAW_OFFSET_Y + (int)offset.Y, boundingBox.Width, boundingBox.Height), sourceRect, color);
        }

        private Rectangle CalculateSourceRectangle(int type)
        {
            int typex = ((type - 1) % 20) + 1;
            int typey = ((type - 1) / 20);


            return new Rectangle(((int)typex - 1) * Config.Tile.SIZE, (int)typey * Config.Tile.SIZE, Config.Tile.SIZE, Config.Tile.SIZE);
        }

        public virtual void Reset()
        {
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }
        public void SetAttributes(UInt32 attributes)
        {
            this.attributes = attributes;
        }

        public Color[] GetTileArray()
        {
            return textureArray;
        }
    }

    class DiscoTile : Tile
    {
        public bool disco = false;

        private TimeSpan blinkTime = TimeSpan.Zero;
        private Color[] blinkColors = { Color.Red, Color.Violet, Color.Green, Color.Blue, Color.Yellow };

        private const int blinkMs = 200;
        private float currentScale = 1.0f;
        private float currentRotation = 0.0f;
        private TimeSpan scaleTimer = TimeSpan.Zero;

        public DiscoTile(Vector2 position, Texture2D texture, UInt32 attributes, int type, Color color)
            : base(position, texture, attributes, type, color)
        {
            
        }

        public override void Update(GameTime gameTime)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            if (disco)
            {
                scaleTimer += gameTime.ElapsedGameTime;
                if (scaleTimer.TotalMilliseconds >= 2000)                                   
                    scaleTimer = TimeSpan.Zero;
                currentScale = (float)Math.Sin((scaleTimer.TotalMilliseconds / 1000.0) * Math.PI * 2) / 2 + 1.5f;
                //currentRotation = (float)(Math.Sin((scaleTimer.TotalMilliseconds / 2000.0) * Math.PI * 2) * Math.PI / 5);

                blinkTime += gameTime.ElapsedGameTime;
                if (blinkTime.TotalMilliseconds >= (blinkMs * blinkColors.Length))
                    blinkTime = TimeSpan.Zero;
                color = blinkColors[(int)(blinkTime.TotalMilliseconds / blinkMs)];

            }            
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            spriteBatch.Draw(texture, new Vector2(boundingBox.X + Config.DRAW_OFFSET_X + (int)offset.X + boundingBox.Width / 2, boundingBox.Y + Config.DRAW_OFFSET_Y + (int)offset.Y + boundingBox.Height / 2), sourceRect, (disco ? color : Globals.tilesColor), currentRotation, new Vector2(boundingBox.Width / 2, boundingBox.Height / 2), currentScale, SpriteEffects.None, 0.0f);
        }

        public override void Reset()
        {
            currentScale = 1.0f;
            currentRotation = 0.0f;
            scaleTimer = TimeSpan.Zero;
            disco = false;
        }
    }

    class MovingTile : Tile
    {
        // Private
        private Animation animation = new Animation(8, 30, false);

        public float movingSpeed { private set; get; }

        // Public
        public MovingTile(Vector2 position, Texture2D texture, UInt32 attributes, int type, Color color, float movingSpeed) : base(position, texture, attributes, type, color)
        {  this.movingSpeed = movingSpeed; }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            animation.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            Rectangle animSourceRect = sourceRect;
            animSourceRect.Y += animation.currentFrame * Config.Tile.SIZE;

            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X + (int)offset.X, boundingBox.Y + Config.DRAW_OFFSET_Y + (int)offset.Y, boundingBox.Width, boundingBox.Height), animSourceRect, Globals.tilesColor);
        }

        public override void Reset()
        {
            animation.Reset();
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

        public BlinkingTile(Vector2 position, Texture2D texture, UInt32 attributes, int type, Color color, int blinkTime = Config.Tile.DEFAULT_BLINK_TIME, int offsetTime = 0) : base(position, texture, attributes, type, color)
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
                if (platform)
                    attributes |= Attributes.Platform;
                if (solid)
                    attributes |= Attributes.Solid;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            base.Draw(spriteBatch, offset);
        }

        public override void Reset()
        {
            hidden = false;
            SetHidden(hidden);
            currentBlinkTime = 0;
        }
    }


    class CrushyTile : Tile
    {

        // Private
        private float tileHeight = (float)Config.Tile.SIZE;
        private bool standing = false;
        private int pixelCount = 0;
        private TimeSpan pixelEmitTime = TimeSpan.Zero;

        // Public
        public override Rectangle boundingBox
        {
            get
            {
                var height = (int)Math.Round(tileHeight);
                return new Rectangle((int)position.X, (int)position.Y + (Config.Tile.SIZE - height), Config.Tile.SIZE, height);
            }
        }
        
        // Methods
        public CrushyTile(Vector2 position, Texture2D texture, UInt32 attributes, int type, Color color) : base(position, texture, attributes, type, color)
        { }

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
                sourceRect.Height = (int)Math.Round(tileHeight);

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

        public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X + (int)offset.X, boundingBox.Y + Config.DRAW_OFFSET_Y + (int)offset.Y, boundingBox.Width, boundingBox.Height), sourceRect, Globals.tilesColor);
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
            int x = Globals.rnd.Next(Config.Tile.SIZE);
            Globals.CurrentLevelState.AddPixelParticle(new PixelParticle(
                               new Vector2(boundingBox.Left + x, boundingBox.Bottom),
                               0.0f,//rnd.Next(Config.PixelParticle.PIXELPARTICLE_LIFETIME_MIN, Config.PixelParticle.PIXELPARTICLE_LIFETIME_MAX), 
                               new Vector2(0.0f, Config.PixelParticle.SPEED),
                               new Vector2(0.0f, 0.0f), Globals.tilesColor, true, Globals.CurrentMap));
        }

        public override void Reset()
        {
            standing = false;
            pixelCount = 0;
            pixelEmitTime = TimeSpan.Zero;
            tileHeight = (float)Config.Tile.SIZE;
            sourceRect.Height = (int)Math.Round(tileHeight);
            attributes = Tile.Attributes.Platform; 
        }
    }

    class SpringTile : Tile
    {
        private bool springy = true;
        private Rectangle sourceRectInactive;

        public override Rectangle boundingBox
        {
            get
            {
                if (!springy)
                    return new Rectangle((int)position.X, (int)position.Y, Config.Tile.SIZE, Config.Tile.SIZE);
                else
                    return new Rectangle((int)position.X, (int)position.Y + 3, Config.Tile.SIZE, 5);
            }
        }

        public SpringTile(Vector2 position, Texture2D texture, UInt32 attributes, int type, Color color) : base(position, texture, attributes, type, color)
        {                
            sourceRectInactive = new Rectangle(sourceRect.X, sourceRect.Y + Config.Tile.SIZE, Config.Tile.SIZE, Config.Tile.SIZE);
        }

        public bool StandOn()
        {
            if (springy)
            {
                springy = false;
                return true;
            }

            return false;
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;
            
            if (!springy)
                spriteBatch.Draw(texture, new Rectangle((int)position.X + Config.DRAW_OFFSET_X + (int)offset.X, (int)position.Y + Config.DRAW_OFFSET_Y + (int)offset.Y, boundingBox.Width, boundingBox.Height), sourceRectInactive, Globals.tilesColor);
            else
                spriteBatch.Draw(texture, new Rectangle((int)position.X + Config.DRAW_OFFSET_X + (int)offset.X, (int)position.Y + Config.DRAW_OFFSET_Y + (int)offset.Y, Config.Tile.SIZE, Config.Tile.SIZE), sourceRect, Globals.tilesColor);
        }

        public override void Reset()
        {
            springy = true;
        }
    }

    class CollectibleTile : Tile
    {
        private const int blinkMs = 200;
        private bool active = true;
        private bool emmiting = false;

        private TimeSpan pixelEmitTime = TimeSpan.Zero;
        private Texture2D pixelTexture;

        private TimeSpan blinkTime = TimeSpan.Zero;
        private Color[] blinkColors = { Color.Red, Color.Violet, Color.Green, Color.Blue, Color.Yellow };

        private float currentScale = 1.0f;
        private float currentRotation = 0.0f;
        private TimeSpan scaleTimer = TimeSpan.Zero;

        public CollectibleTile(Vector2 position, Texture2D texture, Texture2D pixelTexture, UInt32 attributes, int type, Color color, bool emmiting) : base(position, texture, attributes, type, color)
        { 
            this.pixelTexture = pixelTexture;
            this.emmiting = emmiting;
        }
 
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!active)
                return;

            if ((attributes & Attributes.NoDraw) > 0)
                return;

            scaleTimer += gameTime.ElapsedGameTime;
            if (scaleTimer.TotalMilliseconds >= 2000.0)
                scaleTimer = TimeSpan.Zero;
            currentScale = (float)Math.Sin((scaleTimer.TotalMilliseconds / 1000.0) * Math.PI * 2) / 6.0f + 1.0f;
            currentRotation = (float)(Math.Sin((scaleTimer.TotalMilliseconds / 2000.0) * Math.PI * 2) * Math.PI / 4);

            blinkTime += gameTime.ElapsedGameTime;
            if (blinkTime.TotalMilliseconds >= (blinkMs * blinkColors.Length))
                blinkTime = TimeSpan.Zero;
            color = blinkColors[(int)(blinkTime.TotalMilliseconds / blinkMs)];

            if (!emmiting)
                return;

            pixelEmitTime += gameTime.ElapsedGameTime;
            if (pixelEmitTime.TotalMilliseconds > Config.PixelParticle.COLLECTIBLE_EMITTIME)
            {
                pixelEmitTime = TimeSpan.Zero;
                EmitPixel();
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            if ((attributes & Attributes.NoDraw) > 0)
                return;

            spriteBatch.Draw(texture, new Vector2(boundingBox.X + Config.DRAW_OFFSET_X + (int)offset.X + boundingBox.Width / 2, boundingBox.Y + Config.DRAW_OFFSET_Y + (int)offset.Y + boundingBox.Height / 2), sourceRect, color, currentRotation, new Vector2(boundingBox.Width / 2, boundingBox.Height / 2), currentScale, SpriteEffects.None, 0.0f);
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
        public void EmitPixel()
        {
            int x = Globals.rnd.Next(Config.Tile.SIZE - 4);
            float accy = 15.0f + (float)Globals.rnd.NextDouble() * 2.0f;

            Globals.CurrentLevelState.AddPixelParticle(new PixelParticle(
                               new Vector2(boundingBox.Left + 2 + x, boundingBox.Bottom - 2),
                               Globals.rnd.Next(Config.PixelParticle.PIXELPARTICLE_LIFETIME_MIN, Config.PixelParticle.PIXELPARTICLE_LIFETIME_MAX), 
                               new Vector2(0.0f, 0.0f),
                               new Vector2(0.0f, accy), color, false, Globals.CurrentMap, false, Config.StandingType.NoImpact));
        }

        public override void Reset()
        {
            active = true;       
            pixelEmitTime = TimeSpan.Zero;
            currentScale = 1.0f;
            currentRotation = 0.0f;
            scaleTimer = TimeSpan.Zero;
            attributes = Tile.Attributes.Collectible;
        }
    }
}
