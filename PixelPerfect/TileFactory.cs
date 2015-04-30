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
 

    static class TileFactory
    {
        private static Texture2D tileTexture;
        private static Texture2D pixelTexture;
        private static Color[] colors =  {Color.Yellow,
                                          Color.Red,
                                          Color.Green,
                                          Color.Blue,
                                          Color.Purple,
                                          Color.Yellow,
                                          Color.Red,
                                          Color.Green,
                                          Color.Blue,
                                          Color.Purple};

        public static void Init(Texture2D texture, Texture2D ptexture)
        {
            tileTexture = texture;
            pixelTexture = ptexture;
        }

        private static int NormalizeType(int type)
        {
            if (type > 20)
            {
                type = (((type - 1) % 20) + 1);
            }
            return type;
        }

        public static Tile CreateTile(int type, Vector2 position, bool background = false)
        {
            UInt32 attributes = 0;

            if (type == (int)Config.TileType.NONE)
                attributes |= (UInt32)Tile.Attributes.NoDraw;

            if (background)
            {
                attributes |= (UInt32)Tile.Attributes.Background;
            }
            else
            {
                switch (NormalizeType(type))
                {
                    case (int)Config.TileType.KILLING_BUSH:
                    case (int)Config.TileType.KILLING_SPIKE:
                        attributes |= (UInt32)Tile.Attributes.Killing;
                        break;

                    case (int)Config.TileType.SOLID:
                    case (int)Config.TileType.BLINKING_SOLID:
                        attributes |= (UInt32)Tile.Attributes.Solid;
                        break;

                    case (int)Config.TileType.PLATFORM:
                    case (int)Config.TileType.BLINKING_PLATFORM:
                    case (int)Config.TileType.CRUSHY:
                    case (int)Config.TileType.SPRING:
                        attributes |= (UInt32)Tile.Attributes.Platform;
                        break;

                    case (int)Config.TileType.MOVING_LEFT:
                    case (int)Config.TileType.MOVING_RIGHT:
                        attributes |= (UInt32)Tile.Attributes.Platform;
                        attributes |= (UInt32)Tile.Attributes.Moving;
                        break;
                    case (int)Config.TileType.START_POSITION:
                        attributes |= (UInt32)Tile.Attributes.NoDraw;
                        break;
                    case (int)Config.TileType.KEY:
                        attributes |= (UInt32)Tile.Attributes.Collectible;
                        break;

                    case (int)Config.TileType.DOOR_LU:
                    case (int)Config.TileType.DOOR_MU:
                    case (int)Config.TileType.DOOR_RU:
                    case (int)Config.TileType.DOOR_LM:
                    case (int)Config.TileType.DOOR_RM:
                    case (int)Config.TileType.DOOR_LD:
                    case (int)Config.TileType.DOOR_MD:
                        attributes |= (UInt32)Tile.Attributes.Doors;
                        break;

                    case (int)Config.TileType.DOOR_MM_EXIT:
                        attributes |= (UInt32)Tile.Attributes.Doors;
                        attributes |= (UInt32)Tile.Attributes.DoorsMain;
                        break;
                }
            }

            Color color = Color.White;
            Color transpColor = Color.White;
            transpColor.A = 120;
            /*if (type >= (int)Config.TileType.DOOR_LU && type <= (int)Config.TileType.DOOR_RD)
                color = Color.Blue;
            else if (type < (int)Config.TileType.SOLID || type > colors.Length)
                color = Color.White;
            else
                color = colors[type - 1];
            */

            int typex = ((type - 1) % 20) + 1;
            int typey = ((type - 1) / 20);

            type = NormalizeType(type);
                 
            Rectangle sourceRectangle = new Rectangle(((int)typex - 1) * Config.Tile.SIZE, (int)typey * Config.Tile.SIZE, Config.Tile.SIZE, Config.Tile.SIZE);

            if (background)
                return new Tile(position, tileTexture, attributes, sourceRectangle, transpColor);
            else if (type == (int)Config.TileType.CRUSHY)
                return new CrushyTile(position, tileTexture, attributes, sourceRectangle, color);
            else if (type == (int)Config.TileType.KEY)
                return new CollectibleTile(position, tileTexture, pixelTexture, attributes, sourceRectangle, color, true);
            else if (type == (int)Config.TileType.MOVING_LEFT)
                return new MovingTile(position, tileTexture, attributes, sourceRectangle, color, -Config.Tile.MOVINGPLATFORM_SPEED);
            else if (type == (int)Config.TileType.MOVING_RIGHT)
                return new MovingTile(position, tileTexture, attributes, sourceRectangle, color, Config.Tile.MOVINGPLATFORM_SPEED);
            else if (type == (int)Config.TileType.BLINKING_PLATFORM || type == (int)Config.TileType.BLINKING_SOLID)
                return new BlinkingTile(position, tileTexture, attributes, sourceRectangle, color);
            else if (type == (int)Config.TileType.SPRING)
                return new SpringTile(position, tileTexture, attributes, sourceRectangle, color);
            else if ((attributes & (UInt32)Tile.Attributes.Doors) > 0)
                return new DiscoTile(position, tileTexture, attributes, sourceRectangle, color);
            else
                return new Tile(position, tileTexture, attributes, sourceRectangle, color);
        }
    }
}
