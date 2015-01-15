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
        public List<string> levelNames = new List<string>();
        private string icon = "";
        public string directory = "";

        public World(string name, string icon, string directory)
        {
            this.name = name;
            this.icon = icon;
            this.directory = directory;
        }

        public string GetLevelFile(int id)
        {
            if (id < 0 || id > levelNames.Count - 1)
                return "";

            return directory + "\\" + levelNames[id];
        }

        public void AddLevel(string levelName)
        {
            levelNames.Add(levelName);
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

        public static List<World> LoadWorlds()
        {
            return LoadWorlds("worlds.xml");
        }
    }
}
