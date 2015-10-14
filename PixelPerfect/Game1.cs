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
#if ANDROID
using IsolatedStorageSettings = CustomIsolatedStorageSettings.IsolatedStorageSettings;
#endif
#if WINDOWS_PHONE
using Windows.ApplicationModel.Store;
#endif

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
      
        //private float scale = 1.0f;
        private Vector2 scale = new Vector2(1.0f, 1.0f);
        private Vector3 translation = new Vector3(0.0f, 0.0f, 0.0f);
        private float translatex = 0.0f;
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
            Content.RootDirectory = "Content";
#if WINDOWS || WINDOWS_PHONE
            ScaleWP();
#endif
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(111111);
            Globals.game = this;

            TouchPanel.EnabledGestures = GestureType.None;
        }


        private void ScaleScreen()
        {
            translation.X = -3.0f;
#if !(WINDOWS_PHONE || WINDOWS)
            var aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            if (aspectRatio == 1.66666663)
                scale = new Vector2(Config.SCALE_FACTOR, Config.SCALE_FACTOR);
            else
            {
                var scaley = graphics.GraphicsDevice.Viewport.Height / (float)Config.SCREEN_HEIGHT_SCALED;
                var scalex = graphics.GraphicsDevice.Viewport.Width / ((float)Config.SCREEN_WIDTH_SCALED - 6);
                scale = new Vector2(scalex, 
                                   scalex); //Math.Min(scaley, scalex);
            
                translation.X = (graphics.GraphicsDevice.Viewport.Width / scale.X - Config.SCREEN_WIDTH_SCALED) / 2.0f;
                translation.Y = (graphics.GraphicsDevice.Viewport.Height / scale.Y - Config.SCREEN_HEIGHT_SCALED) / 2.0f;
            }           
#endif
        }

#if WINDOWS_PHONE
        private void ScaleWP()
        {
            var content = App.Current.Host.Content;
            // autoscaling part from web
            int? scaleFactor = null;
            var scaleFactorProperty = content.GetType().GetProperty("ScaleFactor");

            if (scaleFactorProperty != null)
                scaleFactor = scaleFactorProperty.GetValue(content, null) as int?;

            if (scaleFactor == null)
                scaleFactor = 100; // 100% WVGA resolution

            scale.X = scale.Y = (scaleFactor.Value / 100.0f);
            scale *= Config.SCALE_FACTOR;
        }
#endif

#if ANDROID
        //private void ScaleAndroid()
        //{

        //}
#endif

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();

            ScaleScreen();
            Globals.renderTarget = new RenderTarget2D(GraphicsDevice, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED,
                                                      false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
            Savestate.Init();
            
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
            
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("suit"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("suit", 0);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            Globals.suit = (int)IsolatedStorageSettings.ApplicationSettings["suit"];

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("completed"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("completed", false);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            Globals.completed = (bool)IsolatedStorageSettings.ApplicationSettings["completed"];

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("firstcutscene"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("firstcutscene", false);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            Globals.firstcutscene = (bool)IsolatedStorageSettings.ApplicationSettings["firstcutscene"];

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("swappedcontrols"))
            {
                IsolatedStorageSettings.ApplicationSettings.Add("swappedcontrols", false);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            Globals.swappedControls = (bool)IsolatedStorageSettings.ApplicationSettings["swappedcontrols"];

#if WINDOWS_PHONE
            Globals.noads = CurrentApp.LicenseInformation.ProductLicenses["noads"].IsActive;
#endif

            gameStateManager = new GameStateManager();
            Globals.gameStateManager = gameStateManager;
            Globals.worlds = World.LoadWorlds();

            var splashScreenState = new SplashScreenState();
            splashScreenState.scale = scale;

            gameStateManager.RegisterState(Config.States.SPLASHSCREEN, splashScreenState);
            gameStateManager.PushState(Config.States.SPLASHSCREEN);            
        }

        protected override void LoadContent()
        {
            Globals.spriteBatch = spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch2 = new SpriteBatch(GraphicsDevice);
            spriteBatch3 = new SpriteBatch(GraphicsDevice);
            MediaPlayer.IsRepeating = true;
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

            GraphicsDevice.Clear(Color.Black);
 
            Matrix matrix = Matrix.Identity;            
            matrix *= Matrix.CreateTranslation(translation); // position adjusting
            matrix *= Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1.0f));
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
            if (Savestate.Instance != null)
                Savestate.Instance.Save();
#endif
            if (gameStateManager != null && gameStateManager.IsStateOnTop(Config.States.LEVEL))
                Globals.gameStateManager.PushState(Config.States.PAUSE); 
            base.OnDeactivated(sender, args);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
#if !WINDOWS
            if (Savestate.Instance != null)
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

