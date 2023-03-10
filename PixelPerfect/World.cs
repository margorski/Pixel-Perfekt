using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class World
    {
        public string name = "";
        public List<Level> levels = new List<Level>();
        public string icon = "";
        public string directory = "";
        public bool active { private set; get; }

        public World(string name, string icon, string directory)
        {
            this.name = name;
            this.icon = icon;
            this.directory = directory;
            active = false;
        }

        public string GetLevelFile(int id)
        {
            if (id < 0 || id > levels.Count - 1)
                return "";

            return directory + "\\" + levels[id].shortName;
        }

        public void AddLevel(string name, string levelName, TimeSpan time)
        {
            Texture2D thumbnail = null;
            //try
            //{
            //    thumbnail = Globals.content.Load<Texture2D>(directory + "\\" + levelName);
            //}
            //catch (Exception ex)
            //{
            //    // do nothing if file exists
            //}

            levels.Add(new Level(name, levelName, thumbnail, time));
        }

        public bool LevelActivated(int id)
        {
#if !DEBUG || !DEVELOPER_VERSION 
            if (id < 0 || id > levels.Count - 1)
                return false;
            
            if (id == 0) // 0 level always activated
                return true;
          
            return (LevelCompleted(id - 1) || LevelSkipped(id - 1)); // if previous level is completed this is activated
#else
            return true;

#endif
        }

        public bool BeatWorldPerfektTime()
        {
            bool value = true;
            for (int i = 0; i < levels.Count; i++)
            {
                if (!BeatLevelPerfektTime(i))
                {
                    value = false;
                    break;
                }
            }
            return value;
        }

        public bool BeatLevelPerfektTime(int id, TimeSpan levelTime)
        {
            return levelTime <= this.levels[id].time;
        }

        public bool BeatLevelPerfektTime(int id)
        {
            Levelsave levelsave;

            if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + levels[id].shortName, out levelsave))
                return false;

            if (levelsave.bestTime == TimeSpan.Zero)
                return false;

            return levelsave.bestTime <= this.levels[id].time;
        }

        public TimeSpan DiffPreviousTime(int id, TimeSpan levelTime)
        {
            Levelsave levelsave;

            if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + levels[id].shortName, out levelsave))
                return -levelTime;

            if (levelsave.bestTime == TimeSpan.Zero)
                return -levelTime;

            return levelTime - levelsave.bestTime;
        }

        public bool LevelCompleted(int id)
        {
            Levelsave levelsave;

            if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + levels[id].shortName, out levelsave))
                return false;

            return levelsave.completed;
        }

        public bool LevelSkipped(int id)
        {
            Levelsave levelsave;

            if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + levels[id].shortName, out levelsave))
                return false;

            return levelsave.skipped;
        }

        public bool Completed()
        {
            Levelsave levelSave;

            foreach (Level level in levels)
            {
                if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + level.shortName, out levelSave))
                    return false;

                if (!levelSave.completed)
                    return false;
            }
            return true;
        }

        public static List<World> LoadWorlds(string worldFile)
        {
            List<World> worlds = new List<World>();
            World tempworld;
            string icon, name, directory;
            TimeSpan time = TimeSpan.Zero;

            using (XmlReader xmlreader = XmlReader.Create(TitleContainer.OpenStream(@"Levels\" + worldFile)))
            {
                while (xmlreader.Read())
                {
                    if (xmlreader.NodeType == XmlNodeType.Element && xmlreader.Name == "world")
                    {
                        name = icon = directory = "";
                        xmlreader.MoveToNextAttribute();
                        if (xmlreader.Name == "name")
                        {
                            name = xmlreader.Value;
                            xmlreader.MoveToNextAttribute();
                        }
                        if (xmlreader.Name == "icon")
                        {
                            icon = xmlreader.Value;
                            xmlreader.MoveToNextAttribute();
                        }
                        if (xmlreader.Name == "directory")
                        {
                            directory = xmlreader.Value;
                            xmlreader.MoveToNextAttribute();
                        }
                        tempworld = new World(name, icon, directory);

                        while (xmlreader.Read())
                        {
                            if (xmlreader.NodeType == XmlNodeType.EndElement && xmlreader.Name == "world")
                            {
                                worlds.Add(tempworld);
                                break;
                            }
                            if (xmlreader.NodeType == XmlNodeType.Element && xmlreader.Name == "level")
                            {
                                string sname, levelname;
                                sname = levelname = "";
                                xmlreader.MoveToNextAttribute();
                                if (xmlreader.Name == "name")
                                {
                                    sname = xmlreader.Value;
                                    xmlreader.MoveToNextAttribute();
                                }
                                if (xmlreader.Name == "time")
                                {
                                    time = TimeSpan.Parse(xmlreader.Value, System.Globalization.CultureInfo.InvariantCulture);
                                    xmlreader.MoveToNextAttribute();
                                }
                                if (xmlreader.Name == "value")
                                {
                                    levelname = xmlreader.Value;
                                    xmlreader.MoveToNextAttribute();
                                }
                                tempworld.AddLevel(sname, levelname, time);
                            }
                        }
                    }
                }
            }        
            return worlds;
        }

        public void PrepareSavestate()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (!Savestate.Instance.levelSaves.ContainsKey(GetLevelFile(i)))
                    Savestate.Instance.levelSaves.Add(GetLevelFile(i), new Levelsave());
            }
        }

        public static List<World> LoadWorlds()
        {
            var worlds = LoadWorlds("worlds.xml");

            RefreshWorldStatus(worlds);

            foreach (World world in worlds)
                world.PrepareSavestate();
            Savestate.Instance.Save();

            return worlds;
        }

        public static void RefreshWorldStatus(List<World> worlds)
        {
            worlds[0].active = true;

            for (int i = 1; i < worlds.Count; i++)
            {
#if !DEBUG
                if (!worlds[i - 1].Completed())
                    break;
#endif
                worlds[i].active = true;
            }
        }

        public bool Skip(int selectedLevel)
        {
            if (selectedLevel >= levels.Count - 1)
                return false; // cannot skip last level

            if (Savestate.Instance.Skipped()) // skip used
                return false;

            Levelsave levelsave;
            Savestate.Instance.levelSaves.TryGetValue(GetLevelFile(selectedLevel), out levelsave);

            if (levelsave.skipped)// level already skipped  
                return false;

            if (levelsave.completed) // cannot skip completed levels
                return false;
            
            levelsave.skipped = true;
            return true;
        }

        public static int LastActiveWorld()
        {
            return Globals.worlds.FindLastIndex(world => world.active);
        }
        
        public static int BeatPerfektTimeCount()
        {
            int count = 0;

            foreach (World world in Globals.worlds)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (world.BeatLevelPerfektTime(i))
                        count++;
                }
            }
            return count;
        }
    }
}
