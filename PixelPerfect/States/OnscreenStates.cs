﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using GameStateMachine;

namespace PixelPerfect
{
    class PauseState : GameState
    {
        ContentManager content;
        GameStateManager gameStateManager;

#if WINDOWS
        MouseState prevMouseState;
        MouseState currMouseState;
        KeyboardState prevKeyboardState;
        KeyboardState currKeyboardState;
#else
        TouchCollection touchState;
#endif
        GamePadState prevGPState;
        GamePadState currGPState;

        public PauseState(ContentManager content, GameStateManager gameStateManager) 
        { 
            this.content = content;
            this.gameStateManager = gameStateManager;
        }

        private SpriteFont menuFont;

        public override void Enter(int previousStateId)
        {
            menuFont = content.Load<SpriteFont>("Silkscreen");
            prevGPState = currGPState = GamePad.GetState(PlayerIndex.One);
#if !WINDOWS
            touchState = TouchPanel.GetState();
#else
            prevMouseState = currMouseState = Mouse.GetState();
            currKeyboardState = prevKeyboardState = Keyboard.GetState();
#endif
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
            currGPState = GamePad.GetState(PlayerIndex.One);
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
            {
                gameStateManager.PopState();
                gameStateManager.ChangeState(Config.States.LEVELSELECT);
            }
            prevGPState = currGPState;

#if !WINDOWS
            touchState = TouchPanel.GetState();
            foreach (TouchLocation touch in touchState)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    gameStateManager.PopState();
                    break;
                }
            }

#else
            currMouseState = Mouse.GetState();

            if (currMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
                gameStateManager.PopState();

            prevMouseState = currMouseState;

            currKeyboardState = Keyboard.GetState();
            if (currKeyboardState.IsKeyDown(Keys.Escape)  && prevKeyboardState.IsKeyUp(Keys.Escape))
            {
                gameStateManager.PopState();
                gameStateManager.ChangeState(Config.States.LEVELSELECT);
            }  
            prevKeyboardState = currKeyboardState;
#endif
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            if (suspended)
                spriteBatch.DrawString(menuFont, "PAUSE", new Vector2(120, 60), Color.White);
            else
                spriteBatch.DrawString(menuFont, "PAUSE", new Vector2(120, 60), Color.Red);
        }
    }

    class TextureBackgroundState : GameState
    {
        private Texture2D background = Util.GetGradientTexture(Config.SCREEN_WIDTH_SCALED + 2, Config.SCREEN_HEIGHT_SCALED, Globals.colorList[23], Globals.colorList[135], Util.GradientType.Horizontal);
        
        public TextureBackgroundState( )
        { }

        public override void Enter(int previousStateId)
        {
        }

        public override void Exit(int nextStateId)
        {
        }

        public override void Suspend(int pushedStateId)
        {
        }

        public override void Resume(int poppedStateId)
        {
            background = Util.GetGradientTexture(Config.SCREEN_WIDTH_SCALED + 2, Config.SCREEN_HEIGHT_SCALED, Globals.colorList[23], Globals.colorList[135], Util.GradientType.Horizontal);
        }

        public override void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            spriteBatch.Draw(background, Vector2.Zero, Color.White);
        }

        public override void Update(GameTime gameTime, bool suspended)
        {
        }
    }

}