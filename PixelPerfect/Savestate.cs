﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
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
        public bool active = false;
        [ProtoMember(5)]
        public bool skipped = false;
    }

    [ProtoContract]
    public sealed class Savestate
    {
        public const int CURRENT_VERSION = 2;

        [ProtoMember(1)]
        public int version = CURRENT_VERSION;

        public static Savestate Instance { get; private set; } 
        
        [ProtoMember(2)]
        public Dictionary<String, Levelsave> levelSaves;

        private Savestate() 
        {
            levelSaves = new Dictionary<string, Levelsave>();            
        }

        public static void Reset()
        {
            Instance = new Savestate();            
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
                        var savefile = storage.OpenFile(Config.SAVEFILE_NAME, System.IO.FileMode.Open);
                        Instance = Serializer.Deserialize<Savestate>(savefile);
                        if (Instance.version != CURRENT_VERSION)
                            CreateSavestate();
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
            Instance.Save();
        }

        public bool Reload()
        {
#if !WINDOWS
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var savefile = storage.OpenFile(Config.SAVEFILE_NAME, System.IO.FileMode.Open);
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

        public bool Save()
        {
#if !WINDOWS
            try
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var savefile = storage.OpenFile(Config.SAVEFILE_NAME, System.IO.FileMode.OpenOrCreate);
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
            foreach (Levelsave levelsave in levelSaves.Values)
            {
                if (levelsave.skipped)
                    return true;
            }
            return false;
        }

    }
}
