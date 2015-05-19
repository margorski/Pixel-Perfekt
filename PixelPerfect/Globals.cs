﻿using System;
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
    static class Globals
    {
        public static Map CurrentMap; 
        public static LevelState CurrentLevelState;
        public static GraphicsDeviceManager graphics;
        public static ContentManager content;
        public static bool upsideDown = false;
        public static Texture2D pixelTexture = null;
        public static Color backgroundColor = Color.Black;
        public static double SpeedModificator = 1.4;        
        public static GameStateMachine.GameStateManager gameStateManager;
        public static SpriteFont silkscreenFont;
        public static List<Color> colorList = Util.GetColorList();
        public static Color enemiesColor = Color.White;
        public static Color emitersColor = Color.White;
        public static Color tilesColor = Color.White;
        public static Random rnd = new Random();
        public static List<Song> backgroundMusicList = new List<Song>();
        public static Dictionary<string, Sprite> spritesDictionary = new Dictionary<string, Sprite>();
        public static Dictionary<string, SoundEffectInstance> soundsDictionary = new Dictionary<string, SoundEffectInstance>();
        public static Tileset tileset;
        public static bool musicEnabled = true;
        public static bool soundEnabled = true;
    }
}
