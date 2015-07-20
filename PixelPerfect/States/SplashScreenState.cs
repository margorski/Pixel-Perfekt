using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

using Microsoft.Xna.Framework.Media;
namespace PixelPerfect
{
    class SplashScreenState : GameState
    {
        TimeSpan fadeTime = TimeSpan.FromMilliseconds(500.0);
        Texture2D logo;
        private Task task;

        Dictionary<string, string> textureDictionary = new Dictionary<string, string>()
        {
            {"pixel", "pixel"}, {"play", "menu\\play"}, {"music", "menu\\music"}, {"sound", "menu\\sound"}, {"back", "menu\\back"},
            {"info", "menu\\info"}, {"play2", "menu\\play2"}, {"skip", "menu\\skip"}, {"miniTileset", "tileset_mini"}, {"miniPlayer", "player_mini"},
            {"cool", "moods\\cool"}, {"happy", "moods\\happy"}, {"confused", "moods\\confused"}, {"shocked", "moods\\shocked"}, {"scared", "moods\\scared"},
            {"keylock", "keylock"}, {"coolLevel", "moods\\level_cool"}, {"happyLevel", "moods\\level_happy"}, {"confusedLevel", "moods\\level_confused"}, 
            {"shockedLevel", "moods\\level_shocked"}, {"scaredLevel", "moods\\level_scared"}, {"keylockLevel", "keylock_small"}, {"trophy", "menu\\trophy"},
            {"skull", "menu\\skull"}, {"clock", "menu\\clock"}, {"tap", "menu\\tap"}, {"next", "menu\\next"}, {"restart", "menu\\restart"},
            {"suit", "menu\\shirt"}, {"suitbutton", "menu\\suitebtn"}, {"suitbuttonlocked", "menu\\suitebtnlocked"}, {"miniDoor", "door_mini"},
            {"key", "key"}, {"cutscene1", "cutscenes\\cutscene1"}, {"exclamation", "cutscenes\\exclamation"}, {"question", "cutscenes\\question"},
            {"credits", "cutscenes\\credits"}
        };

        public override void Enter(int previousStateId)
        {
            logo = Globals.content.Load<Texture2D>("logo");
#if !WINDOWS
            GamePage.Instance.AdsOff();
#endif
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
            Vector2 position = new Vector2(Config.SCREEN_WIDTH_SCALED / 2,
                                           Config.SCREEN_HEIGHT_SCALED / 2);
            spriteBatch.Draw(logo, position, null, Color.White, 0.0f, new Vector2(logo.Width /2 , logo.Height / 2), 0.5f, SpriteEffects.None, 0.0f);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            if (task == null)
            {
                task = new Task(() =>
                {
                    LoadContent();
                    RegisterStates();
                    Theme.ReloadTheme(World.LastActiveWorld());
                });
                task.Start();
            }

            if (task.IsCompleted)
            {
                Globals.gameStateManager.PopState();
                Globals.gameStateManager.PushState(Config.States.BACKGROUND);
                Globals.gameStateManager.PushState(Config.States.DUMMY);
                Globals.gameStateManager.PushState(Config.States.TITLESCREEN);
                //Globals.gameStateManager.PushState(Config.States.FIFTH_CUTSCENE);
            }
        }

        private void RegisterStates()
        {
            var menuState = new TitlescreenState(Globals.gameStateManager);
            var backgroundLevel = Theme.CurrentTheme.level;
            backgroundLevel.scale = scale;
            var pauseState = new PauseState();
            menuState.scale = pauseState.scale = scale;
            var levelSelectState = new LevelSelectState(Globals.gameStateManager);
            levelSelectState.scale = scale;
            var backgroundState = new TextureBackgroundState(Theme.Cool.color1, Theme.Cool.color2);
            var dummyState = new DummyState();
            var worldSelectState = new WorldSelectState(Globals.gameStateManager);
            worldSelectState.scale = scale;
            var levelDetailsState = new LevelDetailsState(Globals.gameStateManager);
            levelDetailsState.scale = scale;
            var tapState = new TapState();
            tapState.scale = scale;
            var controlsState = new ControlsState();
            controlsState.scale = scale;
            var suitState = new SuitSelectState(Globals.gameStateManager);
            suitState.scale = scale;
            var suitUnlockedState = new SuitUnlockedState();
            var winState = new WinState();
            winState.scale = scale;

            Globals.gameStateManager.RegisterState(Config.States.WIN, winState);
            Globals.gameStateManager.RegisterState(Config.States.SUITUNLOCKED, suitUnlockedState);
            Globals.gameStateManager.RegisterState(Config.States.SUITSELECT, suitState);
            Globals.gameStateManager.RegisterState(Config.States.CONTROLS, controlsState);
            Globals.gameStateManager.RegisterState(Config.States.TAP, tapState);
            Globals.gameStateManager.RegisterState(Config.States.BACKGROUND, backgroundState);
            Globals.gameStateManager.RegisterState(Config.States.TITLESCREEN, menuState);
            Globals.gameStateManager.RegisterState(Config.States.PAUSE, pauseState);
            Globals.gameStateManager.RegisterState(Config.States.WORLDSELECT, worldSelectState);
            Globals.gameStateManager.RegisterState(Config.States.LEVELSELECT, levelSelectState);
            Globals.gameStateManager.RegisterState(Config.States.LEVELDETAILS, levelDetailsState);
            Globals.gameStateManager.RegisterState(Config.States.DUMMY, dummyState);

            PrepareCutscenes();
        }

        private void PrepareCutscenes()
        {
            PrepareFirstCutscene();
            PrepareLastCutscene();
        }

        private void PrepareFirstCutscene()
        {            
            Cutscene.CutsceneState cutsceneState = new Cutscene.CutsceneState();
            Cutscene.Scene scene = new Cutscene.Scene();
            scene._duration = TimeSpan.FromSeconds(32.0);
            #region BACKGROUND
            Cutscene.Image background = new Cutscene.Image();
            background.textureName = "cutscene1";
            Cutscene.Keyframe keyframe = new Cutscene.Keyframe();
            keyframe._time = TimeSpan.FromSeconds(0.0);
            keyframe.scale = 2.0f;
            background.keyframeList.Add(keyframe);
            Cutscene.Keyframe keyframe2 = new Cutscene.Keyframe(keyframe);
            keyframe2._time = TimeSpan.FromSeconds(30.0);
            background.keyframeList.Add(keyframe2);
            scene.items.Add(background);
            #endregion
            #region KEYS
            TimeSpan[] keyframeTime = {
                                          TimeSpan.FromSeconds(11.0),
                                          TimeSpan.FromSeconds(15.0),
                                          TimeSpan.FromSeconds(15.7),
                                          TimeSpan.FromSeconds(16.4),
                                          TimeSpan.FromSeconds(17.1),
                                          TimeSpan.FromSeconds(17.8),
                                          TimeSpan.FromSeconds(18.5),
                                          TimeSpan.FromSeconds(19.2),
                                          TimeSpan.FromSeconds(19.9),
                                          TimeSpan.FromSeconds(20.6),
                                          TimeSpan.FromSeconds(21.3),
                                          TimeSpan.FromSeconds(23.2)
                                      };
            int currentKey = 11;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5 - i; j++)
                {
                    Cutscene.Image key = new Cutscene.Image();
                    key.textureName = "key";
                    Cutscene.Keyframe keykeyframe0 = new Cutscene.Keyframe(keyframe);
                    keykeyframe0.position = new Vector2(20 + j * 12 + i * 6, 122 - i * 16);
                    key.keyframeList.Add(keykeyframe0);

                    Cutscene.Keyframe keykeyframe1 = new Cutscene.Keyframe(keyframe2);
                    keykeyframe1.position = keykeyframe0.position;
                    keykeyframe1._time = keyframeTime[currentKey];
                    key.keyframeList.Add(keykeyframe1);

                    Cutscene.Keyframe keykeyframe2 = new Cutscene.Keyframe(keykeyframe1);
                    keykeyframe2.position = new Vector2(63, 94);
                    keykeyframe2._time = keyframeTime[currentKey] + TimeSpan.FromSeconds(0.5);
                    //if (currentKey == 11)
                    //    keykeyframe2._time += TimeSpan.FromSeconds(1.3);
                    key.keyframeList.Add(keykeyframe2);

                    if (currentKey == 11)
                    {
                        Cutscene.Keyframe keykeyframe2andhalf1 = new Cutscene.Keyframe(keykeyframe2);
                        keykeyframe2andhalf1._time += TimeSpan.FromSeconds(1.3);
                        key.keyframeList.Add(keykeyframe2andhalf1);

                        Cutscene.Keyframe keykeyframe2andhalf2 = new Cutscene.Keyframe(keykeyframe2andhalf1);
                        keykeyframe2andhalf2.position = new Vector2(73, 94);
                        keykeyframe2andhalf2._time += TimeSpan.FromSeconds(2.5);
                        key.keyframeList.Add(keykeyframe2andhalf2);
                    }
                    
                    Cutscene.Keyframe keykeyframe3 = new Cutscene.Keyframe(keykeyframe2);
                    keykeyframe3._time = keyframeTime[currentKey] + TimeSpan.FromSeconds(2.0);
                    if (currentKey == 11)
                        keykeyframe3._time += TimeSpan.FromSeconds(3.8);
                    keykeyframe3.position = new Vector2(293, 94);
                    key.keyframeList.Add(keykeyframe3);   

                    scene.items.Add(key);
                    currentKey--;
                }
            }
            #endregion
            #region MAIN_PIKPOK
            Cutscene.AnimatedImage pikpok = new Cutscene.AnimatedImage();
            pikpok.textureName = "enemies_16x16";
            pikpok.reverse = false;
            pikpok.frameCount = Config.ANIM_FRAMES;
            pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
            pikpok.startFrame = new Rectangle(0,0, 16, 16);

            Cutscene.Keyframe pikpok_keyframe1 = new Cutscene.Keyframe();
            pikpok_keyframe1._time = TimeSpan.FromSeconds(2.0);
            pikpok_keyframe1.position = new Vector2(280, 106);
            pikpok_keyframe1.scale = 2.0f;
            pikpok.keyframeList.Add(pikpok_keyframe1);

            Cutscene.Keyframe pikpok_keyframe2 = new Cutscene.Keyframe();
            pikpok_keyframe2.scale = 2.0f;
            pikpok_keyframe2.position = new Vector2(150, 106);
            pikpok_keyframe2._time = TimeSpan.FromSeconds(7.0);
            pikpok_keyframe2.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe2);

            Cutscene.Keyframe pikpok_keyframe3 = new Cutscene.Keyframe();
            pikpok_keyframe3.scale = 2.0f;
            pikpok_keyframe3.position = new Vector2(150, 106);
            pikpok_keyframe3._time = TimeSpan.FromSeconds(10.0);
            pikpok_keyframe3.animated = true;
            pikpok.keyframeList.Add(pikpok_keyframe3);

            Cutscene.Keyframe pikpok_keyframe4 = new Cutscene.Keyframe();
            pikpok_keyframe4.scale = 2.0f;
            pikpok_keyframe4.position = new Vector2(50, 106);
            pikpok_keyframe4._time = TimeSpan.FromSeconds(11.0);
            pikpok_keyframe4.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe4);

            Cutscene.Keyframe pikpok_keyframe5 = new Cutscene.Keyframe();
            pikpok_keyframe5.scale = 2.0f;
            pikpok_keyframe5.position = new Vector2(50, 106);
            pikpok_keyframe5._time = TimeSpan.FromSeconds(11.5);
            pikpok_keyframe5.animated = true;
            pikpok.keyframeList.Add(pikpok_keyframe5);

            Cutscene.Sound sound = new Cutscene.Sound();
            sound.soundName = "coin";
            sound._time = TimeSpan.FromSeconds(11.5);

            Cutscene.Keyframe pikpok_keyframe6 = new Cutscene.Keyframe();
            pikpok_keyframe6.scale = 2.0f;
            pikpok_keyframe6.position = new Vector2(280, 106);
            pikpok_keyframe6._time = TimeSpan.FromSeconds(13.0);
            pikpok.keyframeList.Add(pikpok_keyframe6);
            scene.sounds.Add(sound);

            scene.items.Add(pikpok);
            #endregion
            TimeSpan baseTimeSpan;
            #region REST_PIKPOK
            for (int i = 0; i < 10; i++)
            {
                baseTimeSpan = TimeSpan.FromSeconds(i * 0.7);
                pikpok = new Cutscene.AnimatedImage();
                pikpok.textureName = "enemies_16x16";
                pikpok.reverse = false;
                pikpok.frameCount = Config.ANIM_FRAMES;
                pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
                pikpok.startFrame = new Rectangle(0, 0, 16, 16);

                pikpok_keyframe1 = new Cutscene.Keyframe();
                pikpok_keyframe1._time = TimeSpan.FromSeconds(13.5) + baseTimeSpan;
                pikpok_keyframe1.position = new Vector2(280, 106);
                pikpok_keyframe1.scale = 2.0f;
                pikpok.keyframeList.Add(pikpok_keyframe1);

                pikpok_keyframe4 = new Cutscene.Keyframe();
                pikpok_keyframe4.scale = 2.0f;
                pikpok_keyframe4.position = new Vector2(50, 106);
                pikpok_keyframe4._time = TimeSpan.FromSeconds(15.0) + baseTimeSpan;
                pikpok_keyframe4.animated = false;
                pikpok.keyframeList.Add(pikpok_keyframe4);

                pikpok_keyframe5 = new Cutscene.Keyframe();
                pikpok_keyframe5.scale = 2.0f;
                pikpok_keyframe5.position = new Vector2(50, 106);
                pikpok_keyframe5._time = TimeSpan.FromSeconds(15.5) + baseTimeSpan;
                pikpok_keyframe5.animated = true;
                pikpok.keyframeList.Add(pikpok_keyframe5);
                Cutscene.Sound sound2 = new Cutscene.Sound(sound);
                sound2.soundName = "coin";
                sound2._time = TimeSpan.FromSeconds(15.5) + baseTimeSpan;
                scene.sounds.Add(sound2);

                pikpok_keyframe6 = new Cutscene.Keyframe();
                pikpok_keyframe6.scale = 2.0f;
                pikpok_keyframe6.position = new Vector2(280, 106);
                pikpok_keyframe6._time = TimeSpan.FromSeconds(17.0) + baseTimeSpan;
                pikpok.keyframeList.Add(pikpok_keyframe6);

                scene.items.Add(pikpok);
            }
            #endregion
            #region LATE_PIKPOK
            baseTimeSpan = TimeSpan.FromSeconds(8.0);
            pikpok = new Cutscene.AnimatedImage();
            pikpok.textureName = "enemies_16x16";
            pikpok.reverse = false;
            pikpok.frameCount = Config.ANIM_FRAMES;
            pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
            pikpok.startFrame = new Rectangle(0, 0, 16, 16);

            pikpok_keyframe1 = new Cutscene.Keyframe();
            pikpok_keyframe1._time = TimeSpan.FromSeconds(13.5) + baseTimeSpan;
            pikpok_keyframe1.position = new Vector2(280, 106);
            pikpok_keyframe1.scale = 2.0f;
            pikpok.keyframeList.Add(pikpok_keyframe1);

            pikpok_keyframe4 = new Cutscene.Keyframe();
            pikpok_keyframe4.scale = 2.0f;
            pikpok_keyframe4.position = new Vector2(50, 106);
            pikpok_keyframe4._time = TimeSpan.FromSeconds(15.2) + baseTimeSpan;
            pikpok_keyframe4.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe4);

            pikpok_keyframe5 = new Cutscene.Keyframe();
            pikpok_keyframe5.scale = 2.0f;
            pikpok_keyframe5.position = new Vector2(50, 106);
            pikpok_keyframe5._time = TimeSpan.FromSeconds(17.0) + baseTimeSpan;
            pikpok_keyframe5.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe5);
            Cutscene.Sound sound3 = new Cutscene.Sound(sound);
            sound3.soundName = "coin";
            sound3._time = TimeSpan.FromSeconds(23.7);
            scene.sounds.Add(sound3);

            pikpok_keyframe6 = new Cutscene.Keyframe();
            pikpok_keyframe6.scale = 2.0f;
            pikpok_keyframe6.position = new Vector2(60, 106);
            pikpok_keyframe6._time = TimeSpan.FromSeconds(19.5) + baseTimeSpan;
            pikpok_keyframe6.animated = true;
            pikpok.keyframeList.Add(pikpok_keyframe6);

            var pikpok_keyframe7 = new Cutscene.Keyframe();
            pikpok_keyframe7.scale = 2.0f;
            pikpok_keyframe7.position = new Vector2(280, 106);
            pikpok_keyframe7._time = TimeSpan.FromSeconds(21.0) + baseTimeSpan;
            pikpok.keyframeList.Add(pikpok_keyframe7);

            scene.items.Add(pikpok);

            #endregion
            #region PLAYER
            Vector2 startVector = new Vector2(-15.0f, 106);
            Vector2 endVector = new Vector2(15.0f, 106);

            var playerItem = Cutscene.AnimatedImage.Player(startVector, endVector, TimeSpan.FromSeconds(22.0), 15.0f, 2.0f);
            Cutscene.Keyframe last = new Cutscene.Keyframe(playerItem.keyframeList[1]);
            last._time = TimeSpan.FromSeconds(30.0);

            scene.items.Add(playerItem);

            var playerItemShocked = Cutscene.AnimatedImage.Player(endVector, endVector, TimeSpan.FromSeconds(24), 5.0f, 2.0f);
            playerItemShocked.staticFrame = 1;
            playerItemShocked.startFrame = new Rectangle(24, 16, 8, 16);
            playerItemShocked.keyframeList[0].animated = false;
            playerItemShocked.keyframeList[1]._time = TimeSpan.FromSeconds(25.5);
            playerItemShocked.keyframeList[1].animated = false;
            Cutscene.Keyframe playerKeyframe0 = new Cutscene.Keyframe(playerItemShocked.keyframeList[1]);
            playerKeyframe0._time = TimeSpan.FromSeconds(27.5);
            playerKeyframe0.position.X += 10;
            playerItemShocked.keyframeList.Add(playerKeyframe0);
            scene.items.Add(playerItemShocked);

            var playerItemScared = Cutscene.AnimatedImage.Player(endVector + new Vector2(10, 0), endVector + new Vector2(10, 0), TimeSpan.FromSeconds(27.5), 10.0f, 2.0f);
            playerItemScared.startFrame = new Rectangle(32, 16, 8, 16);
            playerItemScared.keyframeList[1]._time += TimeSpan.FromSeconds(1);

            Cutscene.Keyframe playerKeyframe1 = new Cutscene.Keyframe(playerItemScared.keyframeList[1]);
            playerKeyframe1._time += TimeSpan.FromSeconds(1.5);
            playerKeyframe1.position.X += 240;
            playerItemScared.keyframeList.Add(playerKeyframe1);
            scene.items.Add(playerItemScared);

            #endregion

            #region QUESTIONMARK
            Cutscene.Image questionMark = new Cutscene.Image();
            questionMark.textureName = "question";
            Cutscene.Keyframe questionKeyframe0 = new Cutscene.Keyframe();
            questionKeyframe0.scale = 2.0f;
            questionKeyframe0._time = TimeSpan.FromSeconds(7.0);
            questionKeyframe0.position = new Vector2(160, 90);
            Cutscene.Keyframe questionKeyframe1 = new Cutscene.Keyframe(questionKeyframe0);
            questionKeyframe1._time = TimeSpan.FromSeconds(8.5);
            questionMark.keyframeList.Add(questionKeyframe0);
            questionMark.keyframeList.Add(questionKeyframe1);
            scene.items.Add(questionMark);
            #endregion
            #region EXCLAMATIONMARK
            Cutscene.Image exclamationMark = new Cutscene.Image();
            exclamationMark.textureName = "exclamation";
            Cutscene.Keyframe exclamationKeyframe0 = new Cutscene.Keyframe();
            exclamationKeyframe0.scale = 2.0f;
            exclamationKeyframe0._time = TimeSpan.FromSeconds(8.5);
            exclamationKeyframe0.position = new Vector2(160, 90);
            Cutscene.Keyframe exclamationKeyframe1 = new Cutscene.Keyframe(exclamationKeyframe0);
            exclamationKeyframe1._time = TimeSpan.FromSeconds(10.0);
            exclamationMark.keyframeList.Add(exclamationKeyframe0);
            exclamationMark.keyframeList.Add(exclamationKeyframe1);
            scene.items.Add(exclamationMark);
            #endregion

            cutsceneState.scenes.Add(scene);            
            scene.Init();

            Globals.gameStateManager.RegisterState(Config.States.FIRST_CUTSCENE, cutsceneState);            
        }

        private void PrepareLastCutscene()
        {
            TimeSpan endTime = TimeSpan.FromSeconds(100.0);
            Cutscene.CutsceneState cutsceneState = new Cutscene.CutsceneState();            
            Cutscene.Scene scene = new Cutscene.Scene();
            scene._duration = endTime;
            scene.backgroundColor = Color.Black;
            Cutscene.Image background = new Cutscene.Image();
            background.textureName = "credits";
            Cutscene.Keyframe startKeyframe = new Cutscene.Keyframe();
            startKeyframe._time = TimeSpan.Zero;
            startKeyframe.position = new Vector2(0, Config.SCREEN_HEIGHT_SCALED);            
            background.keyframeList.Add(startKeyframe);
            Cutscene.Keyframe endKeyframe = new Cutscene.Keyframe();
            endKeyframe._time = endTime;
            endKeyframe.position = new Vector2(0, -(Globals.textureDictionary["credits"].Height - Config.SCREEN_HEIGHT_SCALED));
            background.keyframeList.Add(endKeyframe);

            Cutscene.Text text = new Cutscene.Text();
            text.text = "CONGRATULATIONS! \n\nCONSIDER YOURSELF\nA HERO!";
            Cutscene.Keyframe textkeyframe = new Cutscene.Keyframe(startKeyframe);
            textkeyframe.printedLetters = text.text.Length;
            textkeyframe.position = new Vector2(80,100);
            textkeyframe.scale = 1.4f;
            textkeyframe.color = Color.White;
            text.keyframeList.Add(textkeyframe);
            endKeyframe = new Cutscene.Keyframe(textkeyframe);
            endKeyframe._time = TimeSpan.FromSeconds(10);
            text.keyframeList.Add(endKeyframe);

            scene.items.Add(background);
            scene.items.Add(text);
            cutsceneState.scenes.Add(scene);
            scene.Init();

            Globals.gameStateManager.RegisterState(Config.States.FIFTH_CUTSCENE, cutsceneState);            
        }

        //private void PrepareSecondCutscene()
        //{
        //    Cutscene.CutsceneState cutsceneState = new Cutscene.CutsceneState();
        //    Cutscene.Scene scene = new Cutscene.Scene();
        //    scene.gradientTexture = Util.GetGradientTexture(1, Config.SCREEN_HEIGHT_SCALED, Color.Orange, Color.Yellow, Util.GradientType.Horizontal);
        //    scene._duration = TimeSpan.FromSeconds(50.0);

        //    #region MAIN_PIKPOK
        //    Cutscene.AnimatedImage pikpok = new Cutscene.AnimatedImage();
        //    pikpok.textureName = "enemies_16x16";
        //    pikpok.reverse = false;
        //    pikpok.frameCount = Config.ANIM_FRAMES;
        //    pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
        //    pikpok.startFrame = new Rectangle(0, 0, 16, 16);

        //    Cutscene.Keyframe pikpok_keyframe1 = new Cutscene.Keyframe();
        //    pikpok_keyframe1._time = TimeSpan.FromSeconds(1.0);
        //    pikpok_keyframe1.position = new Vector2(-20, 76);
        //    pikpok.keyframeList.Add(pikpok_keyframe1);

        //    Cutscene.Keyframe pikpok_keyframe2 = new Cutscene.Keyframe();
        //    pikpok_keyframe2.position = new Vector2(200, 90);
        //    pikpok_keyframe2._time = TimeSpan.FromSeconds(7.0);            
        //    pikpok.keyframeList.Add(pikpok_keyframe2);

        //    Cutscene.Keyframe pikpok_keyframe3 = new Cutscene.Keyframe();
        //    pikpok_keyframe3.position = new Vector2(40, 60);
        //    pikpok_keyframe3._time = TimeSpan.FromSeconds(13.0);
        //    pikpok.keyframeList.Add(pikpok_keyframe3);

        //    Cutscene.Keyframe pikpok_keyframe4 = new Cutscene.Keyframe();
        //    pikpok_keyframe4.position = new Vector2(160, 120);
        //    pikpok_keyframe4._time = TimeSpan.FromSeconds(18.0);            
        //    pikpok.keyframeList.Add(pikpok_keyframe4);

        //    Cutscene.Keyframe pikpok_keyframe5 = new Cutscene.Keyframe();
        //    pikpok_keyframe5.position = new Vector2(120, 20);
        //    pikpok_keyframe5._time = TimeSpan.FromSeconds(22.0);            
        //    pikpok.keyframeList.Add(pikpok_keyframe5);


        //    Cutscene.Keyframe pikpok_keyframe7 = new Cutscene.Keyframe();
        //    pikpok_keyframe7.position = new Vector2(Config.SCREEN_WIDTH_SCALED / 2, Config.SCREEN_HEIGHT_SCALED / 2);
        //    pikpok_keyframe7._time = TimeSpan.FromSeconds(26.0);
        //    pikpok.keyframeList.Add(pikpok_keyframe7);

        //    Cutscene.Keyframe pikpok_keyframe8 = new Cutscene.Keyframe();
        //    pikpok_keyframe8.position = new Vector2(Config.SCREEN_WIDTH_SCALED / 2, Config.SCREEN_HEIGHT_SCALED / 2);
        //    pikpok_keyframe8._time = TimeSpan.FromSeconds(50.0);
        //    pikpok.keyframeList.Add(pikpok_keyframe8);

        //    scene.items.Add(pikpok);
        //    #endregion            
            
        //    #region PLAYERS
        //    var playerItem = Cutscene.AnimatedImage.Player(new Vector2(280, 90), new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + 50, Config.SCREEN_HEIGHT_SCALED / 2), TimeSpan.FromSeconds(4.5), 5.0f);
        //    playerItem.keyframeList[playerItem.keyframeList.Count - 1]._time = TimeSpan.FromSeconds(40.0);
        //    Cutscene.Keyframe waitKeyframe = new Cutscene.Keyframe(playerItem.keyframeList[playerItem.keyframeList.Count - 1]);
        //    waitKeyframe._time = TimeSpan.FromSeconds((28.0));
        //    playerItem.keyframeList.Add(waitKeyframe);
        //    scene.items.Add(playerItem);

        //    playerItem = Cutscene.AnimatedImage.Player(new Vector2(-20, 50), new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - 40, Config.SCREEN_HEIGHT_SCALED / 2), TimeSpan.FromSeconds(9.5), 5.0f);
        //    playerItem.keyframeList[playerItem.keyframeList.Count - 1]._time = TimeSpan.FromSeconds(40.0);
        //    waitKeyframe = new Cutscene.Keyframe(playerItem.keyframeList[playerItem.keyframeList.Count - 1]);
        //    waitKeyframe._time = TimeSpan.FromSeconds((28.0));
        //    playerItem.keyframeList.Add(waitKeyframe);
        //    scene.items.Add(playerItem);

        //    playerItem = Cutscene.AnimatedImage.Player(new Vector2(180, 160), new Vector2(Config.SCREEN_WIDTH_SCALED / 2, Config.SCREEN_HEIGHT_SCALED / 2 + 40), TimeSpan.FromSeconds(17.0), 5.0f);
        //    playerItem.keyframeList[playerItem.keyframeList.Count - 1]._time = TimeSpan.FromSeconds(40.0);
        //    waitKeyframe = new Cutscene.Keyframe(playerItem.keyframeList[playerItem.keyframeList.Count - 1]);
        //    waitKeyframe._time = TimeSpan.FromSeconds((28.0));
        //    playerItem.keyframeList.Add(waitKeyframe);
        //    scene.items.Add(playerItem);

        //    playerItem = Cutscene.AnimatedImage.Player(new Vector2(120, -10), new Vector2(Config.SCREEN_WIDTH_SCALED / 2, Config.SCREEN_HEIGHT_SCALED / 2 - 40), TimeSpan.FromSeconds(22.5), 5.0f);
        //    playerItem.keyframeList[playerItem.keyframeList.Count - 1]._time = TimeSpan.FromSeconds(40.0);
        //    waitKeyframe = new Cutscene.Keyframe(playerItem.keyframeList[playerItem.keyframeList.Count - 1]);            
        //    waitKeyframe._time = TimeSpan.FromSeconds((28.0));
        //    playerItem.keyframeList.Add(waitKeyframe);
        //    scene.items.Add(playerItem);

        //    #endregion
        //    cutsceneState.scenes.Add(scene);                                   
        //    scene.Init();

        //    Globals.gameStateManager.RegisterState(Config.States.SECOND_CUTSCENE, cutsceneState);  

        //}

        private void LoadContent()
        {
            Globals.silkscreenFont = Globals.content.Load<SpriteFont>("Silkscreen");
            Globals.tileset = new Tileset("tileset");
            LoadTextures();
            LoadSprites();
            LoadSounds();
            LoadMusic();            
        }

        private void LoadTextures()
        {
            foreach (KeyValuePair<string, string> texture in textureDictionary)
            {
                if (!Globals.textureDictionary.ContainsKey(texture.Key))
                    Globals.textureDictionary.Add(texture.Key, Globals.content.Load<Texture2D>(texture.Value));
            }
        }
        
        private void LoadSprites()
        {
            Globals.spritesDictionary.Add("biggo_128x128", new Sprite("biggo_128x128", 128, 128, 2));
            Globals.spritesDictionary.Add("enemies_16x16", new Sprite("enemies_16x16", 16, 16));
            Globals.spritesDictionary.Add("enemies_32x32", new Sprite("enemies_32x32", 32, 32));
            Globals.spritesDictionary.Add("enemies_8x8", new Sprite("enemies_8x8", 8, 8));
            Globals.spritesDictionary.Add("king_48x48", new Sprite("king_48x48", 48, 48, 14));
            Globals.spritesDictionary.Add("player", new Sprite("player", 8, 16, 6));
        }

        private void LoadSounds()
        {
            Globals.soundsDictionary.Add("coin", Globals.content.Load<SoundEffect>(@"Sounds\Pickup_Coin8").CreateInstance());
            Globals.soundsDictionary.Add("jump", Globals.content.Load<SoundEffect>(@"Sounds\Jump4").CreateInstance());
            Globals.soundsDictionary.Add("explosion", Globals.content.Load<SoundEffect>(@"Sounds\Explosion9").CreateInstance());
            Globals.soundsDictionary.Add("randomize", Globals.content.Load<SoundEffect>(@"Sounds\Randomize3").CreateInstance());
            Globals.soundsDictionary.Add("hit", Globals.content.Load<SoundEffect>(@"Sounds\Hit_Hurt2").CreateInstance());
            Globals.soundsDictionary.Add("doors", Globals.content.Load<SoundEffect>(@"Sounds\Randomize2").CreateInstance());
            Globals.soundsDictionary["doors"].IsLooped = true;
            foreach (KeyValuePair<string, SoundEffectInstance> sfeffect in Globals.soundsDictionary)
                sfeffect.Value.Volume = 0.15f;
            Globals.soundsDictionary["hit"].Volume = 0.2f;
            Globals.soundsDictionary["coin"].Volume = 0.12f;
            Globals.soundsDictionary["doors"].Volume = 0.2f;
            Globals.soundsDictionary["doors"].Pitch = 0.5f;
        }

        private void LoadMusic()
        {
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\xylophone (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Elevator Music (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\8-bit loop (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Gasoline Rainbows (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Chippy Cloud Kid (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\ChipChippy (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Sad Song 1"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Dramatic Metal Entrance (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Chaotic Filth (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Chaotic Standoff (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Ring Leader (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Rising Sun (oriental with dance beats)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\Vanguard Bouncy (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\wubby dancer (loop)"));
            Globals.backgroundMusicList.Add(Globals.content.Load<Song>(@"music\King Boss (loop)"));
        }

    }
}
