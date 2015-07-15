using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace PixelPerfect
{
    class Hud
    {
        String levelName = "";
        List<Tile> collectibleTiles = new List<Tile>();
        List<Tile> collectedTiles = new List<Tile>();

        public bool enabled = true;

        private WavyText leveltitle;
        //private Texture2D hudTexture;
        private Color color = Color.Black;
        public Hud() {}

        public void Init(string levelName, int collectiblesCount, Color color)
        {
            this.levelName = levelName;
            var titlex = Config.SCREEN_WIDTH_SCALED / 2 - Globals.silkscreenFont.MeasureString(levelName).X / 2;
            leveltitle = new WavyText(levelName, new Vector2(titlex, 137), 1500, 1.0f, Config.titleColors, 12.0f, 1.0f, 0.0f);

            collectibleTiles.Clear();
            collectedTiles.Clear();
            this.color = color;
            for (int i = 0; i < collectiblesCount; i++)
            {
                collectibleTiles.Add(TileFactory.CreateTile((int)Config.TileType.KEY, new Vector2(Config.SCREEN_WIDTH_SCALED / 2 + Config.Hud.COLLECTIBLES_SPACE + i * Config.Hud.COLLECTIBLES_SPACE, Config.Hud.COLLECTIBLES_Y)));
                ((CollectibleTile)collectibleTiles[i]).Deactivate();
            }
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }

        public void Update(GameTime gameTime)
        {
            leveltitle.Update(gameTime);

            foreach (Tile tile in collectibleTiles)
                tile.Update(gameTime);

            foreach (Tile tile in collectedTiles)
                tile.Update(gameTime);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!enabled)
                return;


            //if (hudTexture != null)
            //    spriteBatch.Draw(hudTexture, new Vector2(0, Config.SCREEN_HEIGHT_SCALED - Config.Hud.HUD_HEIGHT), Color.White);

            Levelsave levelsave;
            Savestate.Instance.levelSaves.TryGetValue(Globals.worlds[Globals.selectedWorld].GetLevelFile(Globals.selectedLevel), out levelsave);

            spriteBatch.Draw(Globals.textureDictionary["pixel"], new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - Config.Hud.HUD_HEIGHT, Config.SCREEN_WIDTH_SCALED + 20, Config.SCREEN_HEIGHT_SCALED), color);

            spriteBatch.Draw(Globals.textureDictionary["clock"], new Vector2(6, Config.Hud.TEXT_POSITION_Y + 3), Color.White);
            Util.DrawStringAligned(spriteBatch, "Time: " + Globals.CurrentLevelState.levelTime.ToString("mm\\:ss\\.f"), Globals.silkscreenFont, Color.White,
                        new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
                        new Vector2(17, Config.Hud.TEXT_POSITION_Y), Util.Align.Left);

            //Util.DrawStringAligned(spriteBatch, levelName, Globals.silkscreenFont, Color.White,
            //                        new Rectangle(0, Config.SCREEN_HEIGHT_SCALED - 22, Config.SCREEN_WIDTH_SCALED, Config.SCREEN_HEIGHT_SCALED),
            //                        new Vector2(0, Config.Hud.TEXT_POSITION_Y), Util.Align.Center);

            leveltitle.Draw(spriteBatch);
            spriteBatch.Draw(Globals.textureDictionary["skull"], new Vector2(200, Config.Hud.TEXT_POSITION_Y + 3), Color.White);
            spriteBatch.DrawString(Globals.silkscreenFont, "DEATHS: " + levelsave.deathCount, new Vector2(210, Config.Hud.TEXT_POSITION_Y + 3), Color.White);

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
