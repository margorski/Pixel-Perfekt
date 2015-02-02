using System;
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

        // Public
        public Vector2 startPosition { private set; get; }
        public byte collectiblesCount { private set;  get; }
        public string levelName { private set; get; }

        // Methods
        public Map(Texture2D tileset, Texture2D pixel, int[] mapa, List<Enemy> enemiesList, List<Emiter> emiterList, List<Trigger> triggerList, string levelName)
        {
            this.mapa = mapa;
            this.enemiesList = enemiesList;
            this.emiterList = emiterList;
            this.triggerList = triggerList;
            this.levelName = levelName;

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
                tile.Draw(spriteBatch);
            foreach (Enemy enemy in enemiesList)
                enemy.Draw(spriteBatch);
            foreach (Emiter emiter in emiterList)
                emiter.Draw(spriteBatch);
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
            int startRow = boundingBox.Top / Config.Tile.SIZE;
            int endRow = (boundingBox.Bottom - 1) / Config.Tile.SIZE;
            int startColumn = boundingBox.Left / Config.Tile.SIZE;
            int endColumn = (boundingBox.Right - 1) / Config.Tile.SIZE;

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
            return false;
        }

        public bool CheckCollisions(Rectangle boundingBox, UInt32 attributes, out Rectangle outRectangle, Config.StandingType standingType = Config.StandingType.Player)
        {
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

        public bool CheckPlatformCollisions(Rectangle boundingBox, out Rectangle outRectangle, out float movingModifier, Config.StandingType standingType = Config.StandingType.Player)
        {
            Rectangle tileRectangle;
            outRectangle = new Rectangle(0, 0, 0, 0);
            movingModifier = 0.0f;
            boundingBox.Y += (boundingBox.Height - 1);
            boundingBox.Height = 1;

            int row = boundingBox.Top / Config.Tile.SIZE;    
            int endRow = (boundingBox.Bottom - 1) / Config.Tile.SIZE;
            int startColumn = boundingBox.Left / Config.Tile.SIZE;
            int endColumn = (boundingBox.Right - 1) / Config.Tile.SIZE;


            for (int j = startColumn; j <= endColumn; j++)
            {
                int index = row * Config.Map.WIDTH + j;
                if (index > tileMapa.Length - 1 || index < 0)
                    continue;

                tileRectangle = tileMapa[index].boundingBox;
                tileRectangle.Height = 1;

                if (((tileMapa[index].attributes & Tile.Attributes.Platform) > 0) &&
                    boundingBox.Intersects(tileRectangle))
                {
                    outRectangle = tileMapa[index].boundingBox;
                    if (tileMapa[index] is CrushyTile)
                        ((CrushyTile)tileMapa[index]).StandOn(standingType);

                    if (tileMapa[index] is MovingTile)
                        movingModifier = ((MovingTile)tileMapa[index]).movingSpeed;

                    return true;
                }
            }
            return false;
        }

        public bool CheckCollisionsPixelPerfect(Player player, UInt32 attributes, GraphicsDeviceManager graphic)
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
                                                       player.GetCurrentFrameTexture(graphic), tileMapa[i * Config.Map.WIDTH + j].GetCurrentFrameTexture(graphic)))
                            return true;
                    }
                }
            }
            return false;
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

                    if ((tileMapa[index].attributes & Tile.Attributes.Collectible) > 0)
                    {
                        // optimize bounding box
                        Rectangle bbox = tileMapa[index].boundingBox;
                        bbox.X += 1;
                        bbox.Width -= 2;                                                
                        if (player.boundingBox.Intersects(bbox)) 
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
                    return CheckCollisionPixelPerfect(player.boundingBox, enemy.boundingBox, player.GetCurrentFrameTexture(graphic), enemy.GetCurrentFrameTexture(graphic));
                }
            }

            EmiterPart emiterPartRectangle = new EmiterPart();
            foreach (Emiter emiter in emiterList)
            {
                if (emiter.CheckCollision(player.boundingBox, out emiterPartRectangle))
                {
                    return CheckCollisionPixelPerfect(player.boundingBox, emiterPartRectangle.boundingBox, player.GetCurrentFrameTexture(graphic), emiterPartRectangle.GetCurrentFrameTexture(graphic));
                }
            }
            return (CheckCollisionsPixelPerfect(player, Tile.Attributes.Killing, graphic));
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
                                xmlreader.MoveToNextAttribute();
                                if (xmlreader.Name == "name" && xmlreader.Value == "name")
                                {
                                    xmlreader.MoveToNextAttribute();
                                    if (xmlreader.Name == "value")
                                        levelName = xmlreader.Value;
                                }
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
                                string texture = "";
                                int sizex = 0;
                                int sizey = 0;
                                int delay = 0;
                                int offset = 0;
                                
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
                                                texture = value;
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
                                        int blinkTime = Config.Enemy.DEFAULT_BLINK_TIME_MS;

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

                                                            enemiesList.Add(new Enemy(content.Load<Texture2D>(directory + "\\" + texture),
                                                                            new Vector2(speedx, speedy),
                                                                            new Vector2(sizex, sizey),
                                                                            triggerORtextureType,
                                                                            startPosition + new Vector2(moveX, moveY),
                                                                            reverse, blink, guardian));
                                                            enemiesList.Last().SetBlinkTime(blinkTime);
                                                            for (int i = 1; i < points.Length; i++)
                                                            {
                                                                coords = points[i].Split(',');
                                                                moveX = (int)float.Parse(coords[0], CultureInfo.InvariantCulture);
                                                                moveY = (int)float.Parse(coords[1], CultureInfo.InvariantCulture);

                                                                var movePoint = startPosition + new Vector2(moveX, moveY);
                                                                enemiesList.Last().AddMovepoint(movePoint);
                                                            }
                                                            enemiesList.Last().PrepareGuardian();
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

                                                            emiterList.Add(new Emiter(content.Load<Texture2D>(directory + "\\" + texture),
                                                                           startPosition,
                                                                           distance, speed, direction,
                                                                           new Rectangle(triggerORtextureType * sizex, 0, sizex, sizey),
                                                                           delay, offset,
                                                                           Color.White));
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

            return new Map(tileset, pixel, mapa, enemiesList, emiterList, triggerList, levelName);
        }
    }
}
