using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect.Bosses.Capman
{
    class PacmanBoss : Enemy
    {

        public PacmanBoss(Texture2D texture, Vector2 speed, Vector2 textureSize, int textureColumn, Vector2 startPosition,
                          bool reverse = true, bool blink = false, bool guardian = false, int offset = 0) :
            base(texture, speed, textureSize, textureColumn, startPosition, reverse, blink, guardian, offset) 
        { 
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Vector2(currentPosition.X + Config.DRAW_OFFSET_X, currentPosition.Y + Config.DRAW_OFFSET_Y),
                                                sourceRectangle, Color.White, 0.0f, Vector2.Zero,
                                                10.0f, (leftDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }
    }


}
