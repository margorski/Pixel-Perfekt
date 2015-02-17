using System;
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
        public int version = 1;
        [ProtoMember(2)]
        public bool completed = false;
        [ProtoMember(3)]
        public int deathCount = 0;
        [ProtoMember(4)]
        public double bestTime = 0.0;
        [ProtoMember(5)]
        public bool active = false;
        [ProtoMember(6)]
        public bool skipped = false;
    }

    [ProtoContract]
    public sealed class Savestate
    {
        public static Savestate Instance { get; private set; } 
        
        [ProtoMember(1)]
        public Dictionary<String, Levelsave> levelSaves;

        private Savestate() 
        {
            levelSaves = new Dictionary<string, Levelsave>();            
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
                    }
                    else
                    {
                        Instance = new Savestate();
                        Instance.Save();
                    }
                }
            }
            catch
            {
                Instance = null;
                return false;
            }
#else
            Instance = new Savestate();
#endif
            return true;   
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
    }
}
