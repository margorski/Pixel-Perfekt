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
        private Dictionary<int, GameState> stateStack = new Dictionary<int, GameState>();
        private Dictionary<int, GameState> registeredStates = new Dictionary<int, GameState>();

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
        public bool ChangeState(int stateId)
        {
            int previousStateId = 0;

            if (!registeredStates.ContainsKey(stateId))
                return false;

            if (stateStack.ContainsKey(stateId))
                return false;

            if (stateStack.Count > 0)
            {
                stateStack.ElementAt(stateStack.Count - 1).Value.Exit(stateId);
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
            for (int i = 0; i < stateStack.Count; i++)
            {
                if (i == stateStack.Count - 1)
                    stateStack.ElementAt(i).Value.Update(gameTime, false);
                else
                    stateStack.ElementAt(i).Value.Update(gameTime, true);
            }      
        }
        public void Draw(SpriteBatch spriteBatch, bool upsideDown = false)
        {
            for (int i = 0; i < stateStack.Count; i++)
            {
                if (i == stateStack.Count - 1)
                    stateStack.ElementAt(i).Value.Draw(spriteBatch, false, upsideDown);
                else
                    stateStack.ElementAt(i).Value.Draw(spriteBatch, true, upsideDown);
            }
        }
    }
}
