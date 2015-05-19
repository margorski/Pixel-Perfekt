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
using GameStateMachine;

namespace PixelPerfect
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteBatch specialBatch;
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
            var menuState = new MenuState(graphics, Content, gameStateManager);            
            var pauseState = new PauseState(Content, gameStateManager); 
            menuState.scale = pauseState.scale = scale;


            //var cstate = new Cutscene.CutsceneState();
            //Cutscene.Scene scene = new Cutscene.Scene();            
            //scene.duration = (new TimeSpan(0, 0, 28)).ToString();
            //scene.backgroundColor = Color.Black;

            //var textItem = new Cutscene.Text();
            //textItem.text = "HELLO\nMY NAME IS JOHN PERFECT.\nI AM\r\r\r\rI WAS ARCADE GAME STAR.\n\nOH THOSE WERE THE DAYS...";
            //var textkeyframestart = new Cutscene.Keyframe();
            //var textkeyframeend = new Cutscene.Keyframe();
            //var textkeyframeStill = new Cutscene.Keyframe();
            //textkeyframestart.position = textkeyframeend.position = textkeyframeStill.position = new Vector2(100, 30);
            //textkeyframeend.printedLetters = textItem.text.Length;
            //textkeyframeend.time = (new TimeSpan(0, 0, 15)).ToString();
            //textkeyframeStill.printedLetters = textItem.text.Length;
            //textkeyframeStill.time = (new TimeSpan(0, 0, 18)).ToString();
            //textItem.keyframeList.Add(textkeyframestart);
            //textItem.keyframeList.Add(textkeyframeend);
            //scene.texts.Add(textItem);

            //var item = new Cutscene.Image();
            //item.textureFile = "cutscenes\\arcade-inside_final";
            //var keyframeStart = new Cutscene.Keyframe();
            //var keyframeEnd = new Cutscene.Keyframe();
            //keyframeStart.time = (new TimeSpan(0, 0, 18)).ToString();
            //keyframeEnd.time = (new TimeSpan(0, 0, 23)).ToString();            
            //item.keyframeList.Add(keyframeStart);
            //item.keyframeList.Add(keyframeEnd);
            //scene.images.Add(item);

            //var item2 = new Cutscene.Image();
            //item2.textureFile = "cutscenes\\arcade-cabinet_final2";
            //var keyframeStart2 = new Cutscene.Keyframe();
            //var keyframeEnd2 = new Cutscene.Keyframe();
            //keyframeStart2.time = (new TimeSpan(0, 0, 23)).ToString();
            //keyframeEnd2.time = (new TimeSpan(0, 0, 28)).ToString();
            //item2.keyframeList.Add(keyframeStart2);
            //item2.keyframeList.Add(keyframeEnd2);
            //scene.images.Add(item2);

            //cstate.scenes.Add(scene);
            
            //gameStateManager.RegisterState(Config.States.CUTSCENE, cstate);
            gameStateManager.RegisterState(Config.States.MENU, menuState);
            gameStateManager.RegisterState(Config.States.PAUSE, pauseState);
            gameStateManager.ChangeState(Config.States.MENU);
            
            //gameStateManager.PushState(Config.States.CUTSCENE);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            specialBatch = new SpriteBatch(GraphicsDevice);
            
            Globals.pixelTexture = Content.Load<Texture2D>("pixel");
            Globals.silkscreenFont = Content.Load<SpriteFont>("Silkscreen");

            //sprites
            Globals.spritesDictionary.Add("biggo_128x128", new Sprite("biggo_128x128", 128, 128, 1));
            Globals.spritesDictionary.Add("enemies_16x16", new Sprite("enemies_16x16", 16, 16));
            Globals.spritesDictionary.Add("enemies_32x32", new Sprite("enemies_32x32", 32, 32));
            Globals.spritesDictionary.Add("enemies_8x8", new Sprite("enemies_8x8", 8, 8));
            Globals.spritesDictionary.Add("king_48x48", new Sprite("king_48x48", 48, 48, 14));
            Globals.spritesDictionary.Add("player", new Sprite("player", 8, 16));
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
            //Globals.backgroundMusicList.Add(Content.Load<Song>(@"music\cheesy bassoon (loop)"));
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
            if (gameStateManager.CurrentState() == 0)
                this.Exit();

            base.Update(gameTime);
        }



        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Globals.backgroundColor);
            //spriteBatch.Begin();

            Matrix matrix = Matrix.Identity;            
            if (Globals.upsideDown)
            {
                matrix *= Matrix.CreateScale(-1);
                matrix *= Matrix.CreateTranslation(new Vector3(Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED - Config.Hud.HUD_HEIGHT, 0));
            }
            else
            {
                matrix *= Matrix.CreateTranslation(new Vector3(-3, 0, 0));
            }
            matrix *= Matrix.CreateScale(scale);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, matrix);//Matrix.CreateScale(scale));
            if (Globals.upsideDown)
                specialBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scale));

            gameStateManager.Draw(spriteBatch);
            if (Globals.upsideDown)
                gameStateManager.Draw(specialBatch, true);
            
            spriteBatch.End();
            if (Globals.upsideDown)
                specialBatch.End();

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
