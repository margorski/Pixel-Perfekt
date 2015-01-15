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

        public PixelParticle(Texture2D texture, Vector2 position, double maxLifeMs, Vector2 speed, Vector2 acc, Color color, bool gravityAffect, bool scaled)
        {
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
        }

        public bool Update(GameTime gameTime)
        {
            currentLifeMs += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (currentLifeMs > maxLifeMs)
                return true;

            if (gravityAffect)
            {
                speedY += Config.Player.GRAVITY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;
            }
            speedY += accY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;
            speedX += accX * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0;

            position.Y += (float)(speedY * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0);
            position.X += (float)(speedX * gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0);
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float scale = 1.0f;

            if (scaled)
                scale = 1.0f + (1.0f - (float)((maxLifeMs - currentLifeMs) / maxLifeMs)) * 10.0f;

            spriteBatch.Draw(texture, new Vector2(position.X + Config.DRAW_OFFSET_X, position.Y + Config.DRAW_OFFSET_Y), null, color, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
            //spriteBatch.Draw(texture, new Vector2(position.X + Config.DRAW_OFFSET_X, position.Y + Config.DRAW_OFFSET_Y), color,);
        }
    }
}
