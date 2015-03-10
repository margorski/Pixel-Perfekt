using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    class Hud
    {
        SpriteFont spriteFont;
        String levelName = "";
        List<Tile> collectibleTiles = new List<Tile>();
        List<Tile> collectedTiles = new List<Tile>();

        public bool enabled = true;

        public Hud(SpriteFont spriteFont) 
        {
            this.spriteFont = spriteFont;
        }

        public void Init(string levelName, int collectiblesCount)
        {
            this.levelName = levelName;

            collectibleTiles.Clear();
            collectedTiles.Clear();

            for (int i = 0; i < collectiblesCount; i++)
            {
                collectibleTiles.Add(TileFactory.CreateTile((int)Config.TileType.KEY, new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + Config.Hud.COLLECTIBLES_SPACE + i * Config.Hud.COLLECTIBLES_SPACE, Config.Hud.COLLECTIBLES_Y)));
                ((CollectibleTile)collectibleTiles[i]).Deactivate();
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (Tile tile in collectibleTiles)
                tile.Update(gameTime);

            foreach (Tile tile in collectedTiles)
                tile.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!enabled)
                return;

            Util.DrawStringAligned(spriteBatch, "Time: " + Globals.CurrentLevelState.levelTime.ToString("mm\\:ss\\.f"), spriteFont, Color.White,
                        new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
                        new Vector2(4, Config.Hud.TEXT_POSITION_Y), Util.Align.Left);
             
            Util.DrawStringAligned(spriteBatch, levelName, spriteFont, Color.White,
                                   new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
                                   new Vector2(0, Config.Hud.TEXT_POSITION_Y), Util.Align.Center);

            Util.DrawStringAligned(spriteBatch, "DEATHS: " + Globals.CurrentLevelState.deathCount, spriteFont, Color.White,
                        new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
                        new Vector2(0, Config.Hud.TEXT_POSITION_Y), Util.Align.Right);
             
            foreach (Tile tile in collectibleTiles)
                tile.Draw(spriteBatch, Vector2.Zero);

            foreach (Tile tile in collectedTiles)
                tile.Draw(spriteBatch, Vector2.Zero);
        }

        public void Collect()
        {
            collectibleTiles.RemoveAt(collectibleTiles.Count - 1);
            collectedTiles.Add(TileFactory.CreateTile((int)Config.TileType.KEY,
                                                      new Vector2(Config.SCREEN_WIDTH_SCALED / 2 - (collectedTiles.Count + 2) * (Config.Hud.COLLECTIBLES_SPACE), Config.Hud.COLLECTIBLES_Y)));
        }

        public void SetText(string text)
        {
            levelName = text;
        }
    }
}
