using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.Xml;
using ProtoBuf;

namespace PixelPerfect
{
    [ProtoContract]
    public class Levelsave
    {
        [ProtoMember(1)]
        public bool completed = false;
        [ProtoMember(2)]
        public int deathCount = 0;
        [ProtoMember(3)]
        public TimeSpan bestTime = TimeSpan.Zero;
        [ProtoMember(4)]
#if DEVELOPER_VERSION
        public bool active = true;
#else
        public bool active = false;
#endif
        [ProtoMember(5)]
        public bool skipped = false;
        [ProtoMember(6)]
        public int completeDeathCount = 0;
    }
    [ProtoContract]
    public sealed class SavestateV4

    {
        public const int CURRENT_VERSION = 4;

        [ProtoMember(1)]
        public int version = CURRENT_VERSION;

        public static Savestate Instance { get; private set; }

        [ProtoMember(2)]
        public Dictionary<String, Levelsave> levelSaves;

        [ProtoMember(3, OverwriteList = true)]
        public bool[] suitUnlocked;

        private SavestateV4()
        {
            levelSaves = new Dictionary<string, Levelsave>();
        }
    }

    [ProtoContract]
    public sealed class Savestate
    {
        public const int CURRENT_VERSION = 5;

        [ProtoMember(1)]
        public int version = CURRENT_VERSION;
        
        public static Savestate Instance { get; private set; } 
        
        [ProtoMember(2)]
        public Dictionary<String, Levelsave> levelSaves;

        [ProtoMember(3, OverwriteList=true)]
        public bool[] suitUnlocked; 

        public Savestate() 
        {
            levelSaves = new Dictionary<string, Levelsave>();
            suitUnlocked = new bool[Config.Player.SUIT_QTY];
            suitUnlocked[0] = true;
#if DEVELOPER_VERSION
            for (int i = 0; i < suitUnlocked.Length; i++)
                suitUnlocked[i] = true;
#endif
        }

        private Savestate ConvertFromV4()
        {
            var newSaveState = new Savestate();
            newSaveState.levelSaves = new Dictionary<string,Levelsave>(levelSaves);

#if !DEBUG
            // randomize suits for achieved trophies
            int rndNumber = 0;

            for (int i = 0; i < (World.BeatPerfektTimeCount() / 3); i++)
            {
                do
                {
                    rndNumber = Globals.rnd.Next(Config.Player.SUIT_QTY - 1);
                }
                while (newSaveState.suitUnlocked[rndNumber]);
                newSaveState.suitUnlocked[rndNumber] = true;
            }
#endif
            return newSaveState;
        }

        public static void Reset()
        {
            Instance = new Savestate();            
            Instance.Save(true);
        }

        public static bool Init() // Savestate class first need to be initialized, if file exists loaded, if not new created and saved
        {
#if !WINDOWS
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (storage.FileExists(Config.SAVEFILE_NAME)) // deserialize
                    {
                        using (IsolatedStorageFileStream savefile = storage.OpenFile(Config.SAVEFILE_NAME, System.IO.FileMode.Open))
                        {
                            Instance = Serializer.Deserialize<Savestate>(savefile);
                            if (Instance.version != CURRENT_VERSION)
                            {
                                if (Instance.version == 4)
                                    Instance = Instance.ConvertFromV4();
                                else
                                    CreateSavestate();
                            }
                        }
                    }
                    else
                    {
                        CreateSavestate();
                    }
                }
            }
            catch
            {
                CreateSavestate();
            }
#else
            Instance = new Savestate();
#endif
            return true;   
        }
        
        private static void CreateSavestate()
        {
            Instance = new Savestate();
            Instance.Save(true);
        }

        public bool Reload()
        {
#if !WINDOWS
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream savefile = storage.OpenFile(Config.SAVEFILE_NAME, System.IO.FileMode.Open))    
                        Instance = Serializer.Deserialize<Savestate>(savefile);
                }
            }
            catch 
            {
                return false;
            }
#endif
            return true;
        }

        public bool Save(bool createNew = false)
        {
#if !WINDOWS
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream savefile = storage.OpenFile(Config.SAVEFILE_NAME, (createNew ? System.IO.FileMode.Create : System.IO.FileMode.OpenOrCreate)))                    
                        Serializer.Serialize<Savestate>(savefile, Instance);
                }
            }
            catch 
            {
                return false;
            }
#endif
            return true;
        }

        public bool Skipped()
        {
            int skippedLevels = 0;

            foreach (Levelsave levelsave in levelSaves.Values)
            {
                if (levelsave.skipped)
                {
                    skippedLevels++;
                    if (skippedLevels >= Config.SKIP_AMOUNT)
                        return true;
                }                 
            }
            return false;
        }

        public int SkipUsed()
        {
            int skippedLevels = 0;

            foreach (Levelsave levelsave in levelSaves.Values)
            {
                if (levelsave.skipped)
                {
                    skippedLevels++;
                }
            }
            return skippedLevels;
        }

        public int SkipLeft()
        {
            return Config.SKIP_AMOUNT - SkipUsed();
        }
    }
}
