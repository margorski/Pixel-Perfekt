using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System.IO;
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
            gameStateManager = new GameStateManager();
            Globals.gameStateManager = gameStateManager;
            var menuState = new MenuState(graphics, Content, gameStateManager);            
            var pauseState = new PauseState(Content, gameStateManager); 
            menuState.scale = pauseState.scale = scale;

            //var cstate = Cutscene.Serializer.Deserialize("test.xml");
            var cstate = new Cutscene.CutsceneState();
            Cutscene.Scene scene = new Cutscene.Scene();
            var texttime = (new TimeSpan(0, 0, 20)).ToString();
            scene.duration = (new TimeSpan(0, 0, 30)).ToString(); ;
            var textItem = new Cutscene.Text();
            textItem.text = "HELLO\nMY NAME IS JOHN PERFECT.\nI AM... I WAS ARCADE GAME STAR.\n\nOH THOSE WERE THE DAYS...";
            var textkeyframestart = new Cutscene.Keyframe();
            var textkeyframeend = new Cutscene.Keyframe();
            textkeyframeend.printedLetters = textItem.text.Length;
            textkeyframeend.time = texttime;
            textItem.keyframeList.Add(textkeyframestart);
            textItem.keyframeList.Add(textkeyframeend);
            scene.texts.Add(textItem);
            var item = new Cutscene.Image();
            item.textureFile = "bubblebobble\\enemiesbig";
            var keyframeStart = new Cutscene.Keyframe();
            var keyframeEnd = new Cutscene.Keyframe();
            keyframeEnd.time = texttime;
            keyframeEnd.color = Color.Red;
            keyframeEnd.scale = 3.0f;
            keyframeEnd.rotation = 3.0f;
            keyframeEnd.position = new Vector2(200, 200);
            item.keyframeList.Add(keyframeStart);
            item.keyframeList.Add(keyframeEnd);
            scene.images.Add(item);
            cstate.scenes.Add(scene);
            Cutscene.Serializer.Serialize(cstate, "test2.xml");

            gameStateManager.RegisterState(Config.States.CUTSCENE, cstate);
            gameStateManager.RegisterState(Config.States.MENU, menuState);
            gameStateManager.RegisterState(Config.States.PAUSE, pauseState);
            gameStateManager.ChangeState(Config.States.MENU);            
            gameStateManager.PushState(Config.States.CUTSCENE);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            specialBatch = new SpriteBatch(GraphicsDevice);
            Globals.pixelTexture = Content.Load<Texture2D>("pixel");
            Globals.silkscreenFont = Content.Load<SpriteFont>("Silkscreen");
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
            matrix *= Matrix.CreateScale(scale);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, matrix);//Matrix.CreateScale(scale));
            if (Globals.upsideDown)
                specialBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scale));

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
            Savestate.Instance.Save();
            base.OnDeactivated(sender, args);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            Savestate.Instance.Save();
            base.OnExiting(sender, args);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            //Savestate.Instance.Reload();
            base.OnActivated(sender, args);
        }
    }
}
