using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
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
        private string icon = "";
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

            return directory + "\\" + levels[id].levelName;
        }

        public void AddLevel(string levelName)
        {
            Texture2D thumbnail = null;
            try
            {
                thumbnail = Globals.content.Load<Texture2D>(directory + "\\" + levelName);
            }
            catch (Exception ex)
            {
                // do nothing if file exists
            }

            levels.Add(new Level(levelName, thumbnail));
        }

        public bool LevelActivated(int id)
        {
            if (Config.CHEAT_MODE)
                return true;

            if (id < 0 || id > levels.Count - 1)
                return false;
            
            if (id == 0) // 0 level always activated
                return true;
          
            return (LevelCompleted(id - 1) || LevelSkipped(id - 1)); // if previous level is completed this is activated
        }

        public bool LevelCompleted(int id)
        {
            Levelsave levelsave;

            if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + levels[id].levelName, out levelsave))
                return false;

            return levelsave.completed;
        }

        public bool LevelSkipped(int id)
        {
            Levelsave levelsave;

            if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + levels[id].levelName, out levelsave))
                return false;

            return levelsave.skipped;
        }

        public bool Completed()
        {
            Levelsave levelSave;

            foreach (Level level in levels)
            {
                if (!Savestate.Instance.levelSaves.TryGetValue(directory + "\\" + level.levelName, out levelSave))
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
                                xmlreader.MoveToNextAttribute();
                                if (xmlreader.Name == "name")
                                    tempworld.AddLevel(xmlreader.Value);
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

            worlds[0].active = true;
            
            for (int i = 1; i < worlds.Count; i++)                
            {                
                if (!Config.CHEAT_MODE && !worlds[i - 1].Completed())
                    break;
                worlds[i].active = true;
            }

            foreach (World world in worlds)
                world.PrepareSavestate();

            return worlds;
        }



        internal bool Skip(int selectedLevel)
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
    }
}
