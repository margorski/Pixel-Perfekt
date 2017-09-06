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
using PixelPerfect.Cutscene;

namespace PixelPerfect
{
    enum LoadState
    {
        Init = 0,
        Textures,
        Sounds,
        Music,
        Sprites,
        Register,
        Finish
    }

    class SplashScreenState : GameState
    {
        TimeSpan fadeTime = TimeSpan.FromMilliseconds(500.0);
        Texture2D logo;
        int loadCounter = 0;
        LoadState loadState = LoadState.Init;
        Dictionary<LoadState, Action> loadDelegates = new Dictionary<LoadState,Action>();

        Dictionary<string, string> textureDictionary = new Dictionary<string, string>()
        {
            {"pixel", "pixel"}, {"play", "menu\\play"}, {"music", "menu\\music"}, {"sound", "menu\\sound"}, {"back", "menu\\back"},
            {"info", "menu\\info"}, {"play2", "menu\\play2"}, {"skip", "menu\\skip"}, {"miniTileset", "Levels\\fgame\\tileset_mini"}, {"miniPlayer", "Levels\\fgame\\player_mini"},
            {"cool", "Levels\\fgame\\cool"}, {"happy", "Levels\\fgame\\happy"}, {"confused", "Levels\\fgame\\confused"}, {"shocked", "Levels\\fgame\\shocked"}, {"scared", "Levels\\fgame\\scared"},
            {"keylock", "Levels\\fgame\\keylock"}, {"coolLevel", "Levels\\fgame\\level_cool"}, {"happyLevel", "Levels\\fgame\\level_happy"}, {"confusedLevel", "Levels\\fgame\\level_confused"}, 
            {"shockedLevel", "Levels\\fgame\\level_shocked"}, {"scaredLevel", "Levels\\fgame\\level_scared"}, {"keylockLevel", "Levels\\fgame\\keylock_small"}, {"trophy", "menu\\trophy"},
            {"skull", "menu\\skull"}, {"clock", "menu\\clock"}, {"tap", "menu\\tap"}, {"next", "menu\\next"}, {"restart", "menu\\restart"},
            {"suit", "menu\\shirt"}, {"suitbutton", "menu\\suitebtn"}, {"suitbuttonlocked", "menu\\suitebtnlocked"}, {"miniDoor", "Levels\\fgame\\door_mini"},
            {"key", "Levels\\fgame\\key"}, {"cutscene1", "Cutscenes\\cutscene1"}, {"exclamation", "Cutscenes\\exclamation"}, {"question", "Cutscenes\\question"},   
            {"ads", "menu\\ads"}, {"swap", "menu\\swap"}
        };

		List<string> musicList = new List<string>()
		{			
			@"music\xylophone",
			@"music\Elevator Music",
			@"music\8-bit loop",
			@"music\Gasoline Rainbows",
			@"music\Chippy Cloud Kid",
			@"music\ChipChippy",
			@"music\Sad Song 1",
			@"music\Dramatic Metal Entrance",
			@"music\Chaotic Filth",
			@"music\Chaotic Standoff",
			@"music\Ring Leader",
			@"music\Rising Sun",
			@"music\Vanguard Bouncy",
			@"music\wubby dancer",
			@"music\King Boss"
		};
			
        List<SoundLoadInfo> soundsList = new List<SoundLoadInfo>() 
        {
            new SoundLoadInfo("coin", @"Sounds\Pickup_Coin8", 0.0f, 0.12f),
            new SoundLoadInfo("jump", @"Sounds\Jump4"),
            new SoundLoadInfo("explosion", @"Sounds\Explosion9"),
            new SoundLoadInfo("randomize", @"Sounds\Randomize3"),
            new SoundLoadInfo("hit", @"Sounds\Hit_Hurt2", 0.0f, 0.2f),
            new SoundLoadInfo("doors", @"Sounds\Randomize2", 0.5f, 0.2f, true)
        };


        List<SpriteLoadInfo> spritesList = new List<SpriteLoadInfo>()
        {
            new SpriteLoadInfo("biggo_128x128", "biggo_128x128", new Point(128, 128), 2),
            new SpriteLoadInfo("enemies_16x16", "enemies_16x16", new Point(16, 16)),
            new SpriteLoadInfo("enemies_32x32", "enemies_32x32", new Point(32, 32)),
            new SpriteLoadInfo("enemies_8x8", "enemies_8x8", new Point(8, 8)),
            new SpriteLoadInfo("king_48x48", "king_48x48", new Point(48, 48), 14),
            new SpriteLoadInfo("player", "player", new Point(8, 16), 6)
            
        };

        public SplashScreenState()
        {
            loadDelegates.Add(LoadState.Init, Init);
            loadDelegates.Add(LoadState.Textures, LoadTextures);
            loadDelegates.Add(LoadState.Sounds, LoadSounds);
            loadDelegates.Add(LoadState.Music, LoadMusic);
            loadDelegates.Add(LoadState.Sprites, LoadSprites);
            loadDelegates.Add(LoadState.Register, RegisterStates);
            loadDelegates.Add(LoadState.Finish, Finish);
        }

        public override void Enter(int previousStateId)
        {
            logo = Globals.content.Load<Texture2D>("logo");

            //var titlex = Config.SCREEN_WIDTH_SCALED / 2 - Globals.silkscreenFont.MeasureString("LOADING").X / 2;
            //loading = new WavyText("LOADING", new Vector2(titlex, 130), 1500, 1.0f, Config.titleColors, 12.0f, 1.0f, 0.0f);
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
            spriteBatch.Draw(logo, position, null, Color.White, 0.0f, new Vector2(logo.Width / 2, logo.Height / 2), 0.5f, SpriteEffects.None, 0.0f);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool suspended)
        {
            loadDelegates[loadState]();
        }

        private void NextState()
        {
            loadState++;
            loadCounter = 0;
        }
        private void Init()
        {
            Globals.silkscreenFont = Globals.content.Load<SpriteFont>("Silkscreen");
            Globals.tileset = new Tileset("Levels\\fgame\\tileset");
            NextState();
        }

        private void LoadTextures()
        {
            if (loadCounter >= textureDictionary.Count)
                NextState();

            var texture = textureDictionary.ElementAt(loadCounter);
            if (!Globals.textureDictionary.ContainsKey(texture.Key))
                Globals.textureDictionary.Add(texture.Key, Globals.content.Load<Texture2D>(texture.Value));
            
            loadCounter++;
        }

        private void LoadSounds()
        {
            if (loadCounter >= soundsList.Count)
                NextState();
            
            var sound = soundsList[loadCounter];
            Globals.soundsDictionary.Add(sound.Key, Globals.content.Load<SoundEffect>(sound.Path).CreateInstance());
            Globals.soundsDictionary[sound.Key].Volume = sound.Volume;
            Globals.soundsDictionary[sound.Key].Pitch = sound.Pitch;
            Globals.soundsDictionary[sound.Key].IsLooped = sound.IsLooped;

            loadCounter++;
        }

        private void LoadMusic()
        {
            if (loadCounter >= musicList.Count)
                NextState();
            
            var music = musicList[loadCounter];
            Globals.backgroundMusicList.Add(Util.LoadSong(music));

            loadCounter++;
        }

        private void LoadSprites()
        {
            if (loadCounter >= spritesList.Count)
                NextState();

            var sprite = spritesList[loadCounter];
            Globals.spritesDictionary.Add(sprite.Key, sprite.Load());
            
            loadCounter++;
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

            NextState();
        }

        private void Finish()
        {
            Theme.ReloadTheme(World.LastActiveWorld(), scale);
            Globals.gameStateManager.PopState();
            Globals.gameStateManager.PushState(Config.States.BACKGROUND);
            Globals.gameStateManager.PushState(Config.States.DUMMY);
            Globals.gameStateManager.PushState(Config.States.TITLESCREEN);
        }

        #region CUTSCENES
        private void PrepareCutscenes()
        {
            PrepareFirstCutscene();
            PrepareLastCutscene();
        }

        private void PrepareFirstCutscene()
        {            
            CutsceneState cutsceneState = new CutsceneState();
            Scene scene = new Scene();
            scene._duration = TimeSpan.FromSeconds(32.0);
            #region BACKGROUND
            Image background = new Image();
            background.textureName = "cutscene1";
            Keyframe keyframe = new Keyframe();
            keyframe._time = TimeSpan.FromSeconds(0.0);
            keyframe.scale = 2.0f;
            background.keyframeList.Add(keyframe);
            Keyframe keyframe2 = new Keyframe(keyframe);
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
                    Image key = new Image();
                    key.textureName = "key";
                    Keyframe keykeyframe0 = new Keyframe(keyframe);
                    keykeyframe0.position = new Vector2(20 + j * 12 + i * 6, 122 - i * 16);
                    key.keyframeList.Add(keykeyframe0);

                    Keyframe keykeyframe1 = new Keyframe(keyframe2);
                    keykeyframe1.position = keykeyframe0.position;
                    keykeyframe1._time = keyframeTime[currentKey];
                    key.keyframeList.Add(keykeyframe1);

                    Keyframe keykeyframe2 = new Keyframe(keykeyframe1);
                    keykeyframe2.position = new Vector2(63, 94);
                    keykeyframe2._time = keyframeTime[currentKey] + TimeSpan.FromSeconds(0.5);
                    //if (currentKey == 11)
                    //    keykeyframe2._time += TimeSpan.FromSeconds(1.3);
                    key.keyframeList.Add(keykeyframe2);

                    if (currentKey == 11)
                    {
                        Keyframe keykeyframe2andhalf1 = new Keyframe(keykeyframe2);
                        keykeyframe2andhalf1._time += TimeSpan.FromSeconds(1.3);
                        key.keyframeList.Add(keykeyframe2andhalf1);

                        Keyframe keykeyframe2andhalf2 = new Keyframe(keykeyframe2andhalf1);
                        keykeyframe2andhalf2.position = new Vector2(73, 94);
                        keykeyframe2andhalf2._time += TimeSpan.FromSeconds(2.5);
                        key.keyframeList.Add(keykeyframe2andhalf2);
                    }
                    
                    Keyframe keykeyframe3 = new Keyframe(keykeyframe2);
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
            AnimatedImage pikpok = new AnimatedImage();
            pikpok.textureName = "enemies_16x16";
            pikpok.reverse = false;
            pikpok.frameCount = Config.ANIM_FRAMES;
            pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
            pikpok.startFrame = new Rectangle(0,0, 16, 16);

            Keyframe pikpok_keyframe1 = new Keyframe();
            pikpok_keyframe1._time = TimeSpan.FromSeconds(2.0);
            pikpok_keyframe1.position = new Vector2(280, 106);
            pikpok_keyframe1.scale = 2.0f;
            pikpok.keyframeList.Add(pikpok_keyframe1);

            Keyframe pikpok_keyframe2 = new Keyframe();
            pikpok_keyframe2.scale = 2.0f;
            pikpok_keyframe2.position = new Vector2(150, 106);
            pikpok_keyframe2._time = TimeSpan.FromSeconds(7.0);
            pikpok_keyframe2.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe2);

            Keyframe pikpok_keyframe3 = new Keyframe();
            pikpok_keyframe3.scale = 2.0f;
            pikpok_keyframe3.position = new Vector2(150, 106);
            pikpok_keyframe3._time = TimeSpan.FromSeconds(10.0);
            pikpok_keyframe3.animated = true;
            pikpok.keyframeList.Add(pikpok_keyframe3);

            Keyframe pikpok_keyframe4 = new Keyframe();
            pikpok_keyframe4.scale = 2.0f;
            pikpok_keyframe4.position = new Vector2(50, 106);
            pikpok_keyframe4._time = TimeSpan.FromSeconds(11.0);
            pikpok_keyframe4.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe4);

            Keyframe pikpok_keyframe5 = new Keyframe();
            pikpok_keyframe5.scale = 2.0f;
            pikpok_keyframe5.position = new Vector2(50, 106);
            pikpok_keyframe5._time = TimeSpan.FromSeconds(11.5);
            pikpok_keyframe5.animated = true;
            pikpok.keyframeList.Add(pikpok_keyframe5);

            Sound sound = new Sound();
            sound.soundName = "coin";
            sound._time = TimeSpan.FromSeconds(11.5);

            Keyframe pikpok_keyframe6 = new Keyframe();
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
                pikpok = new AnimatedImage();
                pikpok.textureName = "enemies_16x16";
                pikpok.reverse = false;
                pikpok.frameCount = Config.ANIM_FRAMES;
                pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
                pikpok.startFrame = new Rectangle(0, 0, 16, 16);

                pikpok_keyframe1 = new Keyframe();
                pikpok_keyframe1._time = TimeSpan.FromSeconds(13.5) + baseTimeSpan;
                pikpok_keyframe1.position = new Vector2(280, 106);
                pikpok_keyframe1.scale = 2.0f;
                pikpok.keyframeList.Add(pikpok_keyframe1);

                pikpok_keyframe4 = new Keyframe();
                pikpok_keyframe4.scale = 2.0f;
                pikpok_keyframe4.position = new Vector2(50, 106);
                pikpok_keyframe4._time = TimeSpan.FromSeconds(15.0) + baseTimeSpan;
                pikpok_keyframe4.animated = false;
                pikpok.keyframeList.Add(pikpok_keyframe4);

                pikpok_keyframe5 = new Keyframe();
                pikpok_keyframe5.scale = 2.0f;
                pikpok_keyframe5.position = new Vector2(50, 106);
                pikpok_keyframe5._time = TimeSpan.FromSeconds(15.5) + baseTimeSpan;
                pikpok_keyframe5.animated = true;
                pikpok.keyframeList.Add(pikpok_keyframe5);
                Sound sound2 = new Sound(sound);
                sound2.soundName = "coin";
                sound2._time = TimeSpan.FromSeconds(15.5) + baseTimeSpan;
                scene.sounds.Add(sound2);

                pikpok_keyframe6 = new Keyframe();
                pikpok_keyframe6.scale = 2.0f;
                pikpok_keyframe6.position = new Vector2(280, 106);
                pikpok_keyframe6._time = TimeSpan.FromSeconds(17.0) + baseTimeSpan;
                pikpok.keyframeList.Add(pikpok_keyframe6);

                scene.items.Add(pikpok);
            }
            #endregion
            #region LATE_PIKPOK
            baseTimeSpan = TimeSpan.FromSeconds(8.0);
            pikpok = new AnimatedImage();
            pikpok.textureName = "enemies_16x16";
            pikpok.reverse = false;
            pikpok.frameCount = Config.ANIM_FRAMES;
            pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
            pikpok.startFrame = new Rectangle(0, 0, 16, 16);

            pikpok_keyframe1 = new Keyframe();
            pikpok_keyframe1._time = TimeSpan.FromSeconds(13.5) + baseTimeSpan;
            pikpok_keyframe1.position = new Vector2(280, 106);
            pikpok_keyframe1.scale = 2.0f;
            pikpok.keyframeList.Add(pikpok_keyframe1);

            pikpok_keyframe4 = new Keyframe();
            pikpok_keyframe4.scale = 2.0f;
            pikpok_keyframe4.position = new Vector2(50, 106);
            pikpok_keyframe4._time = TimeSpan.FromSeconds(15.2) + baseTimeSpan;
            pikpok_keyframe4.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe4);

            pikpok_keyframe5 = new Keyframe();
            pikpok_keyframe5.scale = 2.0f;
            pikpok_keyframe5.position = new Vector2(50, 106);
            pikpok_keyframe5._time = TimeSpan.FromSeconds(17.0) + baseTimeSpan;
            pikpok_keyframe5.animated = false;
            pikpok.keyframeList.Add(pikpok_keyframe5);
            Sound sound3 = new Sound(sound);
            sound3.soundName = "coin";
            sound3._time = TimeSpan.FromSeconds(23.7);
            scene.sounds.Add(sound3);

            pikpok_keyframe6 = new Keyframe();
            pikpok_keyframe6.scale = 2.0f;
            pikpok_keyframe6.position = new Vector2(60, 106);
            pikpok_keyframe6._time = TimeSpan.FromSeconds(19.5) + baseTimeSpan;
            pikpok_keyframe6.animated = true;
            pikpok.keyframeList.Add(pikpok_keyframe6);

            var pikpok_keyframe7 = new Keyframe();
            pikpok_keyframe7.scale = 2.0f;
            pikpok_keyframe7.position = new Vector2(280, 106);
            pikpok_keyframe7._time = TimeSpan.FromSeconds(21.0) + baseTimeSpan;
            pikpok.keyframeList.Add(pikpok_keyframe7);

            scene.items.Add(pikpok);

            #endregion
            #region PLAYER
            Vector2 startVector = new Vector2(-15.0f, 106);
            Vector2 endVector = new Vector2(15.0f, 106);

            var playerItem = AnimatedImage.Player(startVector, endVector, TimeSpan.FromSeconds(22.0), 15.0f, 2.0f);
            Keyframe last = new Keyframe(playerItem.keyframeList[1]);
            last._time = TimeSpan.FromSeconds(30.0);

            scene.items.Add(playerItem);

            var playerItemShocked = AnimatedImage.Player(endVector, endVector, TimeSpan.FromSeconds(24), 5.0f, 2.0f);
            playerItemShocked.staticFrame = 1;
            playerItemShocked.startFrame = new Rectangle(24, 16, 8, 16);
            playerItemShocked.keyframeList[0].animated = false;
            playerItemShocked.keyframeList[1]._time = TimeSpan.FromSeconds(25.5);
            playerItemShocked.keyframeList[1].animated = false;
            Keyframe playerKeyframe0 = new Keyframe(playerItemShocked.keyframeList[1]);
            playerKeyframe0._time = TimeSpan.FromSeconds(27.5);
            playerKeyframe0.position.X += 10;
            playerItemShocked.keyframeList.Add(playerKeyframe0);
            scene.items.Add(playerItemShocked);

            var playerItemScared = AnimatedImage.Player(endVector + new Vector2(10, 0), endVector + new Vector2(10, 0), TimeSpan.FromSeconds(27.5), 10.0f, 2.0f);
            playerItemScared.startFrame = new Rectangle(32, 16, 8, 16);
            playerItemScared.keyframeList[1]._time += TimeSpan.FromSeconds(1);

            Keyframe playerKeyframe1 = new Keyframe(playerItemScared.keyframeList[1]);
            playerKeyframe1._time += TimeSpan.FromSeconds(1.5);
            playerKeyframe1.position.X += 240;
            playerItemScared.keyframeList.Add(playerKeyframe1);
            scene.items.Add(playerItemScared);

            #endregion

            #region QUESTIONMARK
            Image questionMark = new Image();
            questionMark.textureName = "question";
            Keyframe questionKeyframe0 = new Keyframe();
            questionKeyframe0.scale = 2.0f;
            questionKeyframe0._time = TimeSpan.FromSeconds(7.0);
            questionKeyframe0.position = new Vector2(160, 90);
            Keyframe questionKeyframe1 = new Keyframe(questionKeyframe0);
            questionKeyframe1._time = TimeSpan.FromSeconds(8.5);
            questionMark.keyframeList.Add(questionKeyframe0);
            questionMark.keyframeList.Add(questionKeyframe1);
            scene.items.Add(questionMark);
            #endregion
            #region EXCLAMATIONMARK
            Image exclamationMark = new Image();
            exclamationMark.textureName = "exclamation";
            Keyframe exclamationKeyframe0 = new Keyframe();
            exclamationKeyframe0.scale = 2.0f;
            exclamationKeyframe0._time = TimeSpan.FromSeconds(8.5);
            exclamationKeyframe0.position = new Vector2(160, 90);
            Keyframe exclamationKeyframe1 = new Keyframe(exclamationKeyframe0);
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
            String scene1Text = "so you've beat \nthe game...\n" +
                             "you're amazing!\n\n\n\n" +
                             "thank you!";
            String scene2Text = "thanks to all\n  my fellow\n betatesters!";
            String scene3Text = " thanks to stratkat\n    for preparing\n    amazing music\n\n\n" +
                                "   soundcloud.com/\n  daydreamanatomy";
            String scene4Text = "     and at last\n    thanks to my\n     lovely wife\n" +
                                " for supporting me";
            String scene5Text = "the end...";

            CutsceneState finalCutscene = new CutsceneState();
            finalCutscene.backroundMusic = 13;
            Scene scene1 = new Scene();
            finalCutscene.scenes.Add(scene1);
            scene1._duration = TimeSpan.FromSeconds(60.0);
            scene1.backgroundColor = Color.Black;
            scene1.gradientTexture = Util.GetGradientTexture(1, Config.SCREEN_HEIGHT_SCALED, Color.DarkSlateBlue, Color.Black, Util.GradientType.Horizontal);
            
            #region SCENE1

            const int PIKPOK_Y = 90;

            for (int i = 0; i < 5; i++)
            {               
                var baseTimeSpan = TimeSpan.FromSeconds(1.0);
                var pikpok = new AnimatedImage();
                pikpok.textureName = "enemies_16x16";
                pikpok.reverse = false;
                pikpok.frameCount = Config.ANIM_FRAMES;
                pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
                pikpok.startFrame = new Rectangle(0, 0, 16, 16);

                var pikpok_keyframe1 = new Keyframe();
                pikpok_keyframe1._time = baseTimeSpan;
                pikpok_keyframe1.position = new Vector2(280 + i * 25, PIKPOK_Y);
                pikpok_keyframe1.scale = 2.0f;
                pikpok.keyframeList.Add(pikpok_keyframe1);

                var pikpok_keyframe2 = new Keyframe(pikpok_keyframe1);
                pikpok_keyframe2.position = new Vector2(60 + i * 25, PIKPOK_Y);
                pikpok_keyframe2._time += TimeSpan.FromSeconds(1.5);
                pikpok.keyframeList.Add(pikpok_keyframe2);

                var pikpok_keyframe3 = new Keyframe(pikpok_keyframe2);
                pikpok_keyframe3._time += TimeSpan.FromSeconds(1.5);
                pikpok.keyframeList.Add(pikpok_keyframe3);

                var pikpok_keyframe4 = new Keyframe(pikpok_keyframe3);
                pikpok_keyframe4.position = new Vector2(-160 + i * 25, PIKPOK_Y);
                pikpok_keyframe4._time += TimeSpan.FromSeconds(1.5);
                pikpok.keyframeList.Add(pikpok_keyframe4);

                scene1.items.Add(pikpok);

                Image key = new Image();
                key.textureName = "key";
                Vector2 shiftVector = new Vector2(13, -12);

                var key_keyframe1 = new Keyframe(pikpok_keyframe1);
                key_keyframe1.position += shiftVector;
                key.keyframeList.Add(key_keyframe1);

                var key_keyframe2 = new Keyframe(pikpok_keyframe2);
                key_keyframe2.position += shiftVector;
                key.keyframeList.Add(key_keyframe2);

                var key_keyframe3 = new Keyframe(pikpok_keyframe3);
                key_keyframe3.position += shiftVector;
                key.keyframeList.Add(key_keyframe3);

                var key_keyframe4 = new Keyframe(pikpok_keyframe4);
                key_keyframe4.position += shiftVector;
                key.keyframeList.Add(key_keyframe4);

                scene1.items.Add(key);
            }

            var playerBaseTimeSpan = TimeSpan.FromSeconds(5.0);
            var player = new AnimatedImage();
            player.frameCount = Config.Player.ANIM_FRAMES;
            player.frameTime = Config.Player.ANIMATION_DELAY;
            player.reverse = true;
            player.startFrame = new Rectangle(32, Config.Player.HEIGHT, Config.Player.WIDTH * 1, Config.Player.HEIGHT);
            player.textureName = "player";

            var player_keyframe1 = new Keyframe();
            player_keyframe1._time = playerBaseTimeSpan;
            player_keyframe1.position = new Vector2(350, PIKPOK_Y);
            player_keyframe1.scale = 2.0f;
            player.keyframeList.Add(player_keyframe1);

            var player_keyframe2 = new Keyframe(player_keyframe1);
            player_keyframe2.position = new Vector2(130, PIKPOK_Y);
            player_keyframe2._time += TimeSpan.FromSeconds(1.5);
            player.keyframeList.Add(player_keyframe2);

            var player_keyframe3 = new Keyframe(player_keyframe2);
            player_keyframe3._time += TimeSpan.FromSeconds(1.5);
            player.keyframeList.Add(player_keyframe3);

            var player_keyframe4 = new Keyframe(player_keyframe3);
            player_keyframe4.position = new Vector2(-90, PIKPOK_Y);
            player_keyframe4._time += TimeSpan.FromSeconds(1.5);
            player.keyframeList.Add(player_keyframe4);

            scene1.items.Add(player);

            #endregion
            
            #region SCENE2
            
            for (int i = 0; i < 6; i++)
            {                
                var baseTimeSpan = TimeSpan.FromSeconds(14.0);
                var monster = new AnimatedImage();
                monster.textureName = "enemies_16x16";
                monster.reverse = false;
                monster.frameCount = Config.ANIM_FRAMES;
                monster.frameTime = Config.DEFAULT_ANIMATION_SPEED;
                monster.startFrame = new Rectangle(i * 2 * 16, 0, 16, 16);

                var monster_keyframe1 = new Keyframe();
                monster_keyframe1._time = baseTimeSpan;
                monster_keyframe1.position = new Vector2(20, 200 + i * 20);
                monster_keyframe1.scale = 1.0f;
                monster.keyframeList.Add(monster_keyframe1);

                var monster_keyframe2 = new Keyframe(monster_keyframe1);
                monster_keyframe2.position = new Vector2(20, - 120 + i * 20);
                monster_keyframe2._time += TimeSpan.FromSeconds(10.0);
                monster.keyframeList.Add(monster_keyframe2);

                scene1.items.Add(monster);

                monster = new AnimatedImage();
                monster.textureName = "enemies_16x16";
                monster.reverse = false;
                monster.frameCount = Config.ANIM_FRAMES;
                monster.frameTime = Config.DEFAULT_ANIMATION_SPEED;
                monster.startFrame = new Rectangle((i * 2 + 1) * 16, 0, 16, 16);

                monster_keyframe1 = new Keyframe();
                monster_keyframe1._time = baseTimeSpan;
                monster_keyframe1.position = new Vector2(240, -20 - i * 20);
                monster_keyframe1.scale = 1.0f;
                monster.keyframeList.Add(monster_keyframe1);

                monster_keyframe2 = new Keyframe(monster_keyframe1);
                monster_keyframe2.position = new Vector2(240, 300 - i * 20);
                monster_keyframe2._time += TimeSpan.FromSeconds(10.0);
                monster.keyframeList.Add(monster_keyframe2);

                scene1.items.Add(monster);
            }
            #endregion

            #region SCENE3
            for (int i = 0; i < 18; i++)
            {
                var baseTimeSpan = TimeSpan.FromSeconds(25.0);
                var monster = new AnimatedImage();
                monster.textureName = "enemies_32x32";
                monster.reverse = false;
                monster.frameCount = Config.ANIM_FRAMES;
                monster.frameTime = Config.DEFAULT_ANIMATION_SPEED;
                monster.startFrame = new Rectangle(i * 32, 0, 32, 32);

                var monster_keyframe1 = new Keyframe();
                monster_keyframe1._time = baseTimeSpan;
                monster_keyframe1.spriteEffect = SpriteEffects.FlipHorizontally;
                monster_keyframe1.position = new Vector2(-32 - 36 * i, 70);
                monster_keyframe1.scale = 1.0f;
                monster.keyframeList.Add(monster_keyframe1);

                var monster_keyframe2 = new Keyframe(monster_keyframe1);
                monster_keyframe2.position = new Vector2(880 - i * 36, 70);
                monster_keyframe2._time += TimeSpan.FromSeconds(10.0);
                monster.keyframeList.Add(monster_keyframe2);

                scene1.items.Add(monster);
            }      
      
            #endregion

            #region SCENE4
            scene1.items.Add(Cutscene.AnimatedImage.Player(new Vector2(Config.SCREEN_WIDTH_SCALED + 16, 8),
                                                           new Vector2(-16, 140), TimeSpan.FromSeconds(38.0),
                                                           50.0f, 1.0f, 3));
            var bigpok = new AnimatedImage();
            bigpok.textureName = "enemies_16x16";
            bigpok.reverse = false;
            bigpok.frameCount = Config.ANIM_FRAMES;
            bigpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
            bigpok.startFrame = new Rectangle(16, 0, 16, 16);

            var bikpok_keyframe1 = new Keyframe();
            bikpok_keyframe1._time = TimeSpan.FromSeconds(41.0);
            bikpok_keyframe1.position = new Vector2(Config.SCREEN_WIDTH_SCALED + 32, -8);
            bikpok_keyframe1.scale = 3.0f;
            bigpok.keyframeList.Add(bikpok_keyframe1);

            var bikpok_keyframe2 = new Keyframe(bikpok_keyframe1);
            bikpok_keyframe2.position = new Vector2(-32, 110);
            bikpok_keyframe2._time += TimeSpan.FromSeconds(4.5);
            bigpok.keyframeList.Add(bikpok_keyframe2);

            scene1.items.Add(bigpok);
            #endregion

            #region SCENE5
            scene1.items.Add(Cutscene.AnimatedImage.Player(new Vector2(-8, Config.SCREEN_HEIGHT_SCALED/2),
                                               new Vector2(Config.SCREEN_WIDTH_SCALED + 38, Config.SCREEN_HEIGHT_SCALED / 2), TimeSpan.FromSeconds(47.0),
                                               30.0f, 1.0f, 0));

            for (int i = 0; i < 25; i++)
            {
                Image key = new Image();
                key.textureName = "key";

                Vector2 randomVector = new Vector2(Globals.rnd.Next(-15, 15), Globals.rnd.Next(-15, 15));
                var key_keyframe1 = new Keyframe();
                key_keyframe1.position = new Vector2(-38, Config.SCREEN_HEIGHT_SCALED / 2) + randomVector;
                key_keyframe1._time = TimeSpan.FromSeconds(47.0);
                key.keyframeList.Add(key_keyframe1);

                var key_keyframe2 = new Keyframe(key_keyframe1);
                key_keyframe2.position = new Vector2(Config.SCREEN_WIDTH_SCALED + 15, Config.SCREEN_HEIGHT_SCALED / 2) + randomVector;
                key_keyframe2._time += TimeSpan.FromSeconds(11.0);
                key.keyframeList.Add(key_keyframe2);

                scene1.items.Add(key);                
            }

            var king = new AnimatedImage();
            king.textureName = "king_48x48";
            king.reverse = true;
            king.frameCount = 2;
            king.frameTime = Config.DEFAULT_ANIMATION_SPEED * 2;
            king.startFrame = new Rectangle(0, 1 * 48, 48, 48);

            var king_keyframe1 = new Keyframe();
            king_keyframe1._time = TimeSpan.FromSeconds(53.0);
            king_keyframe1.position = new Vector2(-48, Config.SCREEN_HEIGHT_SCALED / 2 - 24);
            king_keyframe1.scale = 1.0f;
            king.keyframeList.Add(king_keyframe1);

            var king_keyframe2 = new Keyframe(king_keyframe1);
            king_keyframe2.position = new Vector2(Config.SCREEN_WIDTH_SCALED + 48, Config.SCREEN_HEIGHT_SCALED / 2 - 24);
            king_keyframe2._time += TimeSpan.FromSeconds(7.0);
            king.keyframeList.Add(king_keyframe2);

            scene1.items.Add(king);

            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var pikpok = new AnimatedImage();
                    pikpok.textureName = "enemies_16x16";
                    pikpok.reverse = false;
                    pikpok.frameCount = Config.ANIM_FRAMES;
                    pikpok.frameTime = Config.DEFAULT_ANIMATION_SPEED;
                    pikpok.startFrame = new Rectangle((Globals.rnd.Next(2) == 1 ? 0 : 16), 0, 16, 16);

                    var pikpok_keyframe1 = new Keyframe();
                    pikpok_keyframe1._time = TimeSpan.FromSeconds(53.0);
                    pikpok_keyframe1.position = new Vector2(-66 - i * 17, -8 * (i % 2) + j * 17); 
                    pikpok_keyframe1.scale = 1.0f;
                    pikpok.keyframeList.Add(pikpok_keyframe1);

                    var pikpok_keyframe2 = new Keyframe(pikpok_keyframe1);
                    pikpok_keyframe2.position = new Vector2(Config.SCREEN_WIDTH_SCALED + 30 - i * 17, -8 * (i % 2) + j * 17);
                    pikpok_keyframe2._time += TimeSpan.FromSeconds(7.0);
                    pikpok.keyframeList.Add(pikpok_keyframe2);

                    scene1.items.Add(pikpok);
                }
            }
            #endregion

            scene1.items.Add(GenerateFinalText(scene1Text, Color.White, TimeSpan.Zero, new Vector2(12, 10)));
            scene1.items.Add(GenerateFinalText(scene2Text, Color.White, TimeSpan.FromSeconds(12.0), new Vector2(60, 40)));
            scene1.items.Add(GenerateFinalText(scene3Text, Color.White, TimeSpan.FromSeconds(24.0), new Vector2(20, 10)));
            scene1.items.Add(GenerateFinalText(scene4Text, Color.White, TimeSpan.FromSeconds(36.0), new Vector2(30, 30)));
            scene1.items.Add(GenerateFinalText(scene5Text, Color.White, TimeSpan.FromSeconds(48.0), new Vector2(50, 40)));

            scene1.Init();
            Globals.gameStateManager.RegisterState(Config.States.FINAL_CUTSCENE, finalCutscene);
        }
        
        private Text GenerateFinalText(String text, Color color, TimeSpan shift, Vector2 position)
        {
            var sceneText = new Text();
            sceneText.text = text;
            
            var textKeyframe1 = new Keyframe();
            textKeyframe1._time = shift;
            textKeyframe1.position = position;
            textKeyframe1.color = Color.Transparent;
            textKeyframe1.scale = 2.0f;
            textKeyframe1.printedLetters = text.Length;
            sceneText.keyframeList.Add(textKeyframe1);

            var textKeyframe2 = new Keyframe(textKeyframe1);
            textKeyframe2._time += TimeSpan.FromSeconds(4.0);
            textKeyframe2.color = color;
            sceneText.keyframeList.Add(textKeyframe2);

            var textKeyframe3 = new Keyframe(textKeyframe2);
            textKeyframe3._time += TimeSpan.FromSeconds(6.0);
            textKeyframe3.color = color;
            sceneText.keyframeList.Add(textKeyframe3);

            var textKeyframe4 = new Keyframe(textKeyframe3);
            textKeyframe4._time += TimeSpan.FromSeconds(4.0);
            textKeyframe4.color = Color.Transparent;
            sceneText.keyframeList.Add(textKeyframe4);

            return sceneText;
        }
        #endregion
    }

    class SoundLoadInfo
    {
        public string Key { get; private set; }
        public string Path { get; private set; }
        public float Volume { get; private set; }
        public float Pitch { get; private set; }
        public bool IsLooped { get; private set; }

        public SoundLoadInfo(string key, string path, float pitch = 0.0f, float volume = 0.15f, bool isLooped = false)
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Key and Path cannot be null or empty string");

            Key = key;
            Path = path;
            Volume = volume;
            Pitch = pitch;
            IsLooped = isLooped;
        }
    }

    class SpriteLoadInfo
    {
        public string Key { get; private set; }
        public string Path { get; private set; }
        public Point Size { get; private set; }
        public int FramesNumber { get; private set; }

        private Sprite _sprite;

        public SpriteLoadInfo(string key, string path, Point size, int framesNumber = Config.ANIM_FRAMES) 
        {
            if (String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Key and Path cannot be null or empty string");
            
            if (framesNumber <= 0)
                throw new ArgumentException("Frames number must be positive value");

            Key = key;
            Path = path;
            Size = size;
            FramesNumber = framesNumber;
        }

        public Sprite Load()
        {
            if (_sprite == null)
                _sprite = new Sprite(Path, Size.X, Size.Y, FramesNumber);
            
            return _sprite;
        }

    }
}
