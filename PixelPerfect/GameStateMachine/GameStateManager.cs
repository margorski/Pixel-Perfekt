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

namespace GameStateMachine
{
    public class GameStateManager
    {
        private enum State
        {
            Idle,
            Exiting,
            Entering
        }

        public const int TRANSITION_TIME = 700; // ms

        private Dictionary<int, GameState> stateStack = new Dictionary<int, GameState>();
        private Dictionary<int, GameState> registeredStates = new Dictionary<int, GameState>();
        private State state = State.Idle;
        private TimeSpan transitionTime = TimeSpan.Zero;
        private int nextStateId = -1;        
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
        public bool ChangeState(int stateId, bool transition = false)
        {
            int previousStateId = 0;

            if (!registeredStates.ContainsKey(stateId))
                return false;

            if (stateStack.ContainsKey(stateId))
                return false;

            if (stateStack.Count > 0)
            {
                stateStack.ElementAt(stateStack.Count - 1).Value.Exit(stateId);

                if (transition)
                {
                    state = State.Exiting;
                    nextStateId = stateId;
                    return true;
                }

                previousStateId = stateStack.ElementAt(stateStack.Count - 1).Key;
                stateStack.Remove(previousStateId);
            }
            stateStack.Add(stateId, registeredStates[stateId]);
            stateStack.ElementAt(stateStack.Count - 1).Value.Enter(previousStateId);

            return true;
        }

        public bool PopState()
        {
            if (stateStack.Count == 0)
                return false;

            int previousStateId = stateStack.ElementAt(stateStack.Count - 1).Key;

            stateStack[previousStateId].Exit(-1);
            stateStack.Remove(previousStateId);
            if (stateStack.Count > 0)
                stateStack.ElementAt(stateStack.Count - 1).Value.Resume(previousStateId);

            return true;
        }
        public bool PushState(int stateId)
        {
            if (!registeredStates.ContainsKey(stateId))
                return false;

            if (stateStack.ContainsKey(stateId))
                return false;

            if (stateStack.Count > 0)
                stateStack.ElementAt(stateStack.Count - 1).Value.Suspend(stateId);

            stateStack.Add(stateId, registeredStates[stateId]);
            stateStack[stateId].Enter(-1);

            return true;
        }

        public GameState GetState(int stateId)
        {
            if (!registeredStates.ContainsKey(stateId))
                return null;

            return stateStack[stateId];
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
        public void Update(GameTime gameTime)
        {
            switch (state)
            {
                case State.Exiting:
                    transitionTime += gameTime.ElapsedGameTime;
                    if (transitionTime.TotalMilliseconds > TRANSITION_TIME)
                    {
                        transitionTime = TimeSpan.Zero;
                        state = State.Entering;

                        int previousStateId = -1;
                        previousStateId = stateStack.ElementAt(stateStack.Count - 1).Key;
                        stateStack.Remove(previousStateId);
                        stateStack.Add(nextStateId, registeredStates[nextStateId]);
                        stateStack.ElementAt(stateStack.Count - 1).Value.Enter(previousStateId);
                        nextStateId = -1;
                    }
                    break;

                case State.Entering:
                    transitionTime += gameTime.ElapsedGameTime;
                    if (transitionTime.TotalMilliseconds > TRANSITION_TIME)
                    {
                        transitionTime = TimeSpan.Zero;
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
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < stateStack.Count; i++)
            {
                if (i == stateStack.Count - 1)
                    stateStack.ElementAt(i).Value.Draw(spriteBatch, false);
                else
                    stateStack.ElementAt(i).Value.Draw(spriteBatch, true);
            }
        }

        public float GetTranslationShift()
        {
            if (state == State.Exiting)
                return -(float)(transitionTime.TotalMilliseconds / TRANSITION_TIME);
            else if (state == State.Entering)
                return (float)(1 - transitionTime.TotalMilliseconds / TRANSITION_TIME);
            else
                return 0.0f;
        }
    }
}
