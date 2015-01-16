﻿using System;
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
    class EmiterPart
    {
        enum MovePhase
        {
            StartStretch,
            Move,
            EndSquizz
        }

        Color color;
        Vector2 position;
        Vector2 endPosition;
        Vector2 speed;
        Vector2 size;
        int maxWidth;
        int maxHeight;
        Rectangle textureRectangle;
        Rectangle sourceRectangle;
        Texture2D texture;
        MovementDirection movementDirection;
        MovePhase movePhase;
        public Rectangle boundingBox
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            }
        }

        public EmiterPart() { }

        public EmiterPart(Vector2 position, uint distance, float speed, MovementDirection movementDirection, Texture2D texture, Rectangle textureRectangle, Color color)
        {
            this.position = position;
            this.texture = texture;
            this.textureRectangle = sourceRectangle = textureRectangle;
            this.movementDirection = movementDirection;
            maxWidth = textureRectangle.Width;
            maxHeight = textureRectangle.Height;
            movePhase = MovePhase.StartStretch;
            this.color = color;
            
            InitializeSize();
            InitializeSpeed(speed);
            InitializeEndPosition(distance);
        }

        public bool Update(double deltaSeconds)
        {
            switch (movePhase)
            {
                case MovePhase.StartStretch:
                    StretchPart(deltaSeconds);
                    break;

                case MovePhase.Move:
                    MovePart(deltaSeconds);
                    break;

                case MovePhase.EndSquizz:
                    if (SquezePart(deltaSeconds))
                        return true;
                    break;
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            AdjustTextureRectangle();
            spriteBatch.Draw(texture, new Rectangle(boundingBox.X + Config.DRAW_OFFSET_X, boundingBox.Y + Config.DRAW_OFFSET_Y, boundingBox.Width, boundingBox.Height)
													, sourceRectangle, color);
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

        public void MovePart(double deltaSeconds)
        {
            position += speed * new Vector2((float)deltaSeconds, (float)deltaSeconds);

            switch (movementDirection)
            {
                case MovementDirection.Left:
                    if (boundingBox.Left <= endPosition.X)
                        movePhase = MovePhase.EndSquizz;
                    break;
                case MovementDirection.Right:
                    if (boundingBox.Right >= endPosition.X)
                        movePhase = MovePhase.EndSquizz;
                    break;
                case MovementDirection.Up:
                    if (boundingBox.Top <= endPosition.Y)
                        movePhase = MovePhase.EndSquizz;
                    break;
                case MovementDirection.Down:
                    if (boundingBox.Bottom >= endPosition.Y)
                        movePhase = MovePhase.EndSquizz;
                    break;
            }
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

        public void AdjustTextureRectangle()
        {
            
            switch (movePhase)
            {
                case MovePhase.Move:
                    sourceRectangle = textureRectangle;
                    break;

                case MovePhase.StartStretch:
                    AdjustTextureRectangleStretch();
                    break;

                case MovePhase.EndSquizz:
                    AdjustTextureRectangleSquizz();
                    break;
            }
        }

        public void AdjustTextureRectangleStretch()
        {
            switch (movementDirection)
            {
                case MovementDirection.Left:
                    sourceRectangle = new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    (int)size.X, textureRectangle.Height);
                    break;
                case MovementDirection.Right:
                    sourceRectangle = new Rectangle(textureRectangle.X + textureRectangle.Width - (int)size.X,
                                                    textureRectangle.Y, 
                                                    (int)size.X, 
                                                    textureRectangle.Height);
                    break;

                case MovementDirection.Down:
                    sourceRectangle = new Rectangle(textureRectangle.X,
                                                    textureRectangle.Y + textureRectangle.Height - (int)size.Y,
                                                    textureRectangle.Width,
                                                    (int)size.Y);
                    break;
                case MovementDirection.Up:
                    sourceRectangle = new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    textureRectangle.Width, (int)size.Y);
                    break;
            }
        }

        public void AdjustTextureRectangleSquizz()
        {
            switch (movementDirection)
            {
                case MovementDirection.Right:
                    sourceRectangle = new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    (int)size.X, textureRectangle.Height);
                    break;
                case MovementDirection.Left:
                    sourceRectangle = new Rectangle(textureRectangle.X + textureRectangle.Width - (int)size.X,
                                                    textureRectangle.Y,
                                                    (int)size.X,
                                                    textureRectangle.Height);
                    break;

                case MovementDirection.Up:
                    sourceRectangle = new Rectangle(textureRectangle.X,
                                                    textureRectangle.Y + textureRectangle.Height - (int)size.Y,
                                                    textureRectangle.Width,
                                                    (int)size.Y);
                    break;
                case MovementDirection.Down:
                    sourceRectangle = new Rectangle(textureRectangle.X, textureRectangle.Y,
                                                    textureRectangle.Width, (int)size.Y);
                    break;
            }
        }

        public Texture2D GetCurrentFrameTexture(GraphicsDeviceManager graphic)
        {
            return Util.BlitTexture(graphic, texture, sourceRectangle, false);
        }
    }

}