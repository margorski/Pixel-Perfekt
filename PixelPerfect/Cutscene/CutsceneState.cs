using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect.Cutscene
{
    class CutsceneState : GameState
    {
        public List<Scene> scenes = new List<Scene>();

        private int currentScene = 0;
        private TimeSpan currentDuration = TimeSpan.Zero;

        public int backroundMusic = -1;

        GamePadState prevGPState;
        GamePadState currGPState;

        public CutsceneState() { }

        public override void Enter(int previousStateId)
        {
            Init();
            MediaPlayer.Stop();

            if (backroundMusic > -1 && Globals.musicEnabled)
            {
                MediaPlayer.Play(Globals.backgroundMusicList[backroundMusic]);
            }
        }

        public override void Exit(int nextStateId)
        {
            Init();
            MediaPlayer.Stop();
        }

        public override void Suspend(int pushedStateId)
        {         
        }

        public override void Resume(int poppedStateId)
        {
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            CurrentScene().Draw(spriteBatch);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            //currGPState = GamePad.GetState(PlayerIndex.One);
            //if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
            //    Globals.gameStateManager.PopState();
            //prevGPState = currGPState;


            currentDuration += gameTime.ElapsedGameTime;

            if (currentDuration >= CurrentScene()._duration)
            {
                currentScene++;
                currentDuration = TimeSpan.Zero;
                if (currentScene >= scenes.Count)
                {
                    Globals.gameStateManager.PopState();
                    return;
                }
            }
            CurrentScene().Update(gameTime);            
        }
        private Scene CurrentScene() { return scenes[currentScene]; }

        private void Init()
        {
            currentDuration = TimeSpan.Zero;
            currentScene = 0;
            foreach (Scene scene in scenes)
                scene.Init();
        }
    }
}
