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
        private class TextLine
        {
            public string text = "";
            public Texture2D avatar = null;

            public TextLine(string text = "", Texture2D avatar = null)
            {
                this.text = text;
                this.avatar = avatar;
            }
        }

        ContentManager content;
        GraphicsDeviceManager graphics;

#if WINDOWS
        MouseState prevMouseState;
        MouseState currMouseState;        
        KeyboardState currKeyboardState;
        KeyboardState previousKeyboardState;
#else
        TouchCollection touchState;
#endif
        GamePadState prevGPState;
        GamePadState currGPState;

        int currentText = 0;
        Hud hud;
        private List<TextLine> textLines = new List<TextLine>();

        private bool texted = false;
        private int drawLetterCount = 0;
        private TimeSpan letterTime = TimeSpan.Zero;

        public TextState(GraphicsDeviceManager graphics, ContentManager content, Hud hud)
        {
            this.graphics = graphics;
            this.content = content;
            this.hud = hud;

#if WINDOWS
            prevMouseState = currMouseState = Mouse.GetState();
#else
            touchState = TouchPanel.GetState();
#endif
        }

        public override void Enter(int previousStateId)
        {
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

            currGPState = GamePad.GetState(PlayerIndex.One);
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
            {
                Globals.gameStateManager.PushState(Config.States.PAUSE);
            }
            prevGPState = currGPState;

#if !WINDOWS
            touchState = TouchPanel.GetState();
            foreach(TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    if (!texted)
                    {
                        drawLetterCount = textLines[currentText].text.Length;
                        texted = true;
                    }
                    else
                    {
                        currentText++;
                        texted = false;
                        drawLetterCount = 0;
                    }
                    break;
                }
            }
#else
            currMouseState = Mouse.GetState();
            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                if (!texted)
                {
                    drawLetterCount = textLines[currentText].text.Length;
                    texted = true;
                }
                else
                {
                    currentText++;
                    texted = false;
                    drawLetterCount = 0;
                }
            }
            prevMouseState = currMouseState;            
#endif
            if (currentText >= textLines.Count)
            {
                Globals.gameStateManager.PopState();
                return;
            }

            if (!texted)
            {
                letterTime += gameTime.ElapsedGameTime;
                if (letterTime.TotalMilliseconds > Config.Hud.TEXTSTATE_LETTERTIME_MS)
                {
                    letterTime = TimeSpan.Zero;
                    drawLetterCount++;
                    if (drawLetterCount >= textLines[currentText].text.Length)
                        texted = true;
                }
            }            
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (!upsidedownBatch && Globals.upsideDown)
                return;

            int marginX = Config.Hud.AVATAR_POSITION_X;

            if (textLines[currentText].avatar != null)
            {
                spriteBatch.Draw(textLines[currentText].avatar, new Vector2(Config.Hud.AVATAR_POSITION_X, Config.Hud.AVATAR_POSITION_Y), Color.White);
                marginX += textLines[currentText].avatar.Width;
            }
            spriteBatch.DrawString(Globals.silkscreenFont, textLines[currentText].text.Substring(0, drawLetterCount), new Vector2(Config.Hud.TEXTSTATE_POSITION_X + marginX, Config.Hud.TEXTSTATE_POSITION_Y), Color.White);
        }

        private void AddTextLine(string text, Texture2D avatar = null)
        {
            if (text == "")
                return;

            textLines.Add(new TextLine(text, avatar));
        }

        public void LoadTextLines(string directory, string filescript)
        {
            string line;

            try
            {
                using (StreamReader streamReader = new StreamReader(TitleContainer.OpenStream(@"Levels\" + directory + "\\" + filescript + ".txt")))
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        string[] lineAvatarPair = line.Split('|');

                        if (lineAvatarPair.Length <= 1)
                            AddTextLine(line);
                        else
                        {
                            Texture2D avatar = null;
                            if (lineAvatarPair[0] != "")
                            {
                                avatar = content.Load<Texture2D>(directory + "\\" + lineAvatarPair[0]);
                            }
                            AddTextLine(lineAvatarPair[1], avatar);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
