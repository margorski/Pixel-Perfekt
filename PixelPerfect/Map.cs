﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Globalization;
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
    class Map
    {
        // Privates
        private Rectangle tileRect = new Rectangle(0, 0, Config.Tile.SIZE, Config.Tile.SIZE);
        private int[] mapa;
        private Tile[] tileMapa;
        private Texture2D tileset;
        private List<Enemy> enemiesList = new List<Enemy>();
        private List<Emiter> emiterList = new List<Emiter>();
        private List<Trigger> triggerList = new List<Trigger>();
        private Vector2 mapOffset = Vector2.Zero;
        public bool upsidedown { get; private set; }
        public bool moving { get; private set; }
        // Public
        public Vector2 startPosition { private set; get; }
        public byte collectiblesCount { private set;  get; }
        public string levelName { private set; get; }
        
        // Methods
        public Map(Texture2D tileset, Texture2D pixel, int[] mapa, List<Enemy> enemiesList, List<Emiter> emiterList, List<Trigger> triggerList, string levelName, bool upsidedown = false, bool moving = false)
        {
            this.mapa = mapa;
            this.enemiesList = enemiesList;
            this.emiterList = emiterList;
            this.triggerList = triggerList;
            this.levelName = levelName;
            this.upsidedown = upsidedown;
            this.moving = moving;

            tileMapa = new Tile[Config.Map.HEIGHT * Config.Map.WIDTH];
            this.tileset = tileset;

            Vector2 tilePosition = Vector2.Zero;
            TileFactory.Init(tileset, pixel);
            for (int i = 0; i < mapa.Length; i++)
            {
                tilePosition.X = (float)((i % Config.Map.WIDTH) * Config.Tile.SIZE);
                tilePosition.Y = (float)((i / Config.Map.WIDTH) * Config.Tile.SIZE);
   
                tileMapa[i] = TileFactory.CreateTile(mapa[i], tilePosition);

                if (mapa[i] == (byte)Config.TileType.KEY)
                    collectiblesCount++;
                else if (mapa[i] == (byte)Config.TileType.START_POSITION)
                    startPosition = tilePosition;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (moving)
            {
                mapOffset.X += (float)(gameTime.ElapsedGameTime.TotalSeconds * Config.Map.MOVING_MAP_SPEED);
                if (mapOffset.X <= -Config.Tile.SIZE * Config.Map.WIDTH)
                    mapOffset.X = 0.0f;
            }


            foreach (Tile tile in tileMapa)
                tile.Update(gameTime);
            foreach (Enemy enemy in enemiesList)
            {
                enemy.Update(gameTime);
            }
            foreach (Emiter emiter in emiterList)
                emiter.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Tile tile in tileMapa)
            {
                tile.Draw(spriteBatch, mapOffset);
                if (moving) // draw 2x tiles for moving map
                {
                    var secondMapOffset = mapOffset + new Vector2(Config.Tile.SIZE * Config.Map.WIDTH, 0.0f);
                    tile.Draw(spriteBatch, secondMapOffset);
                }
            }
            foreach (Enemy enemy in enemiesList)
                enemy.Draw(spriteBatch, Vector2.Zero);
            foreach (Emiter emiter in emiterList)
                emiter.Draw(spriteBatch, Vector2.Zero);
        }

        public void CheckTriggers(Rectangle playerRect)
        {
            foreach (Trigger trigger in triggerList)
            {
                trigger.CheckTrigger(playerRect);
            }
        }

        public bool CheckCollisions(Rectangle boundingBox, UInt32 attributes)
        {
             List<Rectangle> boundingBoxes = new List<Rectangle>();          
            boundingBoxes.Add(new Rectangle(boundingBox.X - (int)mapOffset.X, boundingBox.Y - (int)mapOffset.Y,
                                            boundingBox.Width, boundingBox.Height));
            if (moving)
                boundingBoxes.Add(new Rectangle(boundingBox.X - (int)(mapOffset.X + Config.Tile.SIZE * Config.Map.WIDTH), boundingBox.Y - (int)mapOffset.Y,
                                            boundingBox.Width, boundingBox.Height));

            foreach (Rectangle playerbbox in boundingBoxes)
            {
                int startRow = playerbbox.Top / Config.Tile.SIZE;
                int endRow = (playerbbox.Bottom - 1) / Config.Tile.SIZE;
                int startColumn = playerbbox.Left / Config.Tile.SIZE;
                int endColumn = (playerbbox.Right - 1) / Config.Tile.SIZE;

                for (int i = startRow; i <= endRow; i++)
                {
                    for (int j = startColumn; j <= endColumn; j++)
                    {
                        int index = i * Config.Map.WIDTH + j;
                        if (index > tileMapa.Length - 1 || index < 0)
                            continue;

                        if ((tileMapa[i * Config.Map.WIDTH + j].attributes & attributes) > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool CheckCollisions(Rectangle boundingBox, UInt32 attributes, out Rectangle outRectangle, Config.StandingType standingType = Config.StandingType.Player)
        {
            boundingBox.X -= (int)mapOffset.X;
            boundingBox.Y -= (int)mapOffset.Y;

            outRectangle = new Rectangle(0,0,0,0);

            int startRow = boundingBox.Top / Config.Tile.SIZE;
            int endRow = (boundingBox.Bottom - 1) / Config.Tile.SIZE;
            int startColumn = boundingBox.Left / Config.Tile.SIZE;
            int endColumn = (boundingBox.Right - 1) / Config.Tile.SIZE;
            
            for (int i = startRow; i <= endRow; i++)
            {
                if (i < 0 || i >= Config.Map.HEIGHT)
                    continue;
                
                for (int j = startColumn; j <= endColumn; j++)
                {
                    if (j < 0 || j >= Config.Map.WIDTH)
                        continue;

                    int index = i * Config.Map.WIDTH + j;
                    if (index > tileMapa.Length - 1 || index < 0)
                        continue;
                    
                    if ((tileMapa[i * Config.Map.WIDTH + j].attributes & attributes) > 0)
                    {
                        outRectangle = tileMapa[i * Config.Map.WIDTH + j].boundingBox;
                        if (tileMapa[index] is CrushyTile)
                            ((CrushyTile)tileMapa[index]).StandOn(standingType);
                        
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckPlatformCollisions(Rectangle boundingBox, out Rectangle outRectangle, out float movingModifier, out bool springy, Config.StandingType standingType = Config.StandingType.Player)
        {
            List<Rectangle> boundingBoxes = new List<Rectangle>();            
            boundingBoxes.Add(new Rectangle(boundingBox.X - (int)mapOffset.X, boundingBox.Y - (int)mapOffset.Y,
                                            boundingBox.Width, boundingBox.Height));
            if (moving)
                boundingBoxes.Add(new Rectangle(boundingBox.X - (int)(mapOffset.X + Config.Tile.SIZE * Config.Map.WIDTH), boundingBox.Y - (int)mapOffset.Y,
                                            boundingBox.Width, boundingBox.Height));

            Rectangle tileRectangle;
            outRectangle = new Rectangle(0, 0, 0, 0);
            movingModifier = 0.0f;
            springy = false;

            foreach (Rectangle bbox in boundingBoxes)
            {
                var tempBoundingBox = bbox;
                tempBoundingBox.Y += (tempBoundingBox.Height - 1);
                tempBoundingBox.Height = 1;

                int row = tempBoundingBox.Top / Config.Tile.SIZE;
                int endRow = (tempBoundingBox.Bottom - 1) / Config.Tile.SIZE;
                int startColumn = tempBoundingBox.Left / Config.Tile.SIZE;
                int endColumn = (tempBoundingBox.Right - 1) / Config.Tile.SIZE;


                for (int j = startColumn; j <= endColumn; j++)
                {
                    int index = row * Config.Map.WIDTH + j;
                    if (index > tileMapa.Length - 1 || index < 0)
                        continue;

                    tileRectangle = tileMapa[index].boundingBox;
                    tileRectangle.Height = 1;

                    if (((tileMapa[index].attributes & Tile.Attributes.Platform) > 0) &&
                        tempBoundingBox.Intersects(tileRectangle))
                    {
                        outRectangle = tileMapa[index].boundingBox;
                        if (tileMapa[index] is CrushyTile)
                            ((CrushyTile)tileMapa[index]).StandOn(standingType);

                        else if (tileMapa[index] is MovingTile)
                            movingModifier = ((MovingTile)tileMapa[index]).movingSpeed;

                        else if (tileMapa[index] is SpringTile && standingType == Config.StandingType.Player)                        
                            springy = ((SpringTile)tileMapa[index]).StandOn();
                        
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckCollisionsPixelPerfectPlayer(Player player, UInt32 attributes)
        {
            int startRow = player.boundingBox.Top / Config.Tile.SIZE;
            int endRow = (player.boundingBox.Bottom - 1) / Config.Tile.SIZE;
            int startColumn = player.boundingBox.Left / Config.Tile.SIZE;
            int endColumn = (player.boundingBox.Right - 1) / Config.Tile.SIZE;

            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    int index = i * Config.Map.WIDTH + j;
                    if (index > tileMapa.Length - 1 || index < 0)
                        continue;

                    if ((tileMapa[i * Config.Map.WIDTH + j].attributes & attributes) > 0)
                    {
                        if (CheckCollisionPixelPerfect(player.boundingBox, tileMapa[i * Config.Map.WIDTH + j].boundingBox, 
                                                       player.GetCurrentFrameTexture(), tileMapa[i * Config.Map.WIDTH + j].GetCurrentFrameTexture()))
                            return true;
                    }
                }
            }
            return false;
        }

        public bool CheckCollisionsPixelPerfectPixel(Rectangle boundingBox, UInt32 attributes)
        {
            return true;
        }

        public bool CheckCollisionPixelPerfect(Rectangle rect1, Rectangle rect2, Texture2D texture1, Texture2D texture2)
        {
            Rectangle shareRect;
            
            shareRect = Util.GetSharedRectangle(rect1, rect2);
            if (shareRect.Width == 0 || shareRect.Height == 0)
                return false;

            int colorSize = shareRect.Height * shareRect.Width;
            Color[] texture1Colors = new Color[colorSize];
            Color[] texture2Colors = new Color[colorSize];

            Rectangle sourceRectTexture1 = Util.NormalizeToBase(shareRect, rect1);
            Rectangle sourceRectTexture2 = Util.NormalizeToBase(shareRect, rect2);

            texture1.GetData<Color>(0, sourceRectTexture1, texture1Colors, 0, colorSize);
            texture2.GetData<Color>(0, sourceRectTexture2, texture2Colors, 0, colorSize);

            for (int i = 0; i < colorSize; i++)
            {
                if (texture1Colors[i].A == 255 && texture2Colors[i].A == 255)
                    return true;
            }

            return false;
        }

        public bool GrabCollectibles(Player player, GraphicsDeviceManager graphic)
        {
            List<Rectangle> boundingBoxes = new List<Rectangle>();          
            boundingBoxes.Add(new Rectangle(player.boundingBox.X - (int)mapOffset.X, player.boundingBox.Y - (int)mapOffset.Y,
                                            player.boundingBox.Width, player.boundingBox.Height));
            if (moving)
                boundingBoxes.Add(new Rectangle(player.boundingBox.X - (int)(mapOffset.X + Config.Tile.SIZE * Config.Map.WIDTH), player.boundingBox.Y - (int)mapOffset.Y,
                                            player.boundingBox.Width, player.boundingBox.Height));

             foreach (Rectangle playerbbox in boundingBoxes)
            {
                int startRow = playerbbox.Top / Config.Tile.SIZE;
                int endRow = (playerbbox.Bottom - 1) / Config.Tile.SIZE;
                int startColumn = playerbbox.Left / Config.Tile.SIZE;
                int endColumn = (playerbbox.Right - 1) / Config.Tile.SIZE;

                for (int i = startRow; i <= endRow; i++)
                {
                    for (int j = startColumn; j <= endColumn; j++)
                    {
                        int index = i * Config.Map.WIDTH + j;
                        if (index > tileMapa.Length - 1 || index < 0)
                            continue;

                        if ((tileMapa[index].attributes & Tile.Attributes.Collectible) > 0)
                        {
                            // optimize bounding box
                            Rectangle bbox = tileMapa[index].boundingBox;
                            bbox.X += 1;
                            bbox.Width -= 2;
                            if (playerbbox.Intersects(bbox))
                            {
                                if (--collectiblesCount == 0)
                                {
                                    OpenDoor();
                                    foreach (Enemy enemy in enemiesList)
                                        enemy.TriggerGuardian();
                                }
                                tileMapa[index].SetAttributes(Tile.Attributes.NoDraw);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool KillThisBastard(Player player, GraphicsDeviceManager graphic)
        {
            if (player.GetState(Player.State.dead))
                return false;
            
            foreach (Enemy enemy in enemiesList)
            {
                if (enemy.boundingBox.Intersects(player.boundingBox))
                {
                    return CheckCollisionPixelPerfect(player.boundingBox, enemy.boundingBox, player.GetCurrentFrameTexture(), enemy.GetCurrentFrameTexture(graphic));
                }
            }

            EmiterPart emiterPartRectangle = new EmiterPart();
            foreach (Emiter emiter in emiterList)
            {
                if (emiter.CheckCollision(player.boundingBox, out emiterPartRectangle))
                {
                    return CheckCollisionPixelPerfect(player.boundingBox, emiterPartRectangle.boundingBox, player.GetCurrentFrameTexture(), emiterPartRectangle.GetCurrentFrameTexture(graphic));
                }
            }
            return (CheckCollisionsPixelPerfectPlayer(player, Tile.Attributes.Killing));
        }

        public void OpenDoor()
        {
            foreach (Tile tile in tileMapa)
            {
                if ((tile.attributes & Tile.Attributes.Doors) > 0)
                    tile.SetColor(Color.Red);
            }
        }

        public bool EnteredDoors(Rectangle boundingBox)
        {
            return (CheckCollisions(boundingBox, Tile.Attributes.DoorsMain) && collectiblesCount == 0);
        }

        public static Map LoadMap(string directory, string xmlFile, GraphicsDeviceManager graphics, ContentManager content, GameStateManager gameStateManager, Hud hud, float scale = 1.0f)
        {
            Texture2D pixel = content.Load<Texture2D>("pixel");
            Texture2D tileset = null;
            List<Enemy> enemiesList = new List<Enemy>();
            List<Emiter> emiterList = new List<Emiter>();
            List<Trigger> triggerList = new List<Trigger>();
            int[] mapa = new int[Config.Map.WIDTH * Config.Map.HEIGHT];
            string levelName = "";
            bool upsidedown = false;
            bool moving = false;
            int triggerCount = 0;

            using (XmlReader xmlreader = XmlReader.Create(TitleContainer.OpenStream(@"Levels\" + xmlFile)))
            {
                while (xmlreader.Read())
                {
                    if (xmlreader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlreader.Name)
                        {
                            case "map":
                                xmlreader.ReadToFollowing("property");
                                do
                                {
                                    //xmlreader.MoveToNextAttribute();
                                    //if (xmlreader.Name == "name" && xmlreader.Value == "name")
                                    //{
                                    //    xmlreader.MoveToNextAttribute();
                                    //    if (xmlreader.Name == "value")
                                    //        levelName = xmlreader.Value;
                                    //}

                                    xmlreader.MoveToNextAttribute();
                                    string property_name = "";
                                    if (xmlreader.Name == "name")
                                        property_name = xmlreader.Value;

                                    xmlreader.MoveToNextAttribute();
                                    string value = "";
                                    if (xmlreader.Name == "value")
                                        value = xmlreader.Value;

                                    switch (property_name)
                                    {
                                        case "name":
                                            levelName = value;
                                            break;
                                        case "upsidedown":
                                            upsidedown = (int.Parse(value) == 1 ? true : false);
                                            break;
                                        case "moving":
                                            moving = (int.Parse(value) == 1 ? true : false);
                                            break;
                                            
                                    }
                                } while (xmlreader.ReadToNextSibling("property"));
                                break;

                            // Tilelayer
                            case "layer":
                                xmlreader.ReadToFollowing("property");
                                xmlreader.MoveToNextAttribute();
                                if (xmlreader.Name == "name" && xmlreader.Value == "texture")
                                {
                                    xmlreader.MoveToNextAttribute();
                                    if (xmlreader.Name == "value")
                                        tileset = content.Load<Texture2D>(directory + "\\" + xmlreader.Value);
                                }
                                xmlreader.ReadToFollowing("data");
                                string[] dataStrings = xmlreader.ReadElementContentAsString().Split(',');
                                for (int i = 0; i < Config.Map.WIDTH * Config.Map.HEIGHT; i++)
                                {
                                    try
                                    {
                                        mapa[i] = Int32.Parse(dataStrings[i], CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception ex)
                                    {
                                        string j = ex.Message;
                                    }
                                }
                                    
                                break;

                            // enemies and emmiters
                            case "objectgroup":     
                                int type = 0;
                                float speedx = 0.0f;
                                float speedy = 0.0f;
                                float speed = 0.0f;
                                int sizex = 0;
                                int sizey = 0;
                                int delay = 0;
                                int offset = 0;
                                Texture2D texture = null;

                                xmlreader.ReadToFollowing("properties");

                                while (xmlreader.Read())
                                {
                                    if (xmlreader.NodeType == XmlNodeType.EndElement &&
                                        xmlreader.Name == "properties")
                                        break;

                                    else if (xmlreader.NodeType == XmlNodeType.Element &&
                                        xmlreader.Name == "property")
                                    {
                                        xmlreader.MoveToNextAttribute();
                                        string name = "";
                                        if (xmlreader.Name == "name")
                                            name = xmlreader.Value;                                
                                        
                                        xmlreader.MoveToNextAttribute();
                                        string value = "";
                                        if (xmlreader.Name == "value")
                                            value = xmlreader.Value;
                                    
                                        switch (name)
                                        {
                                            case "_type":
                                                type = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "speed":
                                                speed = float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "speedx":
                                                speedx = float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "speedy":
                                                speedy = float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "size":
                                                sizex = sizey = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "sizex":
                                                sizex = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "sizey":
                                                sizey = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "texture":
                                                texture = content.Load<Texture2D>(directory + "\\" + value);
                                                break;

                                            case "delay":
                                                delay = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                break;

                                            case "offset":
                                                offset = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                break;
                                        } 
                                    }
                                }

                                while (xmlreader.Read())
                                {
                                    if (xmlreader.NodeType == XmlNodeType.EndElement &&
                                        xmlreader.Name == "objectgroup")
                                        break;

                                    if (xmlreader.NodeType == XmlNodeType.Element &&
                                        xmlreader.Name == "object")
                                    {
                                        string name = "";
                                        int triggerORtextureType, x, y, width, height;
                                        triggerORtextureType = x = y = width = height = 0;
                                        bool reverse = true;
                                        bool blink = false;
                                        bool guardian = false;
                                        bool explode = false;
                                        int blinkTime = Config.Enemy.DEFAULT_BLINK_TIME_MS;
                                        float localspeedx, localspeedy, localspeed;
                                        localspeedx = localspeedy = localspeed = 0.0f;
                                        int localoffset, localdelay;
                                        localoffset = -1;
                                        localdelay = 0;
                                        int delay2 = 0;
                                        int numofparts = 1;
                                        int wait = 0;
                                        while (xmlreader.MoveToNextAttribute())
                                        {
                                            if (xmlreader.Name == "name")
                                                name = xmlreader.Value;
                                            else if (xmlreader.Name == "x")
                                                x = (int)float.Parse(xmlreader.Value, CultureInfo.InvariantCulture);
                                            else if (xmlreader.Name == "y")
                                                y = (int)float.Parse(xmlreader.Value, CultureInfo.InvariantCulture);
                                            else if (xmlreader.Name == "width")
                                                width = (int)float.Parse(xmlreader.Value, CultureInfo.InvariantCulture);
                                            else if (xmlreader.Name == "height")
                                                height = (int)float.Parse(xmlreader.Value, CultureInfo.InvariantCulture);
                                            else if (xmlreader.Name == "type")
                                                triggerORtextureType = (int)float.Parse(xmlreader.Value, CultureInfo.InvariantCulture);
                                        }
                                        if (type == Config.LayerType.TRIGGER)
                                        {
                                            triggerList.Add(new Trigger(new Rectangle(x, y, width, height), 1, (Config.TriggerType)triggerORtextureType, gameStateManager));
                                            triggerList[triggerCount].SetStateID(Config.States.TEXT + triggerCount);
                                            TextState textState = new TextState(graphics, content, gameStateManager, hud);
                                            textState.scale = scale;
                                            textState.LoadTextLines(directory, name);
                                            gameStateManager.RegisterState(Config.States.TEXT + triggerCount, textState);
                                            triggerCount++;
                                            continue;
                                        }

                                        xmlreader.MoveToContent();
                                        while (xmlreader.Read())
                                        {
                                            if (xmlreader.NodeType == XmlNodeType.Element &&
                                                xmlreader.Name == "properties")
                                            {
                                                xmlreader.ReadToDescendant("property");
                                                do
                                                {
                                                    xmlreader.MoveToNextAttribute();
                                                    string property_name = "";
                                                    if (xmlreader.Name == "name")
                                                        property_name = xmlreader.Value;

                                                    xmlreader.MoveToNextAttribute();
                                                    string value = "";
                                                    if (xmlreader.Name == "value")
                                                        value = xmlreader.Value;

                                                    switch (property_name)
                                                    {
                                                        case "reverse":
                                                            reverse = (int.Parse(value) == 1) ? true : false;
                                                            break;
                                                        case "blink":
                                                            blink = (int.Parse(value) == 1) ? true : false;
                                                            break;
                                                        case "blinktime":
                                                            blinkTime = (int.Parse(value, CultureInfo.InvariantCulture));
                                                            break;
                                                        case "guardian":
                                                            guardian = (int.Parse(value) == 1) ? true : false;
                                                            break;

                                                        case "explode":
                                                            explode = (int.Parse(value) == 1) ? true : false;
                                                            break;
                                                        case "speed":
                                                            localspeed = float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;

                                                        case "speedx":
                                                            localspeedx = float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;

                                                        case "speedy":
                                                            localspeedy = float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;

                                                        case "delay":
                                                            localdelay = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;

                                                        case "offset":
                                                            localoffset = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;

                                                        case "delay2":
                                                            delay2 = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;

                                                        case "num":
                                                            numofparts = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;

                                                        case "wait":
                                                            wait = (int)float.Parse(value, CultureInfo.InvariantCulture);
                                                            break;
                                                        }
                                                    } while (xmlreader.ReadToNextSibling("property"));
                                                }
                                                else if (xmlreader.NodeType == XmlNodeType.Element &&
                                                    xmlreader.Name == "polyline") 
                                                {
                                                    if (xmlreader.MoveToNextAttribute() && xmlreader.Name == "points")
                                                    {
                                                        string[] points = xmlreader.Value.Split(' ');
                                                        string[] coords;
                                                        int moveX, moveY;
                                                        var pivotShift = new Vector2(sizex / 2, sizey / 2);
                                                        var startPosition = new Vector2(x, y);

                                                        switch (type)
                                                        {
                                                            case Config.LayerType.ENEMY:
                                                                coords = points[0].Split(',');
                                                                moveX = (int)float.Parse(coords[0], CultureInfo.InvariantCulture);
                                                                moveY = (int)float.Parse(coords[1], CultureInfo.InvariantCulture);

                                                                if (Config.CENTER_PIVOT)
                                                                    startPosition -= pivotShift;

                                                                Vector2 speedVector = new Vector2(speedx, speedy);                                                                
                                                                if (localspeedx != 0.0f)
                                                                    speedVector.X = localspeedx;
                                                                if (localspeedy != 0.0f)
                                                                    speedVector.Y = localspeedy;
                                                                if (localoffset <= 100 && localoffset >= 0)
                                                                    offset = localoffset;

                                                                enemiesList.Add(new Enemy(texture,
                                                                                speedVector,
                                                                                new Vector2(sizex, sizey),
                                                                                triggerORtextureType,
                                                                                startPosition + new Vector2(moveX, moveY),
                                                                                reverse, blink, guardian, offset, wait));
                                                                enemiesList.Last().SetBlinkTime(blinkTime);
                                                                
                                                                for (int i = 1; i < points.Length; i++)
                                                                {
                                                                    coords = points[i].Split(',');
                                                                    moveX = (int)float.Parse(coords[0], CultureInfo.InvariantCulture);
                                                                    moveY = (int)float.Parse(coords[1], CultureInfo.InvariantCulture);

                                                                    var movePoint = startPosition + new Vector2(moveX, moveY);
                                                                    enemiesList.Last().AddMovepoint(movePoint);
                                                                }
                                                                enemiesList.Last().Init();                                                                
                                                                //              ,
                                                                //new Vector2(x + moveX, y + moveY),
                                                                break;

                                                            case Config.LayerType.EMITER:                                                                                                                        
                                                                MovementDirection direction;
                                                                uint distance = 0;
                                                                coords = points[1].Split(',');
                                                                moveX = (int)float.Parse(coords[0], CultureInfo.InvariantCulture);
                                                                moveY = (int)float.Parse(coords[1], CultureInfo.InvariantCulture);

                                                                if (moveX > 0)
                                                                {
                                                                    direction = MovementDirection.Right;
                                                                    distance = (uint)moveX;
                                                                }
                                                                else if (moveX < 0)
                                                                {
                                                                    direction = MovementDirection.Left;
                                                                    distance = (uint)Math.Abs(moveX);
                                                                }
                                                                else if (moveY > 0)
                                                                {
                                                                    direction = MovementDirection.Down;
                                                                    distance = (uint)moveY;
                                                                }
                                                                else
                                                                {
                                                                    direction = MovementDirection.Up;
                                                                    distance = (uint)Math.Abs(moveY);
                                                                }

                                                                //if (Config.CENTER_PIVOT)
                                                                  //  startPosition -= pivotShift;

                                                                float emiterSpeed = speed;
                                                                int emiterDelay = delay;
                                                                int emiterOffset = offset;
                                                                if (localspeed != 0.0f)
                                                                    emiterSpeed = localspeed;
                                                                if (localdelay != 0)
                                                                    emiterDelay = localdelay;
                                                                if (localoffset != 0)
                                                                    emiterOffset = localoffset;

                                                                emiterList.Add(new Emiter(texture,
                                                                               startPosition,
                                                                               distance, emiterSpeed, direction,
                                                                               new Rectangle(triggerORtextureType * sizex, 0, sizex, sizey),
                                                                               emiterDelay, emiterOffset,
                                                                               Color.White, explode, delay2, numofparts));
                                                                break;
                                                        }
                                                    }
                                                break;
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

            if (tileset == null)
                return null;

            return new Map(tileset, pixel, mapa, enemiesList, emiterList, triggerList, levelName, upsidedown, moving);
        }
    }
}
