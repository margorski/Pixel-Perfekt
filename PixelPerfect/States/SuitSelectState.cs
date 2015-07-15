using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

#if !WINDOWS
using Microsoft.Phone.Tasks;
#endif

namespace PixelPerfect
{
    class SuitSelectState : GameState
    {
        GameStateManager gameStateManager;

#if !WINDOWS
        TouchCollection touchState;
#else
        MouseState prevMouseState;
        MouseState currMouseState;
        KeyboardState prevKeyboardState;
        KeyboardState currKeyboardState;
#endif
        GamePadState prevGPState;
        GamePadState currGPState;

        WavyText caption;
        List<Button> suitButtons = new List<Button>();

        Button backButton;
        Animation playerAnimation = new Animation(Config.Player.ANIM_FRAMES, Config.Player.ANIMATION_DELAY, true);
        TimeSpan stoppedTimer = TimeSpan.Zero;
        bool stopped = false;

        private string[] names = {"NORMAN", "COMMIE", "EIFFEL", "MESA", "NEON", "BLING",
                                  "PANTY", "SMOKIE", "BASTARD", "KENT", "PLUMBER", "BANER",
                                  "MUTANT", "BOY", "RCOP", "GRYPHON", "IRON", "PIKPOK"};

        public SuitSelectState(GameStateManager gameStateManager) 
        {
            this.gameStateManager = gameStateManager;
        }

        public override void Enter(int previousStateId)
        {
#if !WINDOWS
            touchState = TouchPanel.GetState();
#else
            prevMouseState = currMouseState = Mouse.GetState();
            prevKeyboardState = currKeyboardState = Keyboard.GetState();
#endif
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);

            string captionString = "SUIT SELECT";
            var titlex = Config.SCREEN_WIDTH_SCALED / 2.0f - Globals.silkscreenFont.MeasureString(captionString).X * 2.0f / 2.0f;
            caption = new WavyText(captionString, new Vector2(titlex, 4), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);
            backButton = new Button("", new Rectangle(Config.Menu.BACK_X, Config.Menu.BACK_Y + 1, 24, 24), Globals.textureDictionary["back"], Globals.silkscreenFont, false);

            Globals.selectedLevel = -1;            
            
            if (Globals.musicEnabled && MediaPlayer.State != MediaState.Playing)
                MediaPlayer.Play(Globals.backgroundMusicList[Theme.CurrentTheme.music]);

            PrepareButtons();
        }

        public override void Exit(int nextStateId)
        {
        }

        public override void Resume(int poppedStateId)
        {
        }

        public override void Suspend(int pushedStateId)
        {            
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            if (caption != null)
                caption.Update(gameTime);

            if (!stopped)
                playerAnimation.Update(gameTime);

            stoppedTimer += gameTime.ElapsedGameTime;
            if (stoppedTimer.TotalMilliseconds > 3 * Config.Player.ANIMATION_DELAY * (Config.Player.ANIM_FRAMES - 1))
            {
                stoppedTimer = TimeSpan.Zero;
                stopped = !stopped;
            }
#if WINDOWS
            currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
#endif
            Update_HandleBack();          
            Update_SuitSelect();
#if WINDOWS
            prevMouseState = currMouseState;
#endif       
        }

        public void Update_HandleBack()
        {
            currGPState = GamePad.GetState(PlayerIndex.One);

            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released) 
                GoBack();

            prevGPState = currGPState;
#if WINDOWS
            currKeyboardState = Keyboard.GetState();

            if (currKeyboardState.IsKeyDown(Keys.Escape) && prevKeyboardState.IsKeyUp(Keys.Escape))
                GoBack();
 
            prevKeyboardState = currKeyboardState;
#endif
        }

        private void PrepareButtons()
        {
            suitButtons.Clear();

            for (int i = 0; i < Config.Player.SUIT_QTY; i++)
            {
                var buttonTexture = (Savestate.Instance.suitUnlocked[i] ? Globals.textureDictionary["suitbutton"] : Globals.textureDictionary["suitbuttonlocked"]);
                suitButtons.Add(new Button("", new Rectangle(Config.Menu.SUIT_OFFSET_X + (i % 6) * (buttonTexture.Width + Config.Menu.SUIT_HORIZONTAL_SPACE),
                                                              Config.Menu.SUIT_OFFSET_Y + (i / 6) * (buttonTexture.Height + Config.Menu.SUIT_VERTICAL_SPACE),
                                                              buttonTexture.Height,
                                                              buttonTexture.Width),
                                                              buttonTexture,
                                                              Globals.silkscreenFont, false));
                if (!Savestate.Instance.suitUnlocked[i])
                    suitButtons[i].active = false;                
            }
        }

        private void GoBack()
        {
            gameStateManager.ChangeState(Config.States.LEVELSELECT, true, Config.Menu.TRANSITION_DELAY);      
        }

        public void Update_SuitSelect()
        {            
#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                {
                    foreach (Button button in suitButtons)
                    {
                        button.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                    }
                    backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                }
                else if (touch.State == TouchLocationState.Released)
                {
                    foreach (Button button in suitButtons)
                    {
                        if (button.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                            Globals.suit = suitButtons.IndexOf(button);
                            IsolatedStorageSettings.ApplicationSettings["suit"] = Globals.suit;
                            IsolatedStorageSettings.ApplicationSettings.Save();
                    }
                    if (backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                        GoBack();
                }
            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed)
            {
                foreach (Button button in suitButtons)
                {
                    button.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                }
                backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
            }
            else if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                foreach (Button button in suitButtons)
                {
                    if (button.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))                    
                        Globals.suit = suitButtons.IndexOf(button);
                }
                if (backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                    GoBack();
            }
#endif
        }


        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (caption != null)
                caption.Draw(spriteBatch);
            backButton.Draw(spriteBatch);

            int currentButton = 0;

            foreach (Button suitButton in suitButtons)
            {
                if (currentButton == Globals.suit)
                    suitButton.Draw(spriteBatch, Color.Gold);
                else
                    suitButton.Draw(spriteBatch);
                if (Savestate.Instance.suitUnlocked[currentButton])
                {
                    spriteBatch.Draw(Globals.spritesDictionary["player"].texture, new Vector2(suitButton.rectangle.X + 8, suitButton.rectangle.Y + 4),
                                 new Rectangle((currentButton * 5 + World.LastActiveWorld()) * Config.Player.WIDTH,
                                                (stopped ? 0 : (Config.Player.HEIGHT) * (playerAnimation.currentFrame + 1)),
                                                Config.Player.WIDTH, Config.Player.HEIGHT), Color.White);
                    var textPosition = new Vector2(suitButton.rectangle.X + suitButton.rectangle.Width / 2,
                                                   suitButton.rectangle.Y + suitButton.rectangle.Height);
                    textPosition.X -= Globals.silkscreenFont.MeasureString(names[currentButton]).X / 2;
                    spriteBatch.DrawString(Globals.silkscreenFont, names[currentButton], textPosition, (Globals.suit == currentButton ? Color.Gold : Color.White));
                }
                currentButton++;
            }

            spriteBatch.DrawString(Globals.silkscreenFont, "UNLOCK SUIT EVERY 3x", new Vector2(77, 137), Color.White, 0.0f, Vector2.Zero, 1.4f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.textureDictionary["trophy"], new Vector2(240, 134), Color.Gold);            
        }
    }
}
