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
    class PixelParticle
    {
        Texture2D texture;
        Vector2 position;
        double currentLifeMs;
        double maxLifeMs;
        double speedY;
        double speedX;
        double accY;
        double accX;
        Color color;
        bool gravityAffect;

        public bool enviroAffect;

        public Config.StandingType standingType = Config.StandingType.Pixel;

        public Vector2 enviroSpeed;

        public SoundEffectInstance hitSoundInstance;

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)Math.Ceiling(position.X), (int)Math.Ceiling(position.Y), 1, 1);
            }
            private set 
            {
                BoundingBox = value;
            }
        }

        public Map map;

        private TimeSpan gravityDelayTimer = TimeSpan.Zero;
        private int gravityDelay;
        private bool gravityEnabled = false;

        public PixelParticle(Vector2 position, double maxLifeMs, Vector2 speed, Vector2 acc, Color color, bool gravityAffect, Map map = null, bool enviroAffect = true, Config.StandingType standingType = Config.StandingType.Pixel, int gravityDelay = 0)
        {
            enviroSpeed = Vector2.Zero;
            this.texture = Globals.textureDictionary["pixel"];
            this.position = position;
            this.maxLifeMs = maxLifeMs;
            this.speedY = speed.Y;
            this.speedX = speed.X;
            this.currentLifeMs = 0.0;
            this.accY = acc.Y;
            this.accX = acc.X;
            this.color = color;
            this.gravityAffect = gravityAffect;
            this.map = map;
            this.enviroAffect = enviroAffect;
            this.standingType = standingType;
            this.gravityDelay = gravityDelay;
            hitSoundInstance = Globals.soundsDictionary["hit"];
        }

        public bool Update(GameTime gameTime)
        {
            if (maxLifeMs > 0.0)
            {
                currentLifeMs += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (currentLifeMs > maxLifeMs && maxLifeMs != 0.0)
                    return true;
            }

            if (!gravityEnabled)
            {
                gravityDelayTimer += gameTime.ElapsedGameTime;
                if (gravityDelayTimer.TotalMilliseconds > gravityDelay)
                    gravityEnabled = true;
            }

            if (gravityAffect && gravityEnabled)
            {
                speedY += Config.Player.GRAVITY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;
            }

            speedX += accX * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0 + enviroSpeed.X;
            speedY += accY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0 + enviroSpeed.Y;
            speedY = (float)MathHelper.Clamp((float)speedY, Config.PixelParticle.MAX_FLY_SPEED, Config.PixelParticle.MAX_FALL_SPEED);

            var tempRectangle = new Rectangle();
            float movingModifier = 0.0f;
            bool springy = false;

            position.X += (float)(speedX * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0); // move horizontally
            if (map != null && enviroAffect)
            {
                if (map.CheckCollisions(BoundingBox, Tile.Attributes.Solid, out tempRectangle))
                {
                    alignHorizontalToTile(tempRectangle);
                    speedX = accX = 0.0f;
                }
            }
                        
            position.Y += (float)(speedY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0); // move vertically
            if (map != null && enviroAffect)
            {
                
                if (map.CheckCollisions(BoundingBox, Tile.Attributes.Solid, out tempRectangle) ||    // solid block hit (from top or bottom)
                    (speedY > 0.0f && map.CheckPlatformCollisions(BoundingBox, out tempRectangle, out movingModifier, out springy, standingType)))  // platform hit, collision only when going down
                {
                    accX = Config.PixelParticle.HTORQUE_GROUND * (-speedX);
                    if (Math.Abs(accX) < Config.PixelParticle.HBRAKE)
                        accX = speedX = 0.0f;

                    alignVerticalToTile(tempRectangle);
                    accY = speedY = 0.0f;
                }
                else
                {
                    accX = Config.PixelParticle.HTORQUE_INAIR * (-speedX);
                }
            }
            enviroSpeed.X = movingModifier;

            /*if ((position.X) > Config.Map.WIDTH * Config.Tile.SIZE - 1)
                position.X = -1;
            else if (position.X < -1)
                position.X = Config.Map.WIDTH * Config.Tile.SIZE - 2;
            */
            if (position.Y > Config.SCREEN_HEIGHT_SCALED || position.X < 0 || position.X > Config.SCREEN_WIDTH_SCALED) // remove on out of screen
                return true;

            return false;
        }

        private void alignHorizontalToTile(Rectangle tileBox)
        {
            if (speedX > 0.0f)
                position.X = (float)(tileBox.Left - 1);
            else
                position.X = (float)tileBox.Right;
        }

        private void alignVerticalToTile(Rectangle tileBox)
        {
            if (speedY > 0.0f)
                position.Y = (float)(tileBox.Top - 1);
            else
                position.Y = (float)tileBox.Bottom;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(position.X + Config.DRAW_OFFSET_X, position.Y + Config.DRAW_OFFSET_Y), null, color, 0.0f, Vector2.Zero, 1, SpriteEffects.None, 0);            
            //spriteBatch.Draw(texture, new Vector2(position.X + Config.DRAW_OFFSET_X, position.Y + Config.DRAW_OFFSET_Y), color,);
        }
    }
}
