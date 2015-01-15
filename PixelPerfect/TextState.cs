using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using GameStateMachine;

namespace PixelPerfect
{
    class TextState : GameState
    {
        ContentManager content;
        GraphicsDeviceManager graphics;
        GameStateManager gameStateManager;
        SpriteFont menuFont;

        MouseState prevMouseState;
        MouseState currMouseState;
        GamePadState prevGPState;
        GamePadState currGPState;

        int currentText = 0;
        Hud hud;
        private List<String> textStrings = new List<string>();

        private bool texted = false;
        private int drawLetterCount = 0;
        private TimeSpan letterTime = TimeSpan.Zero;

        public TextState(GraphicsDeviceManager graphics, ContentManager content, GameStateManager gameStateManager, Hud hud)
        {
            this.gameStateManager = gameStateManager;
            this.graphics = graphics;
            this.content = content;
            this.hud = hud;
        }

        public override void Enter(int previousStateId)
        {
            menuFont = content.Load<SpriteFont>("Silkscreen");
            currentText = 0;
            hud.enabled = false;
        }

        public override void Exit(int nextStateId)
        {
            hud.enabled = true;
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

            if (currentText >= textStrings.Count)
                gameStateManager.PopState();

            currGPState = GamePad.GetState(PlayerIndex.One);
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
            {
                gameStateManager.PushState(Config.States.PAUSE);
            }
            prevGPState = currGPState;

            currMouseState = Mouse.GetState();
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (!texted)
                {
                    drawLetterCount = textStrings[currentText].Length;
                    texted = true;
                }
                else
                {
                    currentText++;
                    texted = false;
                    drawLetterCount = 0;
                }
            }

            if (!texted)
            {
                letterTime += gameTime.ElapsedGameTime;
                if (letterTime.TotalMilliseconds > Config.Hud.TEXTSTATE_LETTERTIME_MS)
                {
                    letterTime = TimeSpan.Zero;
                    drawLetterCount++;
                    if (drawLetterCount >= textStrings[currentText].Length)
                        texted = true;
                }
            }
            prevMouseState = currMouseState;
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended)
        {
            spriteBatch.DrawString(menuFont, textStrings[currentText].Substring(0, drawLetterCount), new Vector2(Config.Hud.TEXTSTATE_POSITION_X, Config.Hud.TEXTSTATE_POSITION_Y), Color.White);
        }

        private void AddString(string text)
        {
            if (text == "")
                return;

            textStrings.Add(text);
        }

        public void LoadStrings(string filescript)
        {
            string line;

            try
            {
                using (StreamReader streamReader = new StreamReader(TitleContainer.OpenStream(@"Levels\" + filescript + ".txt")))
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        AddString(line);
                    }
                }
            }
            catch { }
        }
    }
}
