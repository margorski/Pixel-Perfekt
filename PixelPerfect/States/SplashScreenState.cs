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
    class SplashScreenState : GameState
    {
        TimeSpan fadeTime = TimeSpan.FromMilliseconds(500.0);
        Texture2D logo;
        private Task task;

        Dictionary<string, string> textureDictionary = new Dictionary<string, string>()
        {
            {"pixel", "pixel"}, {"play", "menu\\play"}, {"music", "menu\\music"}, {"sound", "menu\\sound"}, {"back", "menu\\back"},
            {"info", "menu\\info"}, {"play2", "menu\\play2"}, {"skip", "menu\\skip"}, {"miniTileset", "tileset_mini"}, {"miniPlayer", "player_mini"},
            {"cool", "moods\\cool"}, {"happy", "moods\\happy"}, {"confused", "moods\\confused"}, {"shocked", "moods\\shocked"}, {"scared", "moods\\scared"},
            {"keylock", "keylock"}, {"coolLevel", "moods\\level_cool"}, {"happyLevel", "moods\\level_happy"}, {"confusedLevel", "moods\\level_confused"}, 
            {"shockedLevel", "moods\\level_shocked"}, {"scaredLevel", "moods\\level_scared"}, {"keylockLevel", "keylock_small"}, {"trophy", "menu\\trophy"},
            {"skull", "menu\\skull"}, {"clock", "menu\\clock"}, {"tap", "menu\\tap"}, {"next", "menu\\next"}, {"restart", "menu\\restart"},
            {"suit", "menu\\shirt"}, {"suitbutton", "menu\\suitebtn"}, {"suitbuttonlocked", "menu\\suitebtnlocked"}, {"miniDoor", "door_mini"}
        };

        public override void Enter(int previousStateId)
        {
            logo = Globals.content.Load<Texture2D>("logo");
        }

        public override void Exit(int nextStateId)
        {
        }
                
        public override void Suspend(int pushedStateId)
        {
        }

        public override void Resume(int poppedStateId)
        {
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            Vector2 position = new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - logo.Width / 2,
                                           Config.SCREEN_HEIGHT_SCALED / 2 - logo.Height / 2);
            spriteBatch.Draw(logo, position, Color.White);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            if (task == null)
            {
                task = new Task(() =>
                {
                    LoadContent();
                    RegisterStates();
                    Theme.ReloadTheme(World.LastActiveWorld());
                });
                task.Start();
            }

            if (task.IsCompleted)
            {
                Globals.gameStateManager.PopState();
                Globals.gameStateManager.PushState(Config.States.BACKGROUND);
                Globals.gameStateManager.PushState(Config.States.DUMMY);
                Globals.gameStateManager.PushState(Config.States.TITLESCREEN);
            }
        }

        private void RegisterStates()
        {
            var menuState = new TitlescreenState(Globals.gameStateManager);
            var backgroundLevel = Theme.CurrentTheme.level;
            backgroundLevel.scale = scale;
            var pauseState = new PauseState();
            menuState.scale = pauseState.scale = scale;
            var levelSelectState = new LevelSelectState(Globals.gameStateManager);
            levelSelectState.scale = scale;
            var backgroundState = new TextureBackgroundState(Theme.Cool.color1, Theme.Cool.color2);
            var dummyState = new DummyState();
            var worldSelectState = new WorldSelectState(Globals.gameStateManager);
            worldSelectState.scale = scale;
            var levelDetailsState = new LevelDetailsState(Globals.gameStateManager);
            levelDetailsState.scale = scale;
            var tapState = new TapState();
            tapState.scale = scale;
            var controlsState = new ControlsState();
            controlsState.scale = scale;
            var suitState = new SuitSelectState(Globals.gameStateManager);
            suitState.scale = scale;
            var suitUnlockedState = new SuitUnlockedState();
            var winState = new WinState();
            winState.scale = scale;

            Globals.gameStateManager.RegisterState(Config.States.WIN, winState);
            Globals.gameStateManager.RegisterState(Config.States.SUITUNLOCKED, suitUnlockedState);
            Globals.gameStateManager.RegisterState(Config.States.SUITSELECT, suitState);
            Globals.gameStateManager.RegisterState(Config.States.CONTROLS, controlsState);
            Globals.gameStateManager.RegisterState(Config.States.TAP, tapState);
            Globals.gameStateManager.RegisterState(Config.States.BACKGROUND, backgroundState);
            Globals.gameStateManager.RegisterState(Config.States.TITLESCREEN, menuState);
            Globals.gameStateManager.RegisterState(Config.States.PAUSE, pauseState);
            Globals.gameStateManager.RegisterState(Config.States.WORLDSELECT, worldSelectState);
            Globals.gameStateManager.RegisterState(Config.States.LEVELSELECT, levelSelectState);
            Globals.gameStateManager.RegisterState(Config.States.LEVELDETAILS, levelDetailsState);
            Globals.gameStateManager.RegisterState(Config.States.DUMMY, dummyState);
        }

        private void LoadContent()
        {
            Globals.silkscreenFont = Globals.content.Load<SpriteFont>("Silkscreen");
            Globals.tileset = new Tileset("tileset");
            LoadTextures();
            LoadSprites();
            LoadSounds();
            LoadMusic();
        }

        private void LoadTextures()
        {
            foreach (KeyValuePair<string, string> texture in textureDictionary)
            {
                if (!Globals.textureDictionary.ContainsKey(texture.Key))
                    Globals.textureDictionary.Add(texture.Key, Globals.content.Load<Texture2D>(texture.Value));
            }
        }
        
        private void LoadSprites()
        {
            Globals.spritesDictionary.Add("biggo_128x128", new Sprite("biggo_128x128", 128, 128, 2));
            Globals.spritesDictionary.Add("enemies_16x16", new Sprite("enemies_16x16", 16, 16));
            Globals.spritesDictionary.Add("enemies_32x32", new Sprite("enemies_32x32", 32, 32));
            Globals.spritesDictionary.Add("enemies_8x8", new Sprite("enemies_8x8", 8, 8));
            Globals.spritesDictionary.Add("king_48x48", new Sprite("king_48x48", 48, 48, 14));
            Globals.spritesDictionary.Add("player", new Sprite("player", 8, 16, 6));
        }

        private void LoadSounds()
        {
            Globals.soundsDictionary.Add("coin", Globals.content.Load<SoundEffect>(@"Sounds\Pickup_Coin8").CreateInstance());
            Globals.soundsDictionary.Add("jump", Globals.content.Load<SoundEffect>(@"Sounds\Jump4").CreateInstance());
            Globals.soundsDictionary.Add("explosion", Globals.content.Load<SoundEffect>(@"Sounds\Explosion9").CreateInstance());
            Globals.soundsDictionary.Add("randomize", Globals.content.Load<SoundEffect>(@"Sounds\Randomize3").CreateInstance());
            Globals.soundsDictionary.Add("hit", Globals.content.Load<SoundEffect>(@"Sounds\Hit_Hurt2").CreateInstance());
            Globals.soundsDictionary.Add("doors", Globals.content.Load<SoundEffect>(@"Sounds\Randomize2").CreateInstance());
            Globals.soundsDictionary["doors"].IsLooped = true;
            foreach (KeyValuePair<string, SoundEffectInstance> sfeffect in Globals.soundsDictionary)
                sfeffect.Value.Volume = 0.15f;
            Globals.soundsDictionary["hit"].Volume = 0.2f;
            Globals.soundsDictionary["coin"].Volume = 0.12f;
            Globals.soundsDictionary["doors"].Volume = 0.2f;
            Globals.soundsDictionary["doors"].Pitch = 0.5f;
        }

        private void LoadMusic()
        {
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\xylophone (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Elevator Music (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\8-bit loop (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Gasoline Rainbows (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Chippy Cloud Kid (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\ChipChippy (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Sad Song 1"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Dramatic Metal Entrance (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Chaotic Filth (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Chaotic Standoff (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Ring Leader (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Rising Sun (oriental with dance beats)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Vanguard Bouncy (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\wubby dancer (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\King Boss (loop)"));
        }

    }
}
