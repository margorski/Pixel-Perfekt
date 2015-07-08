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
    class LevelSelectState : GameState
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
        List<Button> levelButtons = new List<Button>();

        Button backButton;
        Button suitButton;
        Animation pikpokAnimation = new Animation(4, Config.DEFAULT_ANIMATION_SPEED, false);
        public LevelSelectState(GameStateManager gameStateManager) 
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

            var worldName = Globals.worlds[Globals.selectedWorld].name;
            var titlex = Config.SCREEN_WIDTH_SCALED / 2.0f - Globals.silkscreenFont.MeasureString(worldName).X * 2.0f / 2.0f;
            caption = new WavyText(worldName, new Vector2(titlex, 7), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);
            backButton = new Button("", new Rectangle(Config.Menu.BACK_X, Config.Menu.BACK_Y, 24, 24), Globals.textureDictionary["back"], Globals.silkscreenFont, false);
            suitButton = new Button("", new Rectangle(Config.Menu.BUTTONS_X + 2 * (24 + Config.Menu.BUTTONS_SPACE), Config.Menu.BACK_Y, 24, 24), Globals.textureDictionary["suit"], Globals.silkscreenFont, false);
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

            pikpokAnimation.Update(gameTime);
#if WINDOWS
            currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
#endif
            Update_HandleBack();          
            Update_LevelSelect();
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
            levelButtons.Clear();
            var worldName = Globals.worlds[Globals.selectedWorld].name;
            int levelCount = 0;
            foreach (Level level in Globals.worlds[Globals.selectedWorld].levels)
            {
                Rectangle rectangle = new Rectangle(Config.Menu.LEVEL_OFFSET_X + (levelCount % 5) * (Config.Menu.LEVEL_HORIZONTAL_SPACE + Globals.textureDictionary[worldName + "Level"].Width),
                                                    Config.Menu.LEVEL_OFFSET_Y + (levelCount / 5) * (Config.Menu.LEVEL_VERTICAL_SPACE + Globals.textureDictionary[worldName + "Level"].Height),
                                                    Globals.textureDictionary[worldName + "Level"].Width,
                                                    Globals.textureDictionary[worldName + "Level"].Height);
                levelButtons.Add(new Button("", rectangle, Globals.textureDictionary[worldName + "Level"], Globals.silkscreenFont, false));
                if (!Globals.worlds[Globals.selectedWorld].LevelActivated(levelCount))
                    levelButtons[levelCount].active = false;
                levelCount++;
            }
        }

        private void GoBack()
        {
            gameStateManager.ChangeState(Config.States.WORLDSELECT, true, Config.Menu.TRANSITION_DELAY);      
        }

        public void Update_LevelSelect()
        {            
#if !WINDOWS
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                {
                    foreach (Button button in levelButtons)
                    {
                        button.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                    }
                    backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                    suitButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                }
                else if (touch.State == TouchLocationState.Released)
                {
                    foreach (Button button in levelButtons)
                    {
                        if (button.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                            SelectLevel(levelButtons.IndexOf(button));
                    }
                    if (backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                        GoBack();
                    else if (suitButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                        gameStateManager.ChangeState(Config.States.SUITSELECT, true, Config.Menu.TRANSITION_DELAY);
                }
            }
#else
            if (currMouseState.LeftButton == ButtonState.Pressed)
            {
                foreach (Button button in levelButtons)
                {
                    button.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                }
                backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                suitButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
            }
            else if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                foreach (Button button in levelButtons)
                {
                    if (button.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale,true))
                        SelectLevel(levelButtons.IndexOf(button));
                }
                if (backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                    GoBack();
                else if (suitButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                    gameStateManager.ChangeState(Config.States.SUITSELECT, true, Config.Menu.TRANSITION_DELAY);
            }
#endif
        }



        public void SelectLevel(int level)
        {
            if (level >= Globals.worlds[Globals.selectedWorld].levels.Count)
                return;

            Globals.selectedLevel = level;
            gameStateManager.ChangeState(Config.States.LEVELDETAILS, true, Config.Menu.TRANSITION_DELAY);
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (caption != null)
                caption.Draw(spriteBatch);
            backButton.Draw(spriteBatch);
            suitButton.Draw(spriteBatch);
            Color color = Color.White;
            int levelCount = 0;

            foreach (Button button in levelButtons)
            {
                color = Globals.worlds[Globals.selectedWorld].BeatLevelPerfektTime(levelCount) ? Color.Gold :
                        Globals.worlds[Globals.selectedWorld].LevelCompleted(levelCount) ? Color.Green :
                        Globals.worlds[Globals.selectedWorld].LevelSkipped(levelCount) ? Color.MediumVioletRed : Color.White;
                if (color == Color.White)
                    button.Draw(spriteBatch);
                else
                {
                    button.Draw(spriteBatch, color);
                    if (Globals.worlds[Globals.selectedWorld].LevelSkipped(levelCount))
                        spriteBatch.Draw(Globals.spritesDictionary["enemies_16x16"].texture, new Vector2(button.rectangle.X, button.rectangle.Y), 
                                         new Rectangle(0, pikpokAnimation.GetCurrentFrame() * 16, 16, 16), Color.White);
                    if (Globals.worlds[Globals.selectedWorld].BeatLevelPerfektTime(levelCount))
                        spriteBatch.Draw(Globals.textureDictionary["trophy"], new Vector2(button.rectangle.X, button.rectangle.Y), Color.White);
                }
                spriteBatch.DrawString(Globals.silkscreenFont, (levelCount + 1).ToString(),
                                    new Vector2(button.rectangle.Right - 10, button.rectangle.Bottom - 10), color
                                    , 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.0f);
                levelCount++;
                
            }

            String text = "SKIPS";
            Vector2 textPosition = new Vector2(Config.SCREEN_WIDTH_SCALED/2, Config.Menu.SKIPTEXT_Y);
            textPosition -= Globals.silkscreenFont.MeasureString(text) / 2;
            spriteBatch.DrawString(Globals.silkscreenFont, text, textPosition, Color.White);
            for (int i = 0; i < Config.SKIP_AMOUNT; i++)
            {
                bool skipped = (i >= Savestate.Instance.SkipLeft());
                if (skipped)
                    break;
                Vector2 position = new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 8 - 26 + 26 * i, Config.Menu.SKIPTEXT_Y + 4);
                spriteBatch.Draw(Globals.spritesDictionary["enemies_16x16"].texture, position, new Rectangle(0, 
                    (!skipped ? pikpokAnimation.GetCurrentFrame() * 16 : 16), 16, 16), 
                    (!skipped ? Color.White : Color.DimGray));
            }
        }
    }
}
