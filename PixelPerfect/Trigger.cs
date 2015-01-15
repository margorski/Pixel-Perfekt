using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class Trigger
    {
        private Rectangle area;
        private int useCount;
        private bool isIn = false;
        private Config.TriggerType triggerType;
        private GameStateManager gameStateManager;
        private int stateId = -1;

        public Trigger(Rectangle area, int useCount, Config.TriggerType triggerType, GameStateManager gameStateManager)
        {
            this.area = area;
            this.useCount = useCount;
            this.triggerType = triggerType;
            this.gameStateManager = gameStateManager;
        }

        public void SetStateID(int stateId) { this.stateId = stateId; }
        public void CheckTrigger(Rectangle playerRectangle)
        {
            if (useCount == 0)
                return;

            if (isIn)
            {
                if (!playerRectangle.Intersects(area) && !playerRectangle.Contains(area) && !area.Contains(playerRectangle))
                {
                    isIn = false;
                }
            }
            else
            {
                if (playerRectangle.Intersects(area))
                {
                    useCount--;
                    isIn = true;
                    ActivateTrigger();
                }
            }
        }

        private void ActivateTrigger()
        {
            switch (triggerType)
            {
                case Config.TriggerType.NONE:
                    break;
                    
                case Config.TriggerType.PUSHSTATE:
                    gameStateManager.PushState(stateId);
                    break;

                case Config.TriggerType.POPSTATE:
                    gameStateManager.PopState();
                    break;

                case Config.TriggerType.CHANGESTATE:
                    gameStateManager.ChangeState(stateId);
                    break;

            }
        }
    }
}
