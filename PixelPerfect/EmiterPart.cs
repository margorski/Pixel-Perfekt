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
    class EmiterPart
    {
        enum MovePhase
        {
            StartStretch,
            Move,
            EndSquizz
        }

        Animation animation;
        Color color;
        Vector2 position;
        Vector2 endPosition;
        Vector2 speed;
        Vector2 size;
        int maxWidth;
        int maxHeight;
        Rectangle textureRectangle;        
        Texture2D texture;
        MovementDirection movementDirection;
        MovePhase movePhase;
        bool explode = false;
        List<PixelParticle> pixelContainer;
        Map emitMap = null;
        bool randomizeColors = false;

        Color[] textureArray;

        public Rectangle boundingBox
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            }
        }

        private Rectangle sourceRectangle
        {
            get
            {
                var rectangle = AdjustTextureRectangle();
                rectangle.Y += animation.currentFrame * textureRectangle.Height;
                return rectangle;                
            }
        }

        public EmiterPart() { }

        public EmiterPart(Vector2 position, uint distance, float speed, MovementDirection movementDirection, Texture2D texture, Color[] textureArray, Rectangle textureRectangle, Color color, int animationDelay, List<PixelParticle> pixelContainer, Map emitMap, bool explode = false, bool animationreverse = false, bool randomColors = false)
        {
            this.position = position;
            this.texture = texture;
            this.textureRectangle = textureRectangle;
            this.movementDirection = movementDirection;
            maxWidth = textureRectangle.Width;
            maxHeight = textureRectangle.Height;
            movePhase = MovePhase.StartStretch;
            this.color = color;
            this.explode = explode;
            this.pixelContainer = pixelContainer;
            this.emitMap = emitMap;
            this.randomizeColors = randomizeColors;

            animation = new Animation(4, animationDelay, animationreverse);

            this.textureArray = textureArray;//Util.GetTextureArray(Util.BlitTexture(texture, textureRectangle), textureRectangle.Width, textureRectangle.Height * Config.ANIM_FRAMES);
            InitializeSize();
            InitializeSpeed(speed);
            InitializeEndPosition(distance);
        }

        public bool Update(GameTime gameTime)
        {
            animation.Update(gameTime);

            switch (movePhase)
            {
                case MovePhase.StartStretch:
                    StretchPart(gameTime.ElapsedGameTime.TotalSeconds);
                    break;

                case MovePhase.Move:
                    if (MovePart(gameTime.ElapsedGameTime.TotalSeconds))
                        return true;
                    break;

                case MovePhase.EndSquizz:
                    if (SquezePart(gameTime.ElapsedGameTime.TotalSeconds))
                        return true;
                    break;
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            AdjustTextureRectangle();
            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X + (int)offset.X, boundingBox.Y + Config.DRAW_OFFSET_Y + (int)offset.Y, boundingBox.Width, boundingBox.Height)
                                                    , sourceRectangle, Globals.emitersColor);
        }

        public bool Collide(Rectangle boundingBox)
        {
            return this.boundingBox.Intersects(boundingBox);
        }

        public void InitializeEndPosition(uint distance)
        {
            if (movementDirection == MovementDirection.Left)
                endPosition = new Vector2(position.X - distance, position.Y);
            else if (movementDirection == MovementDirection.Right)
                endPosition = new Vector2(position.X + distance, position.Y);
            else if (movementDirection == MovementDirection.Up)
                endPosition = new Vector2(position.X, position.Y - distance);
            else
                endPosition = new Vector2(position.X, position.Y + distance);
        }

        public void InitializeSize()
        {
            if (movementDirection == MovementDirection.Left || movementDirection == MovementDirection.Right)
                this.size = new Vector2(1.0f, maxHeight);
            else
                this.size = new Vector2(maxWidth, 1.0f);
        }

        public void InitializeSpeed(float speed)
        {
            float x, y;
            x = y = 0.0f;

            switch (movementDirection)
            {
                case MovementDirection.Up:
                    y = -Math.Abs(speed);
                    break;
                case MovementDirection.Down:
                    y = Math.Abs(speed);
                    break;
                case MovementDirection.Left:
                    x = -Math.Abs(speed);
                    break;
                case MovementDirection.Right:
                    x = Math.Abs(speed);
                    break;
            }
            this.speed = new Vector2(x, y);   
        }

        public bool MovePart(double deltaSeconds)
        {
            position += speed * new Vector2((float)deltaSeconds, (float)deltaSeconds);

            bool phaseEnd = false;
            switch (movementDirection)
            {
                case MovementDirection.Left:
                    if (boundingBox.Left <= endPosition.X)
                        phaseEnd = true;
                    break;
                case MovementDirection.Right:
                    if (boundingBox.Right >= endPosition.X)
                        phaseEnd = true;
                    break;
                case MovementDirection.Up:
                    if (boundingBox.Top <= endPosition.Y)
                        phaseEnd = true;
                    break;
                case MovementDirection.Down:
                    if (boundingBox.Bottom >= endPosition.Y)
                        phaseEnd = true;
                    break;
            }

            if (phaseEnd)
            {
                movePhase = MovePhase.EndSquizz;
                if (explode)
                {
                    PixelExplosion();
                    return true;
                }
            }
            return false;
        }

        public void StretchPart(double deltaSeconds)
        {
            Vector2 deltaSecondsVector = new Vector2((float)deltaSeconds, (float)deltaSeconds);
            Vector2 absSpeed = new Vector2(Math.Abs(speed.X), Math.Abs(speed.Y));
            size += absSpeed * deltaSecondsVector;
            if (movementDirection == MovementDirection.Left || movementDirection == MovementDirection.Up)
                position += speed * deltaSecondsVector;

            if (movementDirection == MovementDirection.Left || movementDirection == MovementDirection.Right)
            {
                if (size.X >= maxWidth)
                {
                    size.X = maxWidth;
                    movePhase = MovePhase.Move;
                }
            }
            else
            {
                if (size.Y >= maxHeight)
                {
                    size.Y = maxHeight;
                    movePhase = MovePhase.Move;
                }
            }
        }

        public bool SquezePart(double deltaSeconds)
        {
            Vector2 minusSpeed = new Vector2(-Math.Abs(speed.X), -Math.Abs(speed.Y));
            Vector2 deltaSecondsVector = new Vector2((float)deltaSeconds, (float)deltaSeconds);
            size += minusSpeed * deltaSecondsVector;
            if (movementDirection == MovementDirection.Right || movementDirection == MovementDirection.Down)
                position += speed * deltaSecondsVector;

            if (movementDirection == MovementDirection.Left || movementDirection == MovementDirection.Right)
            {
                if (size.X < 1.0f) // part is not visible (casting to int is equal 0)
                    return true;
            }
            else
            {
                if (size.Y < 1.0f) // part is not visible (casting to int is equal 0)
                    return true;
            }
            return false;
        }

        private Rectangle AdjustTextureRectangle()
        {
            
            switch (movePhase)
            {
                case MovePhase.Move:
                    return textureRectangle;

                case MovePhase.StartStretch:
                    return AdjustTextureRectangleStretch();

                case MovePhase.EndSquizz:
                    return AdjustTextureRectangleSquizz();
            }

            return Rectangle.Empty;
        }

        private Rectangle AdjustTextureRectangleStretch()
        {
            switch (movementDirection)
            {
                case MovementDirection.Left:
                    return new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    (int)size.X, textureRectangle.Height);

                case MovementDirection.Right:
                    return new Rectangle(textureRectangle.X + textureRectangle.Width - (int)size.X,
                                                    textureRectangle.Y, 
                                                    (int)size.X, 
                                                    textureRectangle.Height);

                case MovementDirection.Down:
                    return new Rectangle(textureRectangle.X,
                                                    textureRectangle.Y + textureRectangle.Height - (int)size.Y,
                                                    textureRectangle.Width,
                                                    (int)size.Y);

                case MovementDirection.Up:
                    return new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    textureRectangle.Width, (int)size.Y);
            }

            return Rectangle.Empty;
        }

        private Rectangle AdjustTextureRectangleSquizz()
        {
            switch (movementDirection)
            {
                case MovementDirection.Right:
                    return new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    (int)size.X, textureRectangle.Height);

                case MovementDirection.Left:
                    return new Rectangle(textureRectangle.X + textureRectangle.Width - (int)size.X,
                                                    textureRectangle.Y,
                                                    (int)size.X,
                                                    textureRectangle.Height);

                case MovementDirection.Up:
                    return new Rectangle(textureRectangle.X,
                                                    textureRectangle.Y + textureRectangle.Height - (int)size.Y,
                                                    textureRectangle.Width,
                                                    (int)size.Y);

                case MovementDirection.Down:
                    return new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    textureRectangle.Width, (int)size.Y);

            }

            return Rectangle.Empty;
        }

        public void PixelExplosion()
        {
            if (Globals.soundEnabled)
                Globals.soundsDictionary["explosion"].Play();
            Color[] textureColors = GetCurrentFrameArray();         

            for (int i = 0; i < textureColors.Length; i++)
            {
                if (textureColors[i].A == 255)
                {
                    Vector2 boomCenter = position + new Vector2(size.X / 2, size.Y / 2);
                    Vector2 pixPos = position + new Vector2(i % size.X, i / size.X);
                    pixPos.Y-=4;
                    Vector2 pixSpeed = (pixPos - boomCenter) * Globals.rnd.Next(0, Config.PixelParticle.MAX_EXPLOSION_MAGNITUDE);
                    Vector2 acc = Vector2.Zero;//new Vector2(Globals.rnd.Next(-1000, 1000), Globals.rnd.Next(-1000, 1000));

                    AddPixelParticle(new PixelParticle(pixPos,
                                    0.0f,//Config.PixelParticle.PIXELPARTICLE_PLAYER_LIFETIME_MAX,
                                    pixSpeed, acc, Config.boomColors[Globals.rnd.Next(Config.boomColors.Length)], true, emitMap));
                }
            }
        }

        public Color[] GetCurrentFrameArray()
        {
            int frameSizeInArray = (int)(textureRectangle.Width * textureRectangle.Height);
            Color[] currentFrameArray = new Color[frameSizeInArray];
            Array.Copy(textureArray, frameSizeInArray * animation.currentFrame, currentFrameArray, 0, frameSizeInArray);
            return currentFrameArray;            
        }

        private void AddPixelParticle(PixelParticle pixelParticle)
        {
            pixelContainer.Add(pixelParticle);

            while (pixelContainer.Count > Config.PixelParticle.MAX_PARTICLES_LEVEL)
                pixelContainer.RemoveAt(Globals.rnd.Next(pixelContainer.Count));
        }
    }

}
