﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GameStateMachine
{
    public abstract class GameState
    {
        public float scale = 1.0f;

        public abstract void Enter(int previousStateId);
        public abstract void Exit(int nextStateId);
        public abstract void Suspend(int pushedStateId);
        public abstract void Resume(int poppedStateId);
        public abstract void Draw(SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false);
        public abstract void Update(GameTime gameTime, bool suspended);
    }
}
