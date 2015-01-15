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

    class CrushyTile : Tile
    {
        // Private
        private float tileHeight = (float)Config.Tile.SIZE;
        private bool standing = false;
        private List<PixelParticle> pixelParticles = new List<PixelParticle>();
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

            for (int i = 0; i < pixelParticles.Count; i++)
            {
                if (pixelParticles[i].Update(gameTime))
                {
                    pixelParticles.RemoveAt(i);
                    i--;
                }
            }

            if (standing)
            {
                float timeFactor = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                tileHeight -= timeFactor * Config.Tile.CRUSH_SPEED;
                tileHeight = MathHelper.Clamp(tileHeight, 0.0f, (float)Config.Tile.SIZE);
                standing = false;
                sourceRect.Height = (int)tileHeight;

                if (sourceRect.Height == 0)
                {
                    attributes &= ~Attributes.Platform; // unset platform flag
                    attributes |= Attributes.NoDraw; //setting nodraw flag
                }

                pixelEmitTime += gameTime.ElapsedGameTime;
                if (pixelEmitTime.TotalMilliseconds > Config.PixelParticle.PIXELPARTICLE_EMITTIME)
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

            foreach (PixelParticle pixelParticle in pixelParticles)
                pixelParticle.Draw(spriteBatch);
        }

        public void StandOn()
        {
            standing = true;
        }

        public void EmitPixel()
        {
            int x = rnd.Next(Config.Tile.SIZE);
            pixelParticles.Add(new PixelParticle(pixelTexture, 
                               new Vector2(boundingBox.Left + x, boundingBox.Bottom),
                               rnd.Next(Config.PixelParticle.PIXELPARTICLE_LIFETIME_MIN, Config.PixelParticle.PIXELPARTICLE_LIFETIME_MAX), 
                               new Vector2(0.0f, Config.PixelParticle.PIXELPARTICLE_SPEED), 
                               new Vector2(0.0f, 0.0f), color, false, false));
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
