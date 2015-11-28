using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#if ANDROID
using IsolatedStorageSettings = CustomIsolatedStorageSettings.IsolatedStorageSettings;
#endif

namespace PixelPerfect
{
    class WinState : GameState
    {
        enum ScreenState
        {
            WAIT,
            DEATHCOUNT,
            DELAY,
            TIMECOUNT,
            TROPHY,
            IDLE
        }

        private TimeSpan fadeTime;
        ScreenState screenState = ScreenState.WAIT;

#if WINDOWS
        private MouseState prevMouseState;
        private MouseState currMouseState;
#else
        TouchCollection touchCollection;
#endif
        GamePadState prevGPState;
        GamePadState currGPState;

        WavyText caption = new WavyText("WELL DONE!", new Vector2(80, 7), 3000, 2.0f, Config.titleColors, 13.0f, 3f, 0.0f);

        Button backButton = new Button("", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 36 - 40, 120, 24, 24), Globals.textureDictionary["back"], Globals.silkscreenFont, false);
        Button restartButton = new Button("", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 12, 120, 24, 24), Globals.textureDictionary["restart"], Globals.silkscreenFont, false);
        Button playButton = new Button("", new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 + 16 + 40, 120, 24, 24), Globals.textureDictionary["next"], Globals.silkscreenFont, false);

        private string levelId = "";
        private TimeSpan levelTime = TimeSpan.Zero;
        private int deaths = -1;


        private float currentDeaths = 0;
        private float currentTotalDeaths = 0;
        private TimeSpan currentLevelTime = TimeSpan.Zero;
        private TimeSpan soundTimer = TimeSpan.Zero;
        private TimeSpan diffTime = TimeSpan.Zero;

        private Color currentDeathsColor = Color.White;
        private Color currentTotalDeathsColor = Color.White;
        private Color currentLevelTimeColor = Color.White;
        
        private TimeSpan delayTimer = TimeSpan.FromMilliseconds(100.0);

        private List<EmiterPart> fireworks = new List<EmiterPart>();

        private TimeSpan trophyTimer = TimeSpan.FromMilliseconds(500.0);
        private bool trophy = false;
        private TimeSpan diffPreviousTime = TimeSpan.Zero;

        public WinState()
        {
        }

        public void SetStats(string levelId, TimeSpan levelTime, int deaths)
        {
            this.levelId = levelId;
            this.levelTime = levelTime;
            this.deaths = deaths;
        }

        public override void Enter(int previousStateId)
        {
            if (deaths < 0 || levelTime == TimeSpan.Zero || levelId == "")
            {
                Globals.gameStateManager.PopState();
                return;
            }
            fireworks.Clear();
            for (int i = 0; i < 5; i++)
                fireworks.Add(new EmiterPart(new Vector2(200 - 30 * i, 180),
                              125 + (uint)Globals.rnd.Next(30), 200.0f + Globals.rnd.Next(40), MovementDirection.Up,
                              Globals.spritesDictionary["enemies_8x8"].texture,
                              Globals.spritesDictionary["enemies_8x8"].textureArray[0],
                              new Rectangle(0, 0, 8, 8), Color.White, 100, Globals.pixelParticles, null, true, false, true));
            fadeTime = new TimeSpan(0, 0, 0, 0, 500);

            diffPreviousTime = Globals.worlds[Globals.selectedWorld].DiffPreviousTime(Globals.selectedLevel, levelTime);

            if (Globals.musicEnabled)
                MediaPlayer.Pause();
#if WINDOWS
            currMouseState = prevMouseState = Mouse.GetState();
#else
            touchCollection = TouchPanel.GetState();
#endif
            bool checkForSuitUnlock = false;
            if (!Savestate.Instance.levelSaves[levelId].completed)
            {
                Savestate.Instance.levelSaves[levelId].completed = true;
                Savestate.Instance.levelSaves[levelId].skipped = false;
                Savestate.Instance.levelSaves[levelId].bestTime = levelTime;

                // checking if time is beaten, if so then maybe unlock character
                if (Globals.worlds[Globals.selectedWorld].BeatLevelPerfektTime(Globals.selectedLevel))
                    checkForSuitUnlock = true;
            }
            else if (Savestate.Instance.levelSaves[levelId].bestTime > levelTime)
            {
                bool beatenpreviously = false;
                if (Globals.worlds[Globals.selectedWorld].BeatLevelPerfektTime(Globals.selectedLevel))
                    beatenpreviously = true;

                Savestate.Instance.levelSaves[levelId].bestTime = levelTime;

                if (!beatenpreviously && Globals.worlds[Globals.selectedWorld].BeatLevelPerfektTime(Globals.selectedLevel))
                    checkForSuitUnlock = true;
            }

            if (checkForSuitUnlock)
            {
                var perfektTimeCount = World.BeatPerfektTimeCount();

                if (perfektTimeCount == 50)
                {
                    UnlockSuit(Config.Player.SUIT_QTY - 1);
                }
                else if (perfektTimeCount % 3 == 0)
                {
                    UnlockRandomSuit();
                }
            }
            Savestate.Instance.Save();

            if (Globals.worlds[Globals.selectedWorld].Completed() &&
                Globals.selectedWorld == Globals.worlds.Count - 1 &&                
                !Globals.completed)
            {                
                backButton.active = false;
                restartButton.active = false;                
            }
            else
            {
                backButton.active = true;
                restartButton.active = true;
            }
            

            Globals.soundsDictionary["coin"].Pitch = 1.0f;
        }

        public override void Exit(int nextStateId)
        {
            screenState = ScreenState.DEATHCOUNT;
            currentDeaths = currentTotalDeaths = 0.0f;
            currentLevelTime = TimeSpan.Zero;
            diffTime = TimeSpan.Zero;
            diffPreviousTime = TimeSpan.Zero;
            currentDeathsColor = currentLevelTimeColor = currentTotalDeathsColor = Color.White;
            Globals.soundsDictionary["coin"].Pitch = 0.0f;
            delayTimer = TimeSpan.Zero;
            trophy = false;
            trophyTimer = TimeSpan.FromMilliseconds(500.0);
            backButton.active = restartButton.active = true;
        }

        public override void Suspend(int pushedStateId)
        {

        }

        public override void Resume(int poppedStateId)
        {
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, bool suspended, bool upsidedownBatch = false)
        {
            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, 0, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED), new Color(0, 0, 0, (int)((1.0f - (float)fadeTime.TotalMilliseconds / 500.0f) * 200)));

            caption.Draw(spriteBatch);
            
            spriteBatch.DrawString(Globals.silkscreenFont, "DEATHS:", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 55, 32), Color.White, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, ((int)currentDeaths).ToString("D3"), new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 56, 32), currentDeathsColor, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.textureDictionary["skull"], new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 80, 30,
                                                                               Globals.textureDictionary["skull"].Width * 2,
                                                                               Globals.textureDictionary["skull"].Height * 2), Color.White);

            spriteBatch.DrawString(Globals.silkscreenFont, "TOTAL DEATHS:", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 55, 58), Color.White, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, ((int)currentTotalDeaths).ToString("D3"), new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 56, 58), currentTotalDeathsColor, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Globals.textureDictionary["skull"], new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 80, 58,
                                                                   Globals.textureDictionary["skull"].Width * 2,
                                                                   Globals.textureDictionary["skull"].Height * 2), Color.White);

            if (trophy)
                spriteBatch.Draw(Globals.textureDictionary["trophy"], new Rectangle(148, 70,
                                                                                Globals.textureDictionary["trophy"].Width * 2,
                                                                                Globals.textureDictionary["trophy"].Height * 2), new Color(Color.Gold, (float)(1.0 - trophyTimer.TotalMilliseconds / 500.0)));

            spriteBatch.DrawString(Globals.silkscreenFont, "TIME:", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 55, 84), Color.White, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, currentLevelTime.ToString("mm\\:ss\\.ff"), new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 24, 84), currentLevelTimeColor, 0.0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.0f);
            if (screenState == ScreenState.TROPHY || screenState == ScreenState.IDLE)
            {
                spriteBatch.DrawString(Globals.silkscreenFont, "(" + (diffTime.TotalMilliseconds > 0.0 ? "+" : "-") + diffTime.ToString("m\\:ss\\.ff") + ")", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 84, 87), (diffTime.TotalMilliseconds > 0.0 ? Color.Red : Color.Gold), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);                
                if (diffPreviousTime.TotalMilliseconds < 0.0)
                {
                    spriteBatch.DrawString(Globals.silkscreenFont, "NEW RECORD (-" + diffPreviousTime.ToString("m\\:ss\\.ff") + ")", new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 1, 100), Color.CornflowerBlue, 0.0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0.0f);
                }
            }
            spriteBatch.Draw(Globals.textureDictionary["clock"], new Rectangle(Config.SCREEN_WIDTH_SCALED / 2 - 80, 82,
                                                                   Globals.textureDictionary["clock"].Width * 2,
                                                                   Globals.textureDictionary["clock"].Height * 2), Color.White);
            
            spriteBatch.DrawString(Globals.silkscreenFont, "BACK", new Vector2(56, 145), Color.White, 0.0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "RESTART", new Vector2(111, 145), Color.White, 0.0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(Globals.silkscreenFont, "NEXT", new Vector2(187, 145), Color.White, 0.0f, Vector2.Zero, 1.2f, SpriteEffects.None, 0.0f);

            playButton.Draw(spriteBatch);
            restartButton.Draw(spriteBatch);
            backButton.Draw(spriteBatch);

            foreach (EmiterPart emiterpart in fireworks)
                emiterpart.Draw(spriteBatch, Vector2.Zero);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            if (suspended)
                return;

            caption.Update(gameTime);
            foreach (EmiterPart emiterpart in fireworks)
                emiterpart.Update(gameTime);

            if (fadeTime > TimeSpan.Zero)
            {
                fadeTime -= gameTime.ElapsedGameTime;
                if (fadeTime < TimeSpan.Zero)
                    fadeTime = TimeSpan.Zero;
            }

            currGPState = GamePad.GetState(PlayerIndex.One);
#if !WINDOWS
            touchCollection = TouchPanel.GetState();
#else
            currMouseState = Mouse.GetState();
#endif            
            switch (screenState)
            {

                case ScreenState.WAIT:
                    delayTimer -= gameTime.ElapsedGameTime;
                    if (delayTimer <= TimeSpan.Zero)
                    {
                        screenState = ScreenState.DEATHCOUNT;
                        delayTimer = TimeSpan.FromMilliseconds(500.0);
                    }
                    break;

                case ScreenState.DEATHCOUNT:
                    UpdateCounterSound(gameTime);
                    float deathIncrement = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f * 40.0f);
                    
                    currentDeaths += deathIncrement;
                    if (currentDeaths >= deaths)
                    {
                        currentDeaths = deaths;
                        currentDeathsColor = Color.Blue;
                    }
                    
                    currentTotalDeaths += deathIncrement;
                    if (currentTotalDeaths >= Savestate.Instance.levelSaves[levelId].deathCount)
                    {
                        currentTotalDeaths = Savestate.Instance.levelSaves[levelId].deathCount;
                        currentTotalDeathsColor = Color.Blue;
                    }

                    if (currentDeaths == deaths && currentTotalDeaths == Savestate.Instance.levelSaves[levelId].deathCount)
                        screenState = ScreenState.DELAY;

                    if (Clicked())
                        SetIdle();

                    break;

                case ScreenState.DELAY:
                    delayTimer -= gameTime.ElapsedGameTime;
                    if (delayTimer <= TimeSpan.Zero)
                        screenState = ScreenState.TIMECOUNT;

                    if (Clicked())
                        SetIdle();

                    break;

                case ScreenState.TIMECOUNT:
                    UpdateCounterSound(gameTime);
                    currentLevelTime = currentLevelTime.Add(TimeSpan.FromMilliseconds(gameTime.ElapsedGameTime.TotalMilliseconds * 25.0));
                    if (currentLevelTime >= levelTime)
                    {
                        currentLevelTime = levelTime;
                        currentLevelTimeColor = Color.Blue;
                        
                        diffTime = levelTime - Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].time;

                        if (Globals.worlds[Globals.selectedWorld].BeatLevelPerfektTime(Globals.selectedLevel, levelTime))
                        {
                            trophy = true;
                            screenState = ScreenState.TROPHY;
                        }
                        else
                        {
                            screenState = ScreenState.IDLE;
                        }
                        
                    }

                    if (Clicked())
                        SetIdle();

                    break;

                case ScreenState.TROPHY:
                    trophyTimer -= gameTime.ElapsedGameTime;
                    if (trophyTimer <= TimeSpan.Zero)
                    {
                        screenState = ScreenState.IDLE;
                    }

                    if (Clicked())
                        SetIdle();

                    break;

                case ScreenState.IDLE:                   
                    if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
                        GoBack();

                    if (Globals.unlockedSuit > -1)
                    {
                        Globals.gameStateManager.PushState(Config.States.SUITUNLOCKED, true);
                        return;
                    }
#if !WINDOWS
                    
                    foreach (TouchLocation touch in touchCollection)
                    {
                        if (touch.State == TouchLocationState.Pressed)
                        {
                            restartButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                            backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                            playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, false);
                        }
                        if (touch.State == TouchLocationState.Released)
                        {
                            if (playButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                                NextLevel();
                            else if (restartButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                            {                    
                                Globals.gameStateManager.PopState();
                                Globals.CurrentLevelState.Reset();
                            }
                            else if (backButton.Clicked((int)touch.Position.X, (int)touch.Position.Y, scale, true))
                            {
                                GoBack();
                            }
                        }
                    }
#else
                    if (currMouseState.LeftButton == ButtonState.Pressed)
                    {
                        restartButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                        backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                        playButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, false);
                    }
                    else if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (playButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                            NextLevel();
                        else if (restartButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                        {                    
                            Globals.gameStateManager.PopState();
                            Globals.CurrentLevelState.Reset();
                        }
                        else if (backButton.Clicked(currMouseState.Position.X, currMouseState.Position.Y, scale, true))
                        {
                            GoBack();
                        }
                    }
#endif 
                    break;
            }

#if WINDOWS
            prevMouseState = currMouseState;
#endif
            prevGPState = currGPState;
        }

        private void NextLevel()
        {
            if (Globals.worlds[Globals.selectedWorld].Completed())
            {
                if (Globals.selectedWorld == Globals.worlds.Count - 1)
                {
                    if (!Globals.completed)
                    {

                        Globals.completed = true;
#if !WINDOWS
                        IsolatedStorageSettings.ApplicationSettings["completed"] = true;
                        IsolatedStorageSettings.ApplicationSettings.Save();
#endif
                        Globals.gameStateManager.PopState();
                        Globals.gameStateManager.PopState();
                        Globals.gameStateManager.PushState(Config.States.WORLDSELECT);
                        Globals.gameStateManager.PushState(Config.States.FINAL_CUTSCENE);
                        return;
                    }
                }
                else if (!Globals.worlds[Globals.selectedWorld + 1].active)
                {
                    Theme.ReloadTheme(Globals.selectedWorld + 1, scale);
                    Globals.detonateWorldKeylock = Globals.selectedWorld + 1;
                }
                Globals.gameStateManager.PopState();
                Globals.gameStateManager.ChangeState(Config.States.WORLDSELECT);
            }
            else
            {
                int nextLevel = findNextLevel(Globals.selectedLevel);

                if (nextLevel < 0) // only skipped lvls left
                    nextLevel = findNextLevel(0); // find skipped level                

                if (nextLevel < 0)
                    Globals.gameStateManager.ChangeState(Config.States.LEVELSELECT);
                else
                    Globals.selectedLevel = nextLevel;

                var levelState = new LevelState(Globals.worlds[Globals.selectedWorld].directory, Globals.worlds[Globals.selectedWorld].GetLevelFile(Globals.selectedLevel), false, scale);
                levelState.scale = scale;
                levelState.name = Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].levelName;
                
                Globals.gameStateManager.UnregisterState(Config.States.LEVEL);                
                Globals.gameStateManager.RegisterState(Config.States.LEVEL, levelState);
                Globals.gameStateManager.PopState();
                Globals.gameStateManager.ChangeState(Config.States.LEVEL);
            }
        }

        private int findNextLevel(int startLevel)
        {
            int nextLevel = startLevel;
            do
            {
                nextLevel++;
            }
            while (nextLevel < Globals.worlds[Globals.selectedWorld].levels.Count && Globals.worlds[Globals.selectedWorld].LevelCompleted(nextLevel));

            if (nextLevel >= Globals.worlds[Globals.selectedWorld].levels.Count)
                return -1;
            else
                return nextLevel;
        }

        private void UpdateCounterSound(GameTime gameTime)
        {
            soundTimer += gameTime.ElapsedGameTime;
            if (soundTimer.TotalMilliseconds > 100)
            {                
                if (Globals.soundEnabled)
                    Globals.soundsDictionary["coin"].Play();
                soundTimer = TimeSpan.Zero;
            }
        }

        private bool Clicked()
        {
#if !WINDOWS
            foreach (TouchLocation touch in touchCollection)
            {
                if (touch.State == TouchLocationState.Released)
                {
                    return true;
                }
            }
#else
            if (currMouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                return true;
            }
#endif 
            if (currGPState.Buttons.Back == ButtonState.Pressed && prevGPState.Buttons.Back == ButtonState.Released)
                return true;

            return false;
        }

        private void SetIdle()
        {
            currentDeathsColor = currentTotalDeathsColor = currentLevelTimeColor = Color.Blue;
            currentDeaths = deaths;
            currentTotalDeaths = Savestate.Instance.levelSaves[levelId].deathCount;
            currentLevelTime = levelTime;
            diffTime = levelTime - Globals.worlds[Globals.selectedWorld].levels[Globals.selectedLevel].time;
            screenState = ScreenState.IDLE;
            fadeTime = TimeSpan.Zero;
            if (Globals.worlds[Globals.selectedWorld].BeatLevelPerfektTime(Globals.selectedLevel, levelTime))            
                trophy = true;
            trophyTimer = TimeSpan.Zero;
        }

        public void GoBack()
        {
            // checking if world is completed
            if (Globals.worlds[Globals.selectedWorld].Completed())
            {                
                if (Globals.selectedWorld < Globals.worlds.Count - 1 &&
                    !Globals.worlds[Globals.selectedWorld + 1].active)
                {
                    Theme.ReloadTheme(Globals.selectedWorld + 1, scale);
                    Globals.detonateWorldKeylock = Globals.selectedWorld + 1;
                }
                Globals.gameStateManager.PopState();
                Globals.gameStateManager.ChangeState(Config.States.WORLDSELECT);
            }
            else
            {
                Globals.gameStateManager.PopState();
                Globals.gameStateManager.ChangeState(Config.States.LEVELSELECT);
            }
        }

        private void UnlockSuit(int id)
        {
            Savestate.Instance.suitUnlocked[id] = true;
            Globals.unlockedSuit = id;            
        }

        private void UnlockRandomSuit()
        {
#if DEBUG
            return;
#else
            int rndNumber = 0;

            if (Savestate.Instance.suitUnlocked.All<bool>(suit => suit == true))
                return;

            do
            {
                rndNumber = Globals.rnd.Next(Config.Player.SUIT_QTY - 1);
            }
            while (Savestate.Instance.suitUnlocked[rndNumber]);

            UnlockSuit(rndNumber);
#endif
        }
    }
}
