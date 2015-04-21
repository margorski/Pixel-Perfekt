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
    static class Globals
    {
        public static Map CurrentMap; 
        public static LevelState CurrentLevelState;
        public static GraphicsDeviceManager graphics;
        public static ContentManager content;
        public static bool upsideDown = false;
        public static Texture2D pixelTexture = null;
        public static Color backgroundColor = Color.Black;
        public static int color1 = -1;
        public static int color2 = -1;
        public static double SpeedModificator = 1.2;
        public static bool playSounds = false;
        public static SoundEffectInstance hitSoundInstance;
        public static GameStateMachine.GameStateManager gameStateManager;
        public static SpriteFont silkscreenFont;
        public static List<Color> colorList = Util.GetColorList();
    }
}
