using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class GameStateManager
    {
        private enum State
        {
            Idle,
            Delay,
            Entering
        }

        private enum Operation
        {
            None,
            Change,
            Pop,
            Push
        }

        public const int TRANSITION_TIME = 700; // ms

        private Dictionary<int, GameState> stateStack = new Dictionary<int, GameState>();
        private Dictionary<int, GameState> registeredStates = new Dictionary<int, GameState>();
        private State state = State.Idle;
        private TimeSpan transitionTime = TimeSpan.Zero;
        private TimeSpan delayTime = TimeSpan.Zero;
        private int delay = 0;
        private int nextStateId = -1;
        private int previousStateId = -1;
        private bool transition = false;

        public GameStateManager() 
        {
        }
        public bool RegisterState(int stateId, GameState gameState)
        {
            if (registeredStates.ContainsKey(stateId))
                return false;

            registeredStates.Add(stateId, gameState);

            return true;
        }
        public bool UnregisterState(int stateId)
        {
            if (!registeredStates.ContainsKey(stateId))
                return false;

            registeredStates.Remove(stateId);
            return true;
        }
        public bool IsStateOnTop(int stateId)
        {
            if (stateStack.Count > 0)
            {
                if (stateStack.ElementAt(stateStack.Count - 1).Key == stateId)
                    return true;
            }
            return false;
        }
        public bool IsStateOnStack(int stateId)
        {
            return stateStack.ContainsKey(stateId);
        }
        public int CurrentState()
        {
            if (stateStack.Count > 0)
                return stateStack.ElementAt(stateStack.Count - 1).Key;
            return 0;
        }
        public bool ChangeState(int stateId, bool transition = false, int delay = 0)
        {            
            if (!registeredStates.ContainsKey(stateId))
                return false;

            this.delay = delay;
            this.transition = transition;
            nextStateId = stateId;
        
            if (stateStack.Count > 0)
            {
                stateStack.ElementAt(stateStack.Count - 1).Value.Exit(stateId);
                previousStateId = stateStack.ElementAt(stateStack.Count - 1).Key;

                if (transition)
                    ExplodeState(stateStack[previousStateId]);
                
                stateStack.Remove(previousStateId);
            }
            state = State.Delay;
        
            return true;
        }

        public bool PopState(bool transtion = false, int delay = 0)
        {
            this.delay = delay;

            if (stateStack.Count == 0)
                return false;

            nextStateId = -1;
            previousStateId = stateStack.ElementAt(stateStack.Count - 1).Key;
            stateStack[previousStateId].Exit(-1);
            if (transtion)            
                ExplodeState(stateStack[previousStateId]);                
            
            stateStack.Remove(previousStateId);
            state = State.Delay;

            return true;
        }
        public bool PushState(int stateId, bool transition = false, int delay = 0)
        {
            this.delay = delay;
            this.transition = transition;
            nextStateId = stateId;

            if (!registeredStates.ContainsKey(stateId))
                return false;

            if (stateStack.ContainsKey(stateId))
                return false;
                        
            if (stateStack.Count > 0)
            { 
                stateStack.ElementAt(stateStack.Count - 1).Value.Suspend(stateId);                    
                previousStateId = stateStack.ElementAt(stateStack.Count - 1).Key;
            }

            if (transition)
            {
                state = State.Delay;
            }
            else
            {
                SetState();
                stateStack.ElementAt(stateStack.Count - 1).Value.Enter(previousStateId);
            }
            return true;
        }

        public GameState GetState(int stateId)
        {
            if (!registeredStates.ContainsKey(stateId))
                return null;

            return registeredStates[stateId];
        }

        public void EmptyStack()
        {
            while (!IsEmpty())
                PopState();
        }
        public bool IsEmpty() 
        { 
            return (stateStack.Count == 0); 
        }

        private void SetState()
        {
            if (nextStateId == -1)
            {
                if (stateStack.Count > 0)
                    stateStack.ElementAt(stateStack.Count - 1).Value.Resume(previousStateId);
                return;
            }

            if (stateStack.ContainsKey(nextStateId))
            {
                stateStack.Remove(nextStateId);
                stateStack.Add(nextStateId, registeredStates[nextStateId]);
                stateStack.ElementAt(stateStack.Count - 1).Value.Resume(previousStateId);
            }
            else
            {
                stateStack.Add(nextStateId, registeredStates[nextStateId]);
                stateStack.ElementAt(stateStack.Count - 1).Value.Enter(previousStateId);
            }
        }   

        private void ExplodeState(GameState gameState)
        {
            var textureArray = Util.GetTextureArray(Util.DrawToTexture(gameState.Draw));

            if (Globals.soundEnabled)
                Globals.soundsDictionary["explosion"].Play();

            List<int> addedPixels = new List<int>();

            while (Globals.pixelParticles.Count < Config.PixelParticle.MAX_PARTICLES_MENU)
            {
                int pixelNumber = Globals.rnd.Next(textureArray.Length - 1);
                
                if (addedPixels.Contains(pixelNumber))
                    continue;

                if (textureArray[pixelNumber].A == 255)
                {
                    Vector2 boomCenter = new Vector2(Config.SCREEN_WIDTH_SCALED / 2, Config.SCREEN_HEIGHT_SCALED);
                    Vector2 pixPos = new Vector2(pixelNumber % Config.SCREEN_WIDTH_SCALED, pixelNumber / Config.SCREEN_WIDTH_SCALED);
                    Vector2 pixSpeed = (pixPos - boomCenter) * (float)Globals.rnd.NextDouble() * 1.5f;
                    pixSpeed.Y *= 1.5f;
                    Vector2 acc = new Vector2(0, 0);

                    Globals.pixelParticles.Add(new PixelParticle(pixPos, 0.0, pixSpeed, acc, textureArray[pixelNumber], true, null, true, Config.StandingType.Pixel, 0));
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Globals.pixelParticles.Count; i++)
            {
                if (Globals.pixelParticles[i].Update(gameTime))
                {
                    Globals.pixelParticles.RemoveAt(i);
                    i--;
                }
            }

            switch (state)
            {
                case State.Delay:
                    delayTime += gameTime.ElapsedGameTime;
                    if (delayTime.TotalMilliseconds > delay)
                    {
                        delayTime = TimeSpan.Zero;
                        delay = 0;
                        SetState();

                        if (transition)
                            state = State.Entering;
                        else
                        {
                            state = State.Idle;
                            nextStateId = -1;
                            previousStateId = -1;
                        }
                    }
                    break;

                case State.Entering:
                    transitionTime += gameTime.ElapsedGameTime;
                    if (transitionTime.TotalMilliseconds > TRANSITION_TIME)
                    {
                        transitionTime = TimeSpan.Zero;                                                
                        previousStateId = -1;
                        nextStateId = -1;
                        transition = false;
                        state = State.Idle;
                    }
                    break;

                case State.Idle:
                    for (int i = 0; i < stateStack.Count; i++)
                    {
                        if (i == stateStack.Count - 1)
                            stateStack.ElementAt(i).Value.Update(gameTime, false);
                        else
                            stateStack.ElementAt(i).Value.Update(gameTime, true);
                    }
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteBatch spriteBatch2, SpriteBatch spriteBatch3)
        {
            for (int i = 0; i < stateStack.Count; i++)
            {
                if (i == stateStack.Count - 1)
                    stateStack.ElementAt(i).Value.Draw(spriteBatch, false);
                else
                    stateStack.ElementAt(i).Value.Draw(spriteBatch2, true);
            }

            foreach (PixelParticle pparticle in Globals.pixelParticles)
                pparticle.Draw(spriteBatch3);
        }

        public Vector2 GetHorizontalTransition()
        {
            if (state == State.Entering)
                return new Vector2((float)(1 - transitionTime.TotalMilliseconds / TRANSITION_TIME), 0.0f);
            else
                return Vector2.Zero;
        }

        public Vector2 GetVerticalTransition()
        {
            if (state == State.Entering)
                return new Vector2(0.0f, (float)(1 - transitionTime.TotalMilliseconds / TRANSITION_TIME));
            else
                return Vector2.Zero;
        }

        public GameState GetTop()
        {
            if (stateStack.Count == 0)
                return null;

            return stateStack.ElementAt(stateStack.Count - 1).Value;
        }
    }
}
