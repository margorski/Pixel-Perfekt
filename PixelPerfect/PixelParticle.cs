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
        bool scaled;
        
        public CrushyTile.StandingType standingType = CrushyTile.StandingType.Pixel;

        public Vector2 enviroSpeed;
        float scale
        {            
            get 
            {
                if (scaled)
                    return 1.0f + (1.0f - (float)((maxLifeMs - currentLifeMs) / maxLifeMs)) * 10.0f;
                else
                    return 1.0f;
            }
        }

        public Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)Math.Ceiling(position.X), (int)Math.Ceiling(position.Y), (int)scale, (int)scale);
            }
            private set 
            {
                BoundingBox = value;
            }
        }

        public Map map;

        public PixelParticle(Texture2D texture, Vector2 position, double maxLifeMs, Vector2 speed, Vector2 acc, Color color, bool gravityAffect, bool scaled, Map map = null)
        {
            enviroSpeed = Vector2.Zero;
            this.texture = texture;
            this.position = position;
            this.maxLifeMs = maxLifeMs;
            this.speedY = speed.Y;
            this.speedX = speed.X;
            this.currentLifeMs = 0.0;
            this.accY = acc.Y;
            this.accX = acc.X;
            this.color = color;
            this.gravityAffect = gravityAffect;
            this.scaled = scaled;
            this.map = map;
        }

        public bool Update(GameTime gameTime)
        {
            if (maxLifeMs > 0.0)
            {
                currentLifeMs += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (currentLifeMs > maxLifeMs && maxLifeMs != 0.0)
                    return true;
            }

            if (gravityAffect)
            {
                speedY += Config.Player.GRAVITY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;
            }

            speedX += accX * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0 + enviroSpeed.X;
            speedY += accY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0 + enviroSpeed.Y;
            speedY = (float)MathHelper.Clamp((float)speedY, Config.PixelParticle.MAX_FLY_SPEED, Config.PixelParticle.MAX_FALL_SPEED);

            var tempRectangle = new Rectangle();
            float movingModifier = 0.0f;

            position.X += (float)(speedX * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0); // move horizontally
            if (map != null)
            {
                if (map.CheckCollisions(BoundingBox, Tile.Attributes.Solid, out tempRectangle))
                {
                    alignHorizontalToTile(tempRectangle);
                    speedX = accX = 0.0f;
                }
            }
                        
            position.Y += (float)(speedY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0); // move vertically
            if (map != null)
            {
                
                if (map.CheckCollisions(BoundingBox, Tile.Attributes.Solid, out tempRectangle) ||    // solid block hit (from top or bottom)
                    (speedY > 0.0f && map.CheckPlatformCollisions(BoundingBox, out tempRectangle, out movingModifier, standingType)))  // platform hit, collision only when going down
                {
                    accX = Config.PixelParticle.HTORQUE * (-speedX);
                    if (Math.Abs(accX) < Config.PixelParticle.HBRAKE)
                        accX = speedX = 0.0f;

                    alignVerticalToTile(tempRectangle);
                    accY = speedY = 0.0f;
                }
            }
            enviroSpeed.X = movingModifier;

            if (position.X < 0 || position.X >= Config.SCREEN_WIDTH_SCALED || position.Y > Config.SCREEN_HEIGHT_SCALED) // remove on out of screen
                return true;

            return false;
        }

        private void alignHorizontalToTile(Rectangle tileBox)
        {
            if (speedX > 0.0f)
                position.X = (float)(tileBox.Left - scale);
            else
                position.X = (float)tileBox.Right;
        }

        private void alignVerticalToTile(Rectangle tileBox)
        {
            if (speedY > 0.0f)
                position.Y = (float)(tileBox.Top - scale);
            else
                position.Y = (float)tileBox.Bottom;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(position.X + Config.DRAW_OFFSET_X, position.Y + Config.DRAW_OFFSET_Y), null, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            //spriteBatch.Draw(texture, new Vector2(position.X + Config.DRAW_OFFSET_X, position.Y + Config.DRAW_OFFSET_Y), color,);
        }
    }
}
