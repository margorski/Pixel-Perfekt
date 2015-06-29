using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System.IO;
using System.IO.IsolatedStorage;

namespace PixelPerfect
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch spriteBatch2;
        SpriteBatch spriteBatch3;

        GameStateManager gameStateManager;

        private float scale = 1.0f;
        //private float margin = 0.0f;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Globals.graphics = graphics;
            Globals.content = Content;
            //graphics.PreferredBackBufferHeight = Config.SCREEN_HEIGHT;
            //graphics.PreferredBackBufferWidth = Config.SCREEN_WIDTH;

#if !WINDOWS
            graphics.IsFullScreen = true;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
#else
            this.IsMouseVisible = true;                   
#endif
            ScaleScreen();
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(111111);
            
        }


        private void ScaleScreen()
        {
#if !WINDOWS
            // autoscaling part from web
            int? scaleFactor = null;
            var content = App.Current.Host.Content;
            var scaleFactorProperty = content.GetType().GetProperty("ScaleFactor");

            if (scaleFactorProperty != null)
                scaleFactor = scaleFactorProperty.GetValue(content, null) as int?;

            if (scaleFactor == null)
                scaleFactor = 100; // 100% WVGA resolution

            scale = (int)scaleFactor / 100.0f;            
            //scale *= 2.0f; // applying game scaling
            /*
            if (scaleFactor == 150)
            { 
                // 150% for 720P (scaled to 1200x720 viewport, not 1280x720 screen-res)
                // Centered letterboxing - move Margin.Left to the right by 0.5*(1280-1200)/scale
                GamePage.Instance.XnaSurface.Margin = new System.Windows.Thickness(40 / scale, 0, 0, 0);                
            }
            */
            /*System.Windows.Media.ScaleTransform scaleTransform = new System.Windows.Media.ScaleTransform();
            scaleTransform.ScaleX = scaleTransform.ScaleY = 6.0f;//scale * 2.0f;
            // The auto-scaling magic happens on the following line!
            GamePage.Instance.XnaSurface.RenderTransform = scaleTransform;*/
#endif
            scale *= Config.SCALE_FACTOR;
        }



        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            Globals.renderTarget = new RenderTarget2D(GraphicsDevice, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED,
                                                      false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            Savestate.Init();
            
#if !WINDOWS
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("music"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("music", true);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            Globals.musicEnabled = (bool)IsolatedStorageSettings.ApplicationSettings["music"];

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("sound"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("sound", true);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            Globals.soundEnabled = (bool)IsolatedStorageSettings.ApplicationSettings["sound"];
#endif

            gameStateManager = new GameStateManager();
            Globals.gameStateManager = gameStateManager;
            Globals.worlds = World.LoadWorlds();

            var menuState = new TitlescreenState(gameStateManager);
            var backgroundLevel = Theme.CurrentTheme.level; 
            backgroundLevel.scale = scale;            
            var pauseState = new PauseState();             
            menuState.scale = pauseState.scale = scale;
            var levelSelectState = new LevelSelectState(gameStateManager);
            levelSelectState.scale = scale;

            var backgroundState = new TextureBackgroundState(Theme.Cool.color1, Theme.Cool.color2);

            var dummyState = new DummyState();

            var worldSelectState = new WorldSelectState(gameStateManager);
            worldSelectState.scale = scale;

            var levelDetailsState = new LevelDetailsState(gameStateManager);
            levelDetailsState.scale = scale;

            var tapState = new TapState();
            tapState.scale = scale;

            var controlsState = new ControlsState();
            controlsState.scale = scale;

            gameStateManager.RegisterState(Config.States.CONTROLS, controlsState);
            gameStateManager.RegisterState(Config.States.TAP, tapState);
            gameStateManager.RegisterState(Config.States.BACKGROUND, backgroundState);
            gameStateManager.RegisterState(Config.States.TITLESCREEN, menuState);
            gameStateManager.RegisterState(Config.States.PAUSE, pauseState);
            gameStateManager.RegisterState(Config.States.WORLDSELECT, worldSelectState);
            gameStateManager.RegisterState(Config.States.LEVELSELECT, levelSelectState);
            gameStateManager.RegisterState(Config.States.LEVELDETAILS, levelDetailsState);
            gameStateManager.RegisterState(Config.States.DUMMY, dummyState);

            Theme.ReloadTheme(World.LastActiveWorld());

            gameStateManager.PushState(Config.States.BACKGROUND);
            gameStateManager.PushState(Config.States.DUMMY);
            gameStateManager.PushState(Config.States.TITLESCREEN);
                       
        }

        protected override void LoadContent()
        {
            Globals.spriteBatch = spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch2 = new SpriteBatch(GraphicsDevice);
            spriteBatch3 = new SpriteBatch(GraphicsDevice);

            MediaPlayer.IsRepeating = true;
            
            Globals.textureDictionary.Add("pixel", Content.Load<Texture2D>("pixel"));
            Globals.textureDictionary.Add("play", Content.Load<Texture2D>("menu\\play"));
            Globals.textureDictionary.Add("music", Content.Load<Texture2D>("menu\\music"));
            Globals.textureDictionary.Add("sound", Content.Load<Texture2D>("menu\\sound"));
            Globals.textureDictionary.Add("logo", Content.Load<Texture2D>("menu\\logo"));
            Globals.textureDictionary.Add("back", Content.Load<Texture2D>("menu\\back"));
            Globals.textureDictionary.Add("info", Content.Load<Texture2D>("menu\\info"));
            Globals.textureDictionary.Add("play2", Content.Load<Texture2D>("menu\\play2"));
            Globals.textureDictionary.Add("skip", Content.Load<Texture2D>("menu\\skip"));
            Globals.textureDictionary.Add("miniTileset", Content.Load<Texture2D>("tileset_mini"));
            Globals.textureDictionary.Add("miniPlayer", Content.Load<Texture2D>("player_mini"));
            Globals.textureDictionary.Add("miniDoor", Content.Load<Texture2D>("door_mini"));
            Globals.textureDictionary.Add("cool", Content.Load<Texture2D>("moods\\cool"));
            Globals.textureDictionary.Add("happy", Content.Load<Texture2D>("moods\\happy"));
            Globals.textureDictionary.Add("confused", Content.Load<Texture2D>("moods\\confused"));
            Globals.textureDictionary.Add("shocked", Content.Load<Texture2D>("moods\\shocked"));
            Globals.textureDictionary.Add("scared", Content.Load<Texture2D>("moods\\scared"));
            Globals.textureDictionary.Add("keylock", Content.Load<Texture2D>("keylock"));
            Globals.textureDictionary.Add("coolLevel", Content.Load<Texture2D>("moods\\level_cool"));
            Globals.textureDictionary.Add("happyLevel", Content.Load<Texture2D>("moods\\level_happy"));
            Globals.textureDictionary.Add("confusedLevel", Content.Load<Texture2D>("moods\\level_confused"));
            Globals.textureDictionary.Add("shockedLevel", Content.Load<Texture2D>("moods\\level_shocked"));
            Globals.textureDictionary.Add("scaredLevel", Content.Load<Texture2D>("moods\\level_scared"));
            Globals.textureDictionary.Add("keylockLevel", Content.Load<Texture2D>("keylock_small"));
            Globals.textureDictionary.Add("trophy", Content.Load<Texture2D>("menu\\trophy"));
            Globals.textureDictionary.Add("skull", Content.Load<Texture2D>("menu\\skull"));
            Globals.textureDictionary.Add("clock", Content.Load<Texture2D>("menu\\clock"));
            Globals.textureDictionary.Add("tap", Content.Load<Texture2D>("menu\\tap"));
            Globals.textureDictionary.Add("next", Content.Load<Texture2D>("menu\\next"));
            Globals.textureDictionary.Add("restart", Content.Load<Texture2D>("menu\\restart"));
            Globals.silkscreenFont = Content.Load<SpriteFont>("Silkscreen");

            //sprites
            Globals.spritesDictionary.Add("biggo_128x128", new Sprite("biggo_128x128", 128, 128, 2));
            Globals.spritesDictionary.Add("enemies_16x16", new Sprite("enemies_16x16", 16, 16));
            Globals.spritesDictionary.Add("enemies_32x32", new Sprite("enemies_32x32", 32, 32));
            Globals.spritesDictionary.Add("enemies_8x8", new Sprite("enemies_8x8", 8, 8));
            Globals.spritesDictionary.Add("king_48x48", new Sprite("king_48x48", 48, 48, 14));
            Globals.spritesDictionary.Add("player", new Sprite("player", 8, 16, 6));            
            Globals.tileset = new Tileset("tileset");
            
            ////sounds
            Globals.soundsDictionary.Add("coin", Content.Load<SoundEffect>(@"Sounds\Pickup_Coin8").CreateInstance());
            Globals.soundsDictionary.Add("jump", Content.Load<SoundEffect>(@"Sounds\Jump4").CreateInstance());
            Globals.soundsDictionary.Add("explosion", Content.Load<SoundEffect>(@"Sounds\Explosion9").CreateInstance());
            Globals.soundsDictionary.Add("randomize", Content.Load<SoundEffect>(@"Sounds\Randomize3").CreateInstance());
            Globals.soundsDictionary.Add("hit", Content.Load<SoundEffect>(@"Sounds\Hit_Hurt2").CreateInstance());
            Globals.soundsDictionary.Add("doors", Content.Load<SoundEffect>(@"Sounds\Randomize2").CreateInstance());
            Globals.soundsDictionary["doors"].IsLooped = true;            
            foreach (KeyValuePair<string, SoundEffectInstance> sfeffect in Globals.soundsDictionary)
                sfeffect.Value.Volume = 0.15f;
            Globals.soundsDictionary["hit"].Volume = 0.2f;
            Globals.soundsDictionary["coin"].Volume = 0.12f;
            Globals.soundsDictionary["doors"].Volume = 0.2f;
            Globals.soundsDictionary["doors"].Pitch = 0.5f;
            // music
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\xylophone (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Elevator Music (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\8-bit loop (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Gasoline Rainbows (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Chippy Cloud Kid (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\ChipChippy (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Sad Song 1"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Dramatic Metal Entrance (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Chaotic Filth (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Chaotic Standoff (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Ring Leader (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Rising Sun (oriental with dance beats)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\Vanguard Bouncy (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\wubby dancer (loop)"));
            Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\King Boss (loop)"));
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var modifiedGameTime = new GameTime(gameTime.TotalGameTime, new TimeSpan(0, 0, 0, 0, (int)(gameTime.ElapsedGameTime.TotalMilliseconds * Globals.SpeedModificator)));
            gameStateManager.Update(modifiedGameTime);
            //if (gameStateManager.CurrentState() == 0)
            //    this.Exit();

            base.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Globals.backgroundColor);
 
            Matrix matrix = Matrix.Identity;            
            matrix *= Matrix.CreateTranslation(new Vector3(-3, 0, 0)); // position adjusting
            matrix *= Matrix.CreateScale(scale);
            var translationShift = gameStateManager.GetHorizontalTransition();
            var shiftMatrix = matrix * Matrix.CreateTranslation(new Vector3(graphics.GraphicsDevice.Viewport.Width * translationShift.X, graphics.GraphicsDevice.Viewport.Height * translationShift.Y, 0.0f));

            spriteBatch3.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, matrix);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, shiftMatrix);//Matrix.CreateScale(scale));
            spriteBatch2.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, matrix);
         
            gameStateManager.Draw(spriteBatch, spriteBatch2, spriteBatch3);

            spriteBatch2.End();
            spriteBatch.End();
            spriteBatch3.End();

            base.Draw(gameTime);
        }


        protected override void OnDeactivated(object sender, EventArgs args)
        {
#if !WINDOWS
            Savestate.Instance.Save();
#endif
            base.OnDeactivated(sender, args);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
#if !WINDOWS
            Savestate.Instance.Save();
#endif
            base.OnExiting(sender, args);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            //Savestate.Instance.Reload();
            base.OnActivated(sender, args);
        }
    }
}

