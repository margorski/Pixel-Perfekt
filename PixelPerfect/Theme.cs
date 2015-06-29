using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class Theme
    {
        public int color1                { private set; get; }
        public int color2                { private set; get; }
        public int music                 { private set; get; }
        public LevelState level          { private set; get; }

        public Theme (int color1, int color2, int music, LevelState level)
        {
            this.color1 = color1;
            this.color2 = color2;
            this.level = level;
            this.music = music;
        }
                
        public static Theme Cool = new Theme(25, 24, 2, new LevelState("", "menu\\menu_cool", true, Config.SCALE_FACTOR));
        public static Theme Happy = new Theme(46, 50, 3, new LevelState("", "menu\\menu_happy", true, Config.SCALE_FACTOR));
        public static Theme Confused = new Theme(23, 135, 4, new LevelState("", "menu\\menu_confused", true, Config.SCALE_FACTOR)); // clud
        public static Theme Shocked = new Theme(26, 26, 5, new LevelState("", "menu\\menu_shocked", true, Config.SCALE_FACTOR)); //chip chippy
        public static Theme Scared = new Theme(9, 82, 11, new LevelState("", "menu\\menu_scared", true, Config.SCALE_FACTOR)); //rising
        public static Theme[] Themes = { Cool, Happy, Confused, Shocked, Scared };
        public static Theme CurrentTheme = Themes[0];        

        public static void ReloadTheme (int id)
        {
            Theme.CurrentTheme = Theme.Themes[id];
            var backgroundState = (TextureBackgroundState)Globals.gameStateManager.GetState(Config.States.BACKGROUND);
            backgroundState.ChangeColors(Theme.CurrentTheme.color1, Theme.CurrentTheme.color2);
            var titleScreenState = (TitlescreenState)Globals.gameStateManager.GetState(Config.States.TITLESCREEN);
            titleScreenState.backgroundLevel = Theme.CurrentTheme.level;
        }
    }
}
