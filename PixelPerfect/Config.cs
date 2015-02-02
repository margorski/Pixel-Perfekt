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
    static class Config
    {
        /*public const int SCREEN_WIDTH = 400;
        public const int SCREEN_HEIGHT = 240;
         */
        public const int SCREEN_WIDTH_SCALED = 264; //SCREEN_WIDTH / 2;
        public const int SCREEN_HEIGHT_SCALED = 160; //SCREEN_HEIGHT / 2;
        public const int SCALE_FACTOR = 3;
        public const int ANIMATION_SPEED_FACTOR = 135;
        public const int DRAW_OFFSET_X = 0;
        public const int DRAW_OFFSET_Y = 0;
        public const bool CENTER_PIVOT = true;

        public struct States
        {
            public const int MENU = 100;
            public const int LEVEL = 200;
            public const int PAUSE = 300;
            public const int TEXT = 400;
        }

        public struct Map
        {
            public const int WIDTH = 34;
            public const int HEIGHT = 17;
            public const int BLINK_MS = 200;
        }

        public struct Tile
        {
            public const int SIZE = 8;
            public const float CRUSH_SPEED = 3.0f;
            public const float CRUSH_PIXEL_COUNT = 45.0f;
            public const float MOVINGPLATFORM_SPEED = 25.0f;
        }

        public struct PixelParticle
        {
            public const float SPEED = 10.0f;
            public const float EMITTIME = 50.0f;
            public const float HTORQUE_GROUND = 2.0f;
            public const float HTORQUE_INAIR = 0.3f;
            public const float HBRAKE = 0.5f;
            public const float MAX_FALL_SPEED = 70.0f;
            public const float MAX_FLY_SPEED = -150.0f;
            public const int MAX_EXPLOSION_MAGNITUDE = 70;
        }

        public struct Player
        {
            public const int BOOMCOLOR_TIME_MS = 20;
            public const int TRYJUMP_RESETTIME = 200;
            public const int STOPTIME_REVERSE_MS = 300;
            public const int WIDTH = 8;
            public const int HEIGHT = 16;
            public const float JUMP_SPEED = -75.0f;
            public const float HTORQUE_INAIR = 5.5f; 
            public const float GRAVITY = 140.0f;
            public const float MAX_FALL_SPEED = 30.0f;
            public const float MAX_FALL_DISTANCE = Tile.SIZE * 5;
            public const float MOVE_SPEED = 30.0f;
            public const int ANIMATION_DELAY = 70;
        }

        public struct Enemy
        {
            public const int DEFAULT_BLINK_TIME_MS = 500;
        }

        public enum StandingType
        {
            NoImpact = 0,
            Player,
            Pixel
        }

        public enum TileType
        {
            START_POSITION = 20,
            NONE = 0,
            SOLID,
            PLATFORM,
            CRUSHY,
            MOVING_LEFT,
            MOVING_RIGHT,
            KEY,
            KILLING_BUSH,
            KILLING_SPIKE,
            DOOR_LU,
            DOOR_MU,
            DOOR_RU,
            DOOR_LM,
            DOOR_MM_EXIT,
            DOOR_RM,
            DOOR_LD,
            DOOR_MD,
            DOOR_RD
        }

        public enum TriggerType
        {
            NONE = 0,
            PUSHSTATE = 1,
            POPSTATE = 2,
            CHANGESTATE = 3
        }

        public struct LayerType
        {
            public const int ENEMY = 1;
            public const int EMITER = 2;
            public const int TRIGGER = 3;
        }

        public struct Hud
        {
            public const int HUD_HEIGHT = 3 * Tile.SIZE;
            public const int AVATAR_POSITION_Y = Config.SCREEN_HEIGHT_SCALED - HUD_HEIGHT;
            public const int AVATAR_POSITION_X = 20;
            public const int TEXT_POSITION_Y = AVATAR_POSITION_Y;            
            public const int COLLECTIBLES_X = Config.SCREEN_WIDTH_SCALED;
            public const int COLLECTIBLES_Y = TEXT_POSITION_Y + 12;
            public const int COLLECTIBLES_SPACE = 10;
            public const int TEXTSTATE_POSITION_X = 4;
            public const int TEXTSTATE_POSITION_Y = TEXT_POSITION_Y;
            public const int TEXTSTATE_LETTERTIME_MS = 50;
        }
    }
}
