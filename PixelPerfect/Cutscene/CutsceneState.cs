using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GameStateMachine;
using System.IO;

namespace PixelPerfect.Cutscene
{
    public class CutsceneState : GameState
    {
        public List<Scene> scenes = new List<Scene>();

        private int currentScene = 0;
        private TimeSpan currentDuration = TimeSpan.Zero;

        public CutsceneState() { }

        public override void Enter(int previousStateId)
        {
            foreach (Scene scene in scenes)
                scene.Init();
        }

        public override void Exit(int nextStateId)
        {            
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
            currentDuration += gameTime.ElapsedGameTime;

            if (currentDuration >= CurrentScene()._duration)
            {
                currentScene++;
                if (currentScene >= scenes.Count)
                {
                    Globals.gameStateManager.PopState();
                    return;
                }
            }
            CurrentScene().Update(gameTime);            
        }
        private Scene CurrentScene() { return scenes[currentScene]; }
    }
}
