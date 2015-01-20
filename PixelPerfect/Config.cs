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
        public const int SCREEN_WIDTH_SCALED = 200; //SCREEN_WIDTH / 2;
        public const int SCREEN_HEIGHT_SCALED = 120; //SCREEN_HEIGHT / 2;
        public const int SCALE_FACTOR = 4;
        public const int ANIMATION_SPEED_FACTOR = 135;
        public const int DRAW_OFFSET_X = 4;
        public const int DRAW_OFFSET_Y = 0;

        public struct States
        {
            public const int MENU = 100;
            public const int LEVEL = 200;
            public const int PAUSE = 300;
            public const int TEXT = 400;
        }

        public struct Map
        {
            public const int WIDTH = 32;
            public const int HEIGHT = 16;
            public const int BLINK_MS = 200;
        }

        public struct Tile
        {
            public const int SIZE = 6;
            public const float CRUSH_SPEED = 6.0f;
            public const float MOVINGPLATFORM_SPEED = 25.0f;
        }

        public struct PixelParticle
        {
            public const float PIXELPARTICLE_SPEED = 10.0f;
            public const float PIXELPARTICLE_EMITTIME = 50.0f;
            public const int PIXELPARTICLE_LIFETIME_MIN = 700;
            public const int PIXELPARTICLE_LIFETIME_MAX = 1000;
            public const int PIXELPARTICLE_PLAYER_LIFETIME_MIN = 700;
            public const int PIXELPARTICLE_PLAYER_LIFETIME_MAX = 2500;
        }

        public struct Player
        {
            public const int BOOMCOLOR_TIME_MS = 40;
            public const int TRYJUMP_RESETTIME = 200;
            public const int STOPTIME_REVERSE_MS = 300;
            public const int WIDTH = 6;
            public const int HEIGHT = 12;
            public const float JUMP_SPEED = -65.0f;
            public const float GRAVITY = 140.0f;
            public const float MAX_FALL_SPEED = 30.0f;
            public const float MAX_FALL_DISTANCE = 25.0f;
            public const float MOVE_SPEED = 30.0f;
            public const int ANIMATION_DELAY = 70;
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
            public const int TEXT_POSITION_Y = 98;
            public const int COLLECTIBLES_X = Config.SCREEN_WIDTH_SCALED;
            public const int COLLECTIBLES_Y = 110;
            public const int COLLECTIBLES_SPACE = 5;
            public const int TEXTSTATE_POSITION_X = 10;
            public const int TEXTSTATE_POSITION_Y = TEXT_POSITION_Y;
            public const int TEXTSTATE_LETTERTIME_MS = 50;
        }
    }
}
